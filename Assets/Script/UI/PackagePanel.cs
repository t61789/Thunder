using System.Collections.Generic;
using FairyGUI;
using Framework;
using Thunder.UI;
using UnityEngine;

namespace Thunder
{
    public class PackagePanel:FairyPanel
    {
        private GList _CellList;
        private CommonPackage _PlayerPackage;

        protected override void Awake()
        {
            base.Awake();
            _CellList = UIPanel.ui.GetChild("List").asList;
            _PlayerPackage = Player.Ins.Package;
            _PlayerPackage.OnItemChanged += UpdateCell;
            _CellList.numItems = _PlayerPackage.Capacity;
        }

        private void UpdateCell(IEnumerable<int> changeList)
        {
            foreach (var index in changeList)
            {
                var info = _PlayerPackage.GetItemInfoFrom(index);
                var path = ItemSys.GetInfo(info.Id).PackageCellTexturePath;
                Texture texture = null;
                if (info.Id!=0 && !path.IsNullOrEmpty())
                    texture = BundleSys.GetAsset<Texture>(path);
                _CellList.GetChildAt(index).asButton.GetChild("Image").asLoader.texture = new NTexture(texture);
            }
        }
    }
}
