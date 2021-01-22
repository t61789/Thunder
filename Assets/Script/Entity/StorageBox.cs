using Framework;
using Thunder.UI;


namespace Thunder
{
    public class StorageBox:BaseEntity,IInteractive
    {
        public int Size;

        public const string OperationPanel = "storageBoxPanel";
        private Package _Package;

        protected override void Awake()
        {
            base.Awake();
            _Package = new CommonPackage(Size);
        }

        public void Interactive(ControlInfo info)
        {
            if (!info.Down) return;
            ControlSys.ShieldValue.Request(OperationPanel,1);
            var panel = UiSys.OpenUi<StorageBoxPanel>(OperationPanel);
            panel.PackageCellAdapter.SetPackage(_Package);
        }
    }
}
