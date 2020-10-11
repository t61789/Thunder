using System.Collections.Generic;
using System.IO;
using Thunder.Sys;
using UnityEngine;
using UnityEngine.Assertions;
using ItemSys = Thunder.Sys.ItemSys;

namespace Thunder.Utility
{
    public class Package
    {
        private readonly PackageCell[] _Items;

        private readonly CircleQueue<PackageItemInfo> _PackageItemInfoQueue =
            new CircleQueue<PackageItemInfo>(GlobalSettings.PackageItemInfoBuffer);

        private readonly SortedDictionary<ItemId, PackageItemInfo> _PackageItemInfos =
            new SortedDictionary<ItemId, PackageItemInfo>();

        private readonly List<int> _UpdateList = new List<int>();

        public Package(int packageSize)
        {
            Assert.IsTrue(packageSize > 0, "背包容量需要大于0");
            Ins = this;
            _Items = new PackageCell[packageSize];
        }

        public static Package Ins { private set; get; }

        /// <summary>
        ///     向背包中添加一定数量物品
        /// </summary>
        /// <param name="itemGroup"></param>
        /// <param name="update">更新的单元格下标</param>
        /// <returns>未成功添加的物品数量</returns>
        public int AddItem(ItemGroup itemGroup, out int[] update)
        {
            if (!CanPackage(itemGroup.Id))
            {
                update = new int[0];
                return itemGroup.Count;
            }

            _UpdateList.Clear();
            if (!_PackageItemInfos.TryGetValue(itemGroup.Id, out var packageItemInfo))
            {
                packageItemInfo = _PackageItemInfoQueue.Count != 0
                    ? _PackageItemInfoQueue.Dequeue()
                    : new PackageItemInfo();
                _PackageItemInfos.Add(itemGroup.Id, packageItemInfo);
            }

            var maxStack = ItemSys.Ins[itemGroup.Id].MaxStackNum;
            var curCell = packageItemInfo.FirstCell;
            var count = itemGroup.Count;
            do
            {
                if (_Items[curCell].Id == 0 || _Items[curCell].Id == itemGroup.Id)
                {
                    var take = Mathf.Min(count, maxStack - _Items[curCell].Count);
                    count -= take;
                    packageItemInfo.Count += take;
                    _Items[curCell].Count += take;
                    if (take > 0) _UpdateList.Add(take);
                }

                curCell++;
                curCell %= _Items.Length;
            } while (curCell != packageItemInfo.FirstCell && count > 0);

            update = _UpdateList.ToArray();
            return count;
        }

        /// <summary>
        /// 添加一组物品
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="update">更新的单元格下标</param>
        /// <returns></returns>
        public int AddOneGroupItem(ItemId itemId, out int[] update)
        {
            return AddItem((itemId, ItemSys.Ins[itemId].MaxStackNum), out update);
        }

        /// <summary>
        ///     消耗一定数量的物品
        /// </summary>
        /// <param name="itemGroup"></param>
        /// <param name="update">更新的单元格下标</param>
        /// <returns>未完成消耗的数量</returns>
        public int CostItem(ItemGroup itemGroup, out int[] update)
        {
            var itemId = itemGroup.Id;
            var count = itemGroup.Count;
            if (!_PackageItemInfos.TryGetValue(itemId, out var packageItemInfo))
            {
                update = new int[0];
                return count;
            }

            _UpdateList.Clear();
            packageItemInfo.Count -= count;
            var curCell = packageItemInfo.FirstCell;
            do
            {
                if (_Items[curCell].Id == itemId)
                {
                    var cost = Mathf.Min(_Items[curCell].Count, count);
                    _Items[curCell].Count -= cost;
                    count -= cost;
                    packageItemInfo.Count -= cost;
                    if (cost != 0)
                        _UpdateList.Add(curCell);
                    if (_Items[curCell].Count == 0)
                        _Items[curCell].Id = 0;
                }

                curCell++;
                curCell %= _Items.Length;
            } while (curCell != packageItemInfo.FirstCell && count > 0);

            if (packageItemInfo.Count == 0)
            {
                _PackageItemInfoQueue.Enqueue(packageItemInfo);
                _PackageItemInfos.Remove(itemId);
            }

            update = _UpdateList.ToArray();

            return count;
        }

        /// <summary>
        ///     获取单元格内的物品信息
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public PackageCell GetCell(int index)
        {
            return _Items[index];
        }

        /// <summary>
        ///     获取背包中物品的总数
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public int GetItemNum(ItemId itemId)
        {
            return _PackageItemInfos.TryGetValue(itemId, out var result) ? result.Count : 0;
        }

        /// <summary>
        ///     设置指定单元格内的物品
        /// </summary>
        /// <param name="index"></param>
        /// <param name="itemId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int SetCell(int index, ItemId itemId, int count)
        {
            if (count == 0 && itemId != 0)
                throw new InvalidDataException();
            if (_Items[index].Id != 0)
            {
                var info = _PackageItemInfos[_Items[index].Id];
                info.Count -= _Items[index].Count;
                if (info.Count == 0)
                {
                    _PackageItemInfos.Remove(_Items[index].Id);
                    _PackageItemInfoQueue.Enqueue(info);
                }
            }

            _Items[index].Id = itemId;
            var take = Mathf.Min(ItemSys.Ins[itemId].MaxStackNum, count);
            count -= take;
            _Items[index].Count += take;
            if (!_PackageItemInfos.TryGetValue(itemId, out var packageItemInfo))
            {
                packageItemInfo = _PackageItemInfoQueue.Count != 0
                    ? _PackageItemInfoQueue.Dequeue()
                    : new PackageItemInfo();
                _PackageItemInfos.Add(itemId, packageItemInfo);
            }

            packageItemInfo.Count += take;
            return count;
        }

        /// <summary>
        ///     整理物品
        /// </summary>
        public void SortItem()
        {
            var curCell = 0;
            foreach (var info in _PackageItemInfos)
            {
                var maxStack = ItemSys.Ins[info.Key].MaxStackNum;
                info.Value.FirstCell = curCell;
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
        }

        /// <summary>
        /// 目标物品可否纳入背包
        /// </summary>
        public bool CanPackage(ItemId itemId)
        {
            return (ItemSys.Ins[itemId].Flag & ItemFlag.Packageable) != 0;
        }
    }

    public class PackageItemInfo
    {
        public int Count;
        public int FirstCell;
    }

    public struct PackageCell
    {
        public ItemId Id;
        public int Count;

        public PackageCell(ItemId id, int count)
        {
            Id = id;
            Count = count;
        }
    }
}