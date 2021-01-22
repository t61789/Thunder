using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Framework
{
    public abstract class Package
    {
        public event Action<IEnumerable<int>> OnItemChanged;

        /// <summary>
        /// 向包裹中非指定地添加物品
        /// </summary>
        /// <param name="itemGroup">需要添加的物品组</param>
        /// <param name="putOnlySpaceEnough">是否只在空间完全足够的情况下添加物品</param>
        /// <returns>操作结果</returns>
        public abstract PackageOperation PutItem(ItemGroup itemGroup, bool putOnlySpaceEnough);

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
        public abstract ItemGroup GetItemInfoFrom(int index);

        /// <summary>
        /// 消耗指定数量的物品
        /// </summary>
        /// <param name="itemGroup"></param>
        /// <param name="costOnlyItemEnough">仅在完全满足条件时才消耗</param>
        /// <returns>操作结果</returns>
        public abstract PackageOperation CostItem(ItemGroup itemGroup, bool costOnlyItemEnough);

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

        protected virtual void InvokeOnItemChanged(IEnumerable<int> changeList)
        {
            OnItemChanged?.Invoke(changeList);
        }
    }

    public class PackageOperation : ObjectQueueItem<PackageOperation>
    {
        public int RemainingNum;
        public List<int> ItemChangeList = new List<int>();

        protected override void Reset()
        {
            ItemChangeList.Clear();
            RemainingNum = 0;
        }
    }

    public class CommonPackage : Package
    {
        public int Capacity => _Items.Length;

        private readonly ItemGroup[] _Items;
        private readonly ItemGroup[] _TempItems;
        private readonly SortedDictionary<ItemId, PackageItemInfo> _ItemManifest
            = new SortedDictionary<ItemId, PackageItemInfo>();

        public CommonPackage(int cellNum)
        {
            if (cellNum <= 0)
                throw new Exception("背包容量需要大于0");

            _Items = new ItemGroup[cellNum];
            _TempItems = new ItemGroup[cellNum];
        }

        public int GetItemNum(ItemId id)
        {
            return _ItemManifest.TryGetValue(id, out var result) ? result.Count : 0;
        }

        public override PackageOperation PutItem(ItemGroup itemGroup, bool putOnlySpaceEnough)
        {
            var result = PackageOperation.Take();
            if (itemGroup.Count == 0 || !CanPackage(itemGroup.Id))
            {
                result.RemainingNum = itemGroup.Count;
                return result;
            }

            if (!_ItemManifest.TryGetValue(itemGroup.Id, out var packageItemInfo))
            {
                packageItemInfo = PackageItemInfo.Take();
                _ItemManifest.Add(itemGroup.Id, packageItemInfo);
            }

            var items = _Items;
            if (putOnlySpaceEnough)
            {
                _Items.CopyTo(_TempItems, 0);
                items = _TempItems;
            }

            var maxStack = ItemSys.GetInfo(itemGroup.Id).MaxStackNum;
            var curCell = 0;
            var count = itemGroup.Count;
            do
            {
                if (items[curCell].Id == 0 || items[curCell].Id == itemGroup.Id)
                {
                    items[curCell].Id = itemGroup.Id;
                    var take = Mathf.Min(count, maxStack - items[curCell].Count);
                    count -= take;
                    packageItemInfo.Count += take;
                    items[curCell].Count += take;
                    if (take > 0) result.ItemChangeList.Add(curCell);
                }

                curCell++;
                curCell %= items.Length;
            } while (curCell != 0 && count > 0);

            if (putOnlySpaceEnough) // 未完全添加，回滚数据
            {
                if (count != 0)
                {
                    packageItemInfo.Count -= itemGroup.Count - count;
                    if (packageItemInfo.Count == 0)
                        _ItemManifest.Remove(itemGroup.Id);

                    result.ItemChangeList.Clear();
                    result.RemainingNum = itemGroup.Count;
                    return result;
                }

                // 完全添加
                foreach (var index in result.ItemChangeList)
                    _Items[index] = _TempItems[index];
            }

            result.RemainingNum = count;

            InvokeOnItemChanged(result.ItemChangeList);

            return result;
        }

        public override ItemGroup PutItemInto(int index, ItemGroup itemGroup)
        {
            if (itemGroup.Id == 0) // 若源物品为空则直接置空
            {
                var takeOut = _Items[index];
                SetItemAt(index, new ItemGroup());
                return takeOut;
            }

            if (itemGroup.Count == 0 || !CanPackage(itemGroup.Id)) // 数量为空或是不能放入背包则不能添加
                return itemGroup;

            var maxStack = ItemSys.GetInfo(itemGroup.Id).MaxStackNum;

            // 为空或是物品相同则直接添加
            if (_Items[index].Id == 0 || _Items[index].Id == itemGroup.Id)
            {
                _Items[index].Id = itemGroup.Id;
                var take = Mathf.Min(maxStack - _Items[index].Count, itemGroup.Count);
                _Items[index].Count += take;
                itemGroup.Count -= take;

                if (_ItemManifest.TryGetValue(itemGroup.Id, out var packageItemInfo))
                {
                    packageItemInfo.Count += take;
                }
                else
                {
                    packageItemInfo = PackageItemInfo.Take();
                    packageItemInfo.Count = _Items[index].Count;
                    _ItemManifest.Add(itemGroup.Id, packageItemInfo);
                }

                return itemGroup.Count == 0 ? new ItemGroup() : itemGroup;
            }

            // 目标单元格不为空或是源物品且暂存区的物品数量大于最大堆叠数
            // 因为暂存区只有一个单元格，则会造成目标单元格内的物品无法被放置到暂存区中
            // 所以在这种情况下判定为无法添加物品
            if (itemGroup.Count > maxStack)
                return itemGroup;

            // 替换单元格内的物品
            var result = _Items[index];
            SetItemAt(index,itemGroup);

            InvokeOnItemChanged(EnumerateOnce(index));

            return result;
        }

        public override void SetItemAt(int index, ItemGroup itemGroup)
        {
            var desItem = _Items[index];
            if (desItem.Id != 0)
            {
                var manifest = _ItemManifest[desItem.Id];
                manifest.Count -= desItem.Count;
                if (manifest.Count == 0)
                {
                    _ItemManifest.Remove(desItem.Id);
                    manifest.Dispose();
                }
            }

            _Items[index] = itemGroup;

            if (!_ItemManifest.TryGetValue(itemGroup.Id, out var packageItemInfo))
            {
                packageItemInfo = PackageItemInfo.Take();
                packageItemInfo.Count = itemGroup.Count;
                _ItemManifest.Add(itemGroup.Id, packageItemInfo);
            }
            else
            {
                packageItemInfo.Count += itemGroup.Count;
            }

            InvokeOnItemChanged(EnumerateOnce(index));
        }

        public override IEnumerable<ItemGroup> GetAllItemInfo()
        {
            return _Items;
        }

        public override ItemGroup GetItemInfoFrom(int index)
        {
            return _Items[index];
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

            InvokeOnItemChanged(Tools.GetIndexEnum(0,_Items.Length));
        }

        public override PackageOperation CostItem(ItemGroup itemGroup, bool costOnlyItemEnough)
        {
            var result = PackageOperation.Take();

            if (itemGroup.Count == 0) return result;

            if (itemGroup.Id==0 || 
                GetItemNum(itemGroup.Id)==0 || 
                costOnlyItemEnough && GetItemNum(itemGroup.Id) < itemGroup.Count)
            {
                result.RemainingNum = itemGroup.Count;
                return result;
            }

            var itemId = itemGroup.Id;
            var count = itemGroup.Count;
            var manifest = _ItemManifest[itemGroup.Id];
            var curCell = 0;

            do
            {
                if (_Items[curCell].Id == itemId)
                {
                    var cost = Mathf.Min(_Items[curCell].Count, count);
                    _Items[curCell].Count -= cost;
                    count -= cost;
                    if (cost != 0)
                        result.ItemChangeList.Add(curCell);
                    if (_Items[curCell].Count == 0)
                        _Items[curCell].Id = 0;
                }

                curCell++;
                curCell %= _Items.Length;
            } while (curCell != 0 && count > 0);

            manifest.Count -= itemGroup.Count - count;
            if (manifest.Count == 0)
            {
                _ItemManifest.Remove(itemGroup.Id);
                manifest.Dispose();
            }

            result.RemainingNum = count;

            InvokeOnItemChanged(result.ItemChangeList);

            return result;
        }

        public string GetItemStr()
        {
            var builder = new StringBuilder();
            foreach (var itemGroup in _Items)
                builder.Append($"({itemGroup.Id} {itemGroup.Count})");

            return builder.ToString();
        }

        private IEnumerable<int> EnumerateOnce(int index)
        {
            yield return index;
        }

        public class PackageItemInfo : ObjectQueueItem<PackageItemInfo>
        {
            public int Count;
            //public int FirstCell;
            protected override void Reset()
            {
                Count = 0;
                //FirstCell = 0;
            }
        }
    }
}
