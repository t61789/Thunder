using System.Collections.Generic;
using Framework;


using UnityEngine;

namespace Thunder.UI
{
    public class PackageCellAdapter
    {
        private Package _Package;
        private PackageCell[] _PackageCells;
        private readonly RectTransform[] _CellContainers;
        private readonly RectTransform _FloatContainer;

        public PackageCellAdapter(RectTransform[] cellContainers,RectTransform floatContainer)
        {
            _CellContainers = cellContainers;
            _PackageCells = new PackageCell[_CellContainers.Length];
            _FloatContainer = floatContainer;
        }

        public void SetPackage(Package package)
        {
            // todo 背包提交器

            //_Package.OnItemChanged -= RefreshCell;
            _Package = package;
            for (int i = 0; i < _PackageCells.Length; i++)
            {
                if(_PackageCells[i]==null)continue;
                ObjectPool.Put(_PackageCells[i]);
                _PackageCells[i] = null;
            }
            //RefreshCell(Tools.GetIndexArr(0, _Package.Capacity));
            //_Package.OnItemChanged += RefreshCell;
        }

        private void RefreshCell(IEnumerable<int> indexs)
        {
            foreach (var index in indexs)
            {
                var item = _Package.GetItemInfoFrom(index);
                if (_PackageCells[index] != null)
                {
                    ObjectPool.Put(_PackageCells[index]);
                    _PackageCells = null;
                }
                if (item.Id == 0) continue;
                PackageCell cell = null;//ObjectPool.Get<PackageCell>(Config.PackageCellPrefabAssetPath);
                cell.Init(_Package,index, _FloatContainer);
                cell.RectTrans.SetParent(_CellContainers[index]);
                _PackageCells[index] = cell;
            }
        }
    }
}
