using Framework;
using Thunder.UI;
using UnityEngine;

namespace Thunder
{
    public class BuildingCenter:BaseEntity,IInteractive
    {
        public static BuildingCenter Ins;

        public int CtrlShield=10;

        private readonly Process _Process = new Process();

        protected override void Awake()
        {
            base.Awake();
            Ins = this;
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

        public void Interactive(ControlInfo info)
        {
            if (info.Down)
            {
                StartingBuildingMode();
            }
        }

        public void StartingBuildingMode()
        {
            ControlSys.ShieldValue.Request(nameof(BuildingCenter), CtrlShield);
            _Process.OnProcessComplete += StartBuildingMode;
            PublicEvents.StartingBuildingMode?.Invoke(_Process);
        }

        public void StartBuildingMode()
        {
            _Process.OnProcessComplete -= StartBuildingMode;
            PublicEvents.StartBuildingMode?.Invoke();
        }

        public void EndingBuildingMode()
        {
            _Process.OnProcessComplete += EndBuildingMode;
            PublicEvents.EndingBuildingMode?.Invoke(_Process);
        }

        public void EndBuildingMode()
        {
            ControlSys.ShieldValue.Release(nameof(BuildingCenter));
            _Process.OnProcessComplete -= EndBuildingMode;
            PublicEvents.EndBuildingMode?.Invoke();
        }
    }
}
