using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Framework
{
    public abstract class Package
    {
        public event Action<PackageItemChangedInfo> OnItemChanged;

        /// <summary>
        /// 获取指定物品的数量
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract int GetItemNum(ItemId id);

        /// <summary>
        /// 向包裹中的某个单元格添加物品，若单元格内有物品，则会作为返回值返回
        /// </summary>
        /// <param name="index">单元格下标</param>
        /// <param name="itemGroup">需要添加的物品组</param>
        /// <returns>被替换的物品</returns>
        public abstract ItemGroup PutItemInto(int index, ItemGroup itemGroup);

        /// <summary>
        /// 设置包裹中指定单元格的物品
        /// </summary>
        /// <param name="index"></param>
        /// <param name="itemGroup"></param>
        public abstract void SetItemAt(int index, ItemGroup itemGroup);

        /// <summary>
        /// 获取包裹内所有物品的信息
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<ItemGroup> GetAllItemInfo();

        /// <summary>
        /// 获取包裹中指定单元格的物品
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public abstract ItemGroup GetItemInfo(int index);

        /// <summary>
        /// 检查背包是否能完全装下指定物品
        /// </summary>
        /// <returns></returns>
        public abstract bool PutItemCheck(params ItemGroup[] groups);

        /// <summary>
        /// 将指定物品装入包裹
        /// </summary>
        /// <returns></returns>
        public abstract PackageOperation PutItem(params ItemGroup[] groups);

        /// <summary>
        /// 检查物品是否满足消耗
        /// </summary>
        /// <returns></returns>
        public abstract bool CostItemCheck(params ItemGroup[] groups);

        /// <summary>
        /// 消耗指定数量的物品
        /// </summary>
        /// <returns>操作结果</returns>
        public abstract PackageOperation CostItem(params ItemGroup[] groups);

        /// <summary>
        /// 排序物品
        /// </summary>
        public abstract void SortItem();

        /// <summary>
        /// 目标物品可否纳入背包
        /// </summary>
        public bool CanPackage(ItemId itemId)
        {
            return ItemSys.GetInfo(itemId).CanPackage;
        }

        protected virtual void InvokeOnItemCellChanged(PackageItemChangedInfo changedInfo)
        {
            OnItemChanged?.Invoke(changedInfo);
        }
    }

    public struct PackageItemChangedInfo
    {
        public IEnumerable<int> ChangedCells;
        public IEnumerable<ItemGroup> ChangedItemGroups;

        public PackageItemChangedInfo(IEnumerable<int> changedCells, IEnumerable<ItemGroup> changedItemGroups)
        {
            ChangedCells = changedCells;
            ChangedItemGroups = changedItemGroups;
        }
    }

    public struct PackageOperation
    {
        public IEnumerable<ItemGroup> Remaining;
        public PackageItemChangedInfo PackageItemChangedInfo;

        public PackageOperation(IEnumerable<ItemGroup> remaining, PackageItemChangedInfo packageItemChangedInfo)
        {
            Remaining = remaining;
            PackageItemChangedInfo = packageItemChangedInfo;
        }

        public PackageOperation(IEnumerable<ItemGroup> remaining, IEnumerable<int> changedCells, IEnumerable<ItemGroup> changedItemGroups)
        {
            Remaining = remaining;
            PackageItemChangedInfo = new PackageItemChangedInfo(changedCells,changedItemGroups);
        }
    }

    public class CommonPackage : Package
    {
        public int Capacity => _Items.Length;

        private readonly ItemGroup[] _Items;
        private readonly SortedDictionary<ItemId, PackageItemInfo> _ItemManifest
            = new SortedDictionary<ItemId, PackageItemInfo>();

        private readonly List<ItemGroup> _RemainingList 
            = new List<ItemGroup>();
        private readonly Dictionary<ItemId, int> _ItemChangedDic
            = new Dictionary<ItemId, int>();
        private readonly List<int> _ItemCellChangedList
            = new List<int>();

        private readonly int[] _LimitItems;

        public CommonPackage(int cellNum, params int[] limitItems)
        {
            if (cellNum <= 0)
                throw new Exception("背包容量需要大于0");

            _Items = new ItemGroup[cellNum];

            _LimitItems = limitItems;
        }

        public override int GetItemNum(ItemId id)
        {
            return _ItemManifest.TryGetValue(id, out var result) ? result.Count : 0;
        }

        public override ItemGroup PutItemInto(int index, ItemGroup itemGroup)
        {
            itemGroup = itemGroup.ToValid();

            if (!LimitItem(itemGroup.Id) || !CanPackage(itemGroup.Id)) // 不能放入背包则不能添加
                return itemGroup;

            var maxStack = ItemSys.GetInfo(itemGroup.Id).MaxStackNum;

            if (_Items[index].Id == 0)
                _Items[index].Id = itemGroup.Id;

            // 源物品与目标物品不同则进行替换
            if (_Items[index].Id != itemGroup.Id)
            {
                // 如果源物品数量大于堆叠数，则无法将源物品全部放入单元格内进行替换
                // 所以在这种情况下判定为无法添加物品
                if (itemGroup.Count > maxStack)
                    return itemGroup;

                var takeOut = _Items[index];
                SetItemAt(index, itemGroup);
                return takeOut;
            }

            // 为空或是物品相同则直接添加

            // 源物品和目标物品均为空则不做修改
            if (itemGroup.Id == 0) return new ItemGroup();

            var take = Mathf.Min(maxStack - _Items[index].Count, itemGroup.Count);
            _Items[index].Count += take;
            itemGroup.Count -= take;

            if (take != 0)
            {
                _ItemCellChangedList.Clear();
                _ItemCellChangedList.Add(index);
                _ItemChangedDic.Clear();
                _ItemChangedDic.ModifyIntDic(itemGroup.Id, take);

                ModifyManifest(itemGroup.Id, take);

                InvokeOnItemCellChanged(
                    new PackageItemChangedInfo(
                        _ItemCellChangedList,
                        ToChangedItemGroups(_ItemChangedDic)));
            }

            return itemGroup.Count == 0 ? new ItemGroup() : itemGroup;
        }

        public override void SetItemAt(int index, ItemGroup itemGroup)
        {
            itemGroup = itemGroup.ToValid();

            _ItemChangedDic.Clear();
            _ItemCellChangedList.Clear();

            var desItem = _Items[index];
            if (!desItem.Equals(itemGroup))
                _ItemCellChangedList.Add(index);

            if (desItem.Id != 0)
                ModifyManifest(desItem.Id,-desItem.Count);

            _Items[index] = itemGroup;
            if (itemGroup.Id != 0)
                _ItemChangedDic.ModifyIntDic(itemGroup.Id, itemGroup.Count);

            ModifyManifest(itemGroup.Id, itemGroup.Count);

            if(_ItemCellChangedList.Count!=0)
                InvokeOnItemCellChanged(
                    new PackageItemChangedInfo(
                        _ItemCellChangedList,
                        ToChangedItemGroups(_ItemChangedDic)));
        }

        public override IEnumerable<ItemGroup> GetAllItemInfo()
        {
            return _Items;
        }

        public override ItemGroup GetItemInfo(int index)
        {
            return _Items[index];
        }

        public override bool PutItemCheck(params ItemGroup[] groups)
        {
            _ItemChangedDic.Clear();
            _RemainingList.Clear();
            for (int i = 0; i < groups.Length; i++)
            {
                var g = groups[i].ToValid();
                if (!LimitItem(g.Id)) return false;
                if (g.Id == 0) continue;
                _RemainingList.Add(g);
                _ItemChangedDic.Add(g.Id, _RemainingList.Count - 1);
            }


            if (_RemainingList.Count == 0)
                return true;

            var startIndex = 0;
            var curIndex = 0;
            var remainingItems = 0;
            do
            {
                var curCell = _Items[curIndex];

                if (curCell.Id == 0)
                    curCell.Id = _RemainingList[remainingItems].Id;

                if (_ItemChangedDic.TryGetValue(curCell.Id, out var num))
                {
                    var maxStack = ItemSys.GetInfo(curCell.Id).MaxStackNum;
                    var g = _RemainingList[num];
                    if (g.Count != 0)
                    {
                        var take = Mathf.Min(g.Count, maxStack - curCell.Count);
                        g.Count -= take;
                        _RemainingList[num] = g;
                        if (g.Count == 0 && num == remainingItems)
                            remainingItems = FindNextAvailableGroup(_RemainingList, num);
                    }
                }

                curIndex = (curIndex + 1) % _Items.Length;

            } while (remainingItems < _RemainingList.Count && startIndex != curIndex);

            return remainingItems >= _RemainingList.Count;
        }

        public override PackageOperation PutItem(params ItemGroup[] groups)
        {
            _ItemCellChangedList.Clear();
            _ItemChangedDic.Clear();
            _RemainingList.Clear();
            for (int i = 0; i < groups.Length; i++)
            {
                var g = groups[i].ToValid();
                if (g.Id == 0 || !LimitItem(g.Id)) continue;
                _RemainingList.Add(g);
                _ItemChangedDic.Add(g.Id, _RemainingList.Count - 1);
            }

            var result = new PackageOperation(
                JumpOverInvalidItem(_RemainingList),
                _ItemCellChangedList,
                ToChangedItemGroups(_ItemChangedDic));

            if (_RemainingList.Count == 0)
                return result;
            
            var curCellIndex = 0;
            var remainingFirst = 0;
            do
            {
                var curCell = _Items[curCellIndex];

                if (curCell.Id == 0)
                    curCell.Id = _RemainingList[remainingFirst].Id;

                if (_ItemChangedDic.TryGetValue(curCell.Id, out var curSrcItemIndex))
                {
                    var maxStack = ItemSys.GetInfo(curCell.Id).MaxStackNum;
                    var curSrcItem = _RemainingList[curSrcItemIndex];
                    if (curSrcItem.Count != 0)
                    {
                        var take = Mathf.Min(curSrcItem.Count, maxStack - curCell.Count);
                        curSrcItem.Count -= take;
                        curCell.Count += take;

                        if (take != 0)
                        {
                            _ItemCellChangedList.Add(curCellIndex);
                            _ItemChangedDic.ModifyIntDic(curCell.Id,take);
                            ModifyManifest(curCell.Id, take);
                        }

                        if (curSrcItem.Count == 0 && curSrcItemIndex == remainingFirst)
                            remainingFirst = FindNextAvailableGroup(_RemainingList, remainingFirst);

                        _Items[curCellIndex] = curCell;
                        _RemainingList[curSrcItemIndex] = curSrcItem;
                    }
                }

                curCellIndex = (curCellIndex + 1) % _Items.Length;

            } while (remainingFirst < _RemainingList.Count && curCellIndex != 0);
            
            if (_ItemCellChangedList.Count != 0)
                InvokeOnItemCellChanged(result.PackageItemChangedInfo);

            return result;
        }

        public override bool CostItemCheck(params ItemGroup[] groups)
        {
            foreach (var itemGroup in groups)
                if (GetItemNum(itemGroup.Id)<itemGroup.Count || !LimitItem(itemGroup.Id))
                    return false;
            return true;
        }

        public override PackageOperation CostItem(params ItemGroup[] groups)
        {
            _ItemCellChangedList.Clear();
            _ItemChangedDic.Clear();
            _RemainingList.Clear();
            
            for (int i = 0; i < groups.Length; i++)
            {
                var g = groups[i].ToValid();
                if (g.Id == 0) continue;
                _RemainingList.Add(g);
                _ItemChangedDic.Add(g.Id, _RemainingList.Count - 1);
            }

            var result = new PackageOperation(
                JumpOverInvalidItem(_RemainingList),
                _ItemCellChangedList,
                ToChangedItemGroups(_ItemChangedDic));

            if (_RemainingList.Count == 0)
                return result;

            var curCellIndex = 0;
            var remainingCount = _RemainingList.Count;
            do
            {
                var curCell = _Items[curCellIndex];

                if (_ItemChangedDic.TryGetValue(curCell.Id, out var curSrcItemIndex))
                {
                    var curSrcItem = _RemainingList[curSrcItemIndex];
                    if (curSrcItem.Count != 0)
                    {
                        var maxStack = ItemSys.GetInfo(curCell.Id).MaxStackNum;
                        var take = Mathf.Min(maxStack - curCell.Count, curSrcItem.Count);

                        curCell.Count -= take;
                        curSrcItem.Count -= take;

                        if (take != 0)
                        {
                            _ItemChangedDic.ModifyIntDic(curCell.Id,-take);
                            _ItemCellChangedList.Add(curCellIndex);
                            ModifyManifest(curCell.Id,-take);
                        }

                        if (curSrcItem.Count == 0)
                            remainingCount--;

                        _Items[curCellIndex] = curCell;
                        _RemainingList[curSrcItemIndex] = curSrcItem;
                    }
                }

                curCellIndex = (curCellIndex + 1) % _RemainingList.Count;

            } while (remainingCount > 0 && curCellIndex != 0);

            if(_ItemCellChangedList.Count!=0)
                InvokeOnItemCellChanged(
                    new PackageItemChangedInfo(
                        _ItemCellChangedList,
                        ToChangedItemGroups(_ItemChangedDic)));

            return result;
        }

        public override void SortItem()
        {
            var curCell = 0;
            foreach (var info in _ItemManifest)
            {
                var maxStack = ItemSys.GetInfo(info.Key).MaxStackNum;
                //info.Value.FirstCell = curCell;
                var count = info.Value.Count;
                while (count > 0)
                {
                    var take = Mathf.Min(maxStack, count);
                    count -= take;
                    _Items[curCell].Id = info.Key;
                    _Items[curCell].Count += take;
                    curCell++;
                }
            }

            while (curCell < _Items.Length)
            {
                _Items[curCell].Id = 0;
                _Items[curCell].Count = 0;
                curCell++;
            }

            _ItemChangedDic.Clear();
            InvokeOnItemCellChanged(
                new PackageItemChangedInfo(
                    Tools.GetIndexEnum(0, _Items.Length),
                    ToChangedItemGroups(_ItemChangedDic)));
        }

        public string GetItemStr()
        {
            var builder = new StringBuilder();
            foreach (var itemGroup in _Items)
                builder.Append($"({itemGroup.Id} {itemGroup.Count})");

            return builder.ToString();
        }

        private void ModifyManifest(ItemId id, int change)
        {
            if (change == 0) return;
            if (!_ItemManifest.TryGetValue(id, out var info))
            {
                info = PackageItemInfo.Take();
                info.Count = change;
                _ItemManifest.Add(id,info);
                return;
            }

            info.Count += change;
            if (info.Count == 0)
                _ItemManifest.Remove(id);
        }

        private bool LimitItem(ItemId id)
        {
            if (_LimitItems.Length == 0) return true;
            foreach (var limitItem in _LimitItems)
                if (limitItem == id.Id)
                    return true;
            return false;
        }

        private int FindNextAvailableGroup(IList<ItemGroup> groups, int startIndex)
        {
            var count = startIndex;
            while (count < groups.Count && groups[count].Count != 0 && !LimitItem(groups[count].Id))
                count++;
            return count;
        }

        private static IEnumerable<ItemGroup> JumpOverInvalidItem(IEnumerable<ItemGroup> groups)
        {
            foreach (var itemGroup in groups)
            {
                if (itemGroup.Count != 0)
                    yield return itemGroup;
            }
        }

        private static IEnumerable<ItemGroup> ToChangedItemGroups(Dictionary<ItemId, int> dic)
        {
            foreach (var i in dic)
                yield return new ItemGroup(i.Key, i.Value);
        }

        public class PackageItemInfo : ObjectQueueItem<PackageItemInfo>
        {
            public int Count;
            protected override void Reset()
            {
                Count = 0;
                //FirstCell = 0;
            }
        }
    }
}
