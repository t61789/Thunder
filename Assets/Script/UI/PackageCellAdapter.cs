using System.Collections.Generic;
using Thunder.Entity;


using Thunder.Utility;
using Tool;
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
            _Package.OnItemChanged -= RefreshCell;
            _Package = package;
            for (int i = 0; i < _PackageCells.Length; i++)
            {
                if(_PackageCells[i]==null)continue;
                ObjectPool.Ins.Recycle(_PackageCells[i]);
                _PackageCells[i] = null;
            }
            RefreshCell(Tools.GetIndexArr(0, _Package.Size));
            _Package.OnItemChanged += RefreshCell;
        }

        private void RefreshCell(IEnumerable<int> indexs)
        {
            foreach (var index in indexs)
            {
                var item = _Package.GetCell(index);
                if (_PackageCells[index] != null)
                {
                    ObjectPool.Ins.Recycle(_PackageCells[index]);
                    _PackageCells = null;
                }
                if (item.Id == 0) continue;
                var cell = ObjectPool.Ins.Alloc<PackageCell>(GlobalSettings.PackageCellPrefabAssetPath);
                cell.Init(_Package,index, _FloatContainer);
                cell.RectTrans.SetParent(_CellContainers[index]);
                _PackageCells[index] = cell;
            }
        }
    }
}
