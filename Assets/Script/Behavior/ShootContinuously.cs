using BehaviorDesigner.Runtime.Tasks;
using Framework;

namespace Thunder.Behavior
{
    public class ShootContinuously:Action
    {
        //private MachineGunFireControl _MachineGunFireControl;

        public override void OnAwake()
        {
            //_MachineGunFireControl = GetComponent<MachineGunFireControl>();
        }

        public override TaskStatus OnUpdate()
        {
            //_MachineGunFireControl.TryShoot(new ControlInfo(default,true,true,true));
            return TaskStatus.Running;
        }
    }
}
