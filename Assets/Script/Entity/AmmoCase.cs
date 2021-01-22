using Framework;


namespace Thunder
{
    public class AmmoCase : BaseEntity, IInteractive
    {
        public float TakeAmmoTime = 1;
        
        private SimpleCounter _TakeAmmoCounter;
        private bool _Used;

        private static Player Player => Player.Ins;

        public void Interactive(ControlInfo info)
        {
            if (!info.Stay)
            {
                _TakeAmmoCounter.Recount();
                _Used = false;
            }

            if (!_TakeAmmoCounter.Completed || _Used) return;
            var ammoId = Player.WeaponBelt.CurrentWeapon.AmmoGroup.AmmoId;
            Player.Package.PutItem((ammoId, ItemSys.GetInfo(ammoId).MaxStackNum), false);
            _Used = true;
        }

        protected override void Awake()
        {
            base.Awake();
            _TakeAmmoCounter = new SimpleCounter(TakeAmmoTime);
        }
    }
}