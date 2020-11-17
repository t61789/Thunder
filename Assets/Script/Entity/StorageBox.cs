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
            _Package = new Package(Size);
        }

        public void Interactive(ControlInfo info)
        {
            if (!info.Down) return;
            ControlSys.ShieldValue.Request(OperationPanel);
            var panel = UISys.OpenUI<StorageBoxPanel>(OperationPanel);
            panel.PackageCellAdapter.SetPackage(_Package);
        }
    }
}
