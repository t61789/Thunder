using System.Collections.Generic;
using System.Linq;
using FairyGUI;
using Framework;
using Thunder.UI;
using UnityEngine;

namespace Thunder
{
    public class PackagePanel:FairyPanel
    {
        private PackageList _PackageList;

        private void Start()
        {
            _PackageList = new PackageList(UIPanel.ui.GetChild("List"),Player.Ins.Package);
        }

        private void OnDestroy()
        {
            _PackageList.Destroy();
        }
    }
}
