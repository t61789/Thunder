using Framework;
using Thunder.UI;
using UnityEngine;

namespace Thunder
{
    public class BuildingCenter:BaseEntity
    {
        public static BuildingCenter Ins;

        public string BuildingModeStartKey = "building_mode_start_key";
        public string BuildingModeEndKey = "building_mode_end_key";
        public int CtrlShield=10;

        private bool _Building = false;
        private readonly Process _Process = new Process();

        protected override void Awake()
        {
            base.Awake();
            Ins = this;
        }

        private void Update()
        {
            ReadCtrl();
        }

        private void OnGUI()
        {
            //if (GUI.Button(new Rect(0, 0, 500, 200), "Start"))
            //{
                //VirtualBuilding.Ins.Show(2);
                //FairyPanel.OpenPanel("packagePanel");
            //}

            //if (GUI.Button(new Rect(0, 200, 500, 200), "End"))
            //{
               // EndingBuildingMode();
            //}
        }

        public void ReadCtrl()
        {
            if (ControlSys.RequireKey(CtrlKeys.GetKey(BuildingModeStartKey)).Down)
            {
                StartingBuildingMode();
                return;
            }

            if (ControlSys.RequireKey(CtrlKeys.GetKey(BuildingModeEndKey)).Down)
            {
                EndingBuildingMode();
            }
        }

        public void StartingBuildingMode()
        {
            if (_Building || _Process.Running) return;

            ControlSys.ShieldValue.Request(nameof(BuildingCenter), CtrlShield);
            _Process.OnProcessComplete += StartBuildingMode;
            _Building = true;
            PublicEvents.StartingBuildingMode?.Invoke(_Process);
        }

        public void EndingBuildingMode()
        {
            if (!_Building || _Process.Running) return;

            _Process.OnProcessComplete += EndBuildingMode;
            PublicEvents.EndingBuildingMode?.Invoke(_Process);
        }

        private void StartBuildingMode()
        {
            _Process.OnProcessComplete -= StartBuildingMode;
            PublicEvents.StartBuildingMode?.Invoke();
        }

        private void EndBuildingMode()
        {
            ControlSys.ShieldValue.Release(nameof(BuildingCenter));
            _Process.OnProcessComplete -= EndBuildingMode;
            _Building = false;
            PublicEvents.EndBuildingMode?.Invoke();
        }
    }
}
