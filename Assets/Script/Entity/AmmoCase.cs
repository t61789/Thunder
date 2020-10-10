using Thunder.Sys;
using Thunder.Utility;

namespace Thunder.Entity
{
    public class AmmoCase:BaseEntity,IInteractive
    {
        public float TakeAmmoTime = 1;

        private SimpleCounter _TakeAmmoCounter;
        private bool _Used;

        protected override void Awake()
        {
            base.Awake();
            _TakeAmmoCounter = new SimpleCounter(TakeAmmoTime);
        }

        public void Interactive(ControlInfo info)
        {
            if (!info.Stay)
            {
                _TakeAmmoCounter.Recount();
                _Used = false;
            }

            if (!_TakeAmmoCounter.Completed || _Used) return;
            Player.Ins.WeaponBelt.CurrentWeapon.FillAmmo();
            _Used = true;
        }
    }
}
