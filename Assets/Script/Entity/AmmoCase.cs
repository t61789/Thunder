using Thunder.Sys;
using Thunder.Utility;

namespace Thunder.Entity
{
    public class AmmoCase : BaseEntity, IInteractive
    {
        private SimpleCounter _TakeAmmoCounter;
        private bool _Used;
        public float TakeAmmoTime = 1;

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

        protected override void Awake()
        {
            base.Awake();
            _TakeAmmoCounter = new SimpleCounter(TakeAmmoTime);
        }
    }
}