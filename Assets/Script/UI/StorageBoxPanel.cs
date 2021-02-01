using Framework;


namespace Thunder.UI
{
    public class StorageBoxPanel:PanelUi
    {
        //public PackageCellAdapter PackageCellAdapter { private set; get; }

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
