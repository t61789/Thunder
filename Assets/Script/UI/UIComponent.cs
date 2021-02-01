using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FairyGUI;
using Framework;
using UnityEngine;

namespace Thunder.UI
{
    public class PackageList
    {
        public GList CellList { get; }
        private readonly PackageCell[] _Cells;
        private readonly CommonPackage _Package;

        public PackageList(GObject obj,CommonPackage package)
        {
            CellList = obj.asList;
            var sel = from cellCom in CellList.GetChildren()
                select new PackageCell(cellCom.asCom);
            _Cells = sel.ToArray();
            _Package = package;

            _Package.OnItemChanged += UpdateCell;
            CellList.numItems = _Package.Capacity;
        }

        public void Destroy()
        {
            _Package.OnItemChanged -= UpdateCell;
        }

        private void UpdateCell(PackageItemChangedInfo changedInfo)
        {
            foreach (var index in changedInfo.ChangedCells)
            {
                var info = _Package.GetItemInfo(index);
                _Cells[index].SetItem(info);
            }
        }
    }

    public class PackageCell
    {
        public GButton Button { get; }

        private readonly GLoader _Texture;
        private readonly GTextField _Num;
        private ItemGroup _CurItemGroup;

        public PackageCell(GComponent component)
        {
            Button = component.asButton;
            _Texture = Button.GetChild("texture").asLoader;
            _Num = Button.GetChild("num").asTextField;
        }

        public void SetItem(ItemGroup itemGroup)
        {
            var itemInfo = ItemSys.GetInfo(itemGroup.Id);

            if (_CurItemGroup.Id != itemGroup.Id)
            {
                var texture = _CurItemGroup.Id == 0
                    ? null
                    : BundleSys.GetAsset<Texture>(itemInfo.PackageCellTexturePath);
                _Texture.texture = new NTexture(texture);
            }

            if(_CurItemGroup.Count!=itemGroup.Count)
                _Num.text = itemGroup.Count.ToString();

            _CurItemGroup = itemGroup;
        }

        public ItemGroup GetItem()
        {
            return _CurItemGroup;
        }
    }
}
