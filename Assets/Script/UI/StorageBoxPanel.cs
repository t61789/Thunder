using Framework;
using Thunder.Entity;

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

        public override void BeforeClose()
        {
            base.BeforeClose();
            ControlSys.ShieldValue.Release(StorageBox.OperationPanel);
        }
    }
}
