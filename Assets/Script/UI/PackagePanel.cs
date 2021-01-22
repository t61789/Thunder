using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FairyGUI;
using Framework;

namespace Thunder
{
    public class PackagePanel:PanelUi
    {
        private GList _CellList;
        private CommonPackage _PlayerPackage;

        protected override void Awake()
        {
            base.Awake();
            _CellList = GetComponent<UIPanel>().ui.GetChild("List").asList;
        }

        private void Start()
        {
            _PlayerPackage = Player.Ins.Package;
            _PlayerPackage.OnItemChanged += UpdateCell;
        }

        private void UpdateCell(IEnumerable<int> changeList)
        {
            //_CellList.
        }
    }
}
