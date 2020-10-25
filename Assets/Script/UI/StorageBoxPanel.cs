using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thunder.Entity;
using Thunder.Sys;
using Thunder.Tool;

using Thunder.Utility;

namespace Thunder.UI
{
    public class StorageBoxPanel:PanelUI
    {
        public PackageCellAdapter PackageCellAdapter { private set; get; }

        protected override void Awake()
        {
            base.Awake();
            // todo PackageCellAdapter = new PackageCellAdapter();
        }

        protected override void BeforeClose()
        {
            base.BeforeClose();
            ControlSys.Ins.ShieldValue.Release(StorageBox.OperationPanel);
        }
    }
}
