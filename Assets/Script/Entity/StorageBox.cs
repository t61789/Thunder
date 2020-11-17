using Framework;
using Thunder.UI;
using Thunder.Utility;

namespace Thunder.Entity
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
