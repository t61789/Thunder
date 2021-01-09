using Framework;

using UnityEngine;

namespace Thunder
{
    public class Crossbow : BaseWeapon
    {
        private const string FIRE = "Fire1";
        private const string RELOAD = "Reload";
        private const string AUTO_RELOAD = "AutoReload";

        private readonly StickyInputDic _StickyInputDic = new StickyInputDic();
        private Animator _Animator;
        private CrossbowArrow _Arrow;
        public Vector3 ArrowPos;
        public float Damage;
        public Vector3 LaunchForce;

        public override float OverHeatFactor => 0;

        protected override void Awake()
        {
            base.Awake();

            _Animator = GetComponent<Animator>();
            _StickyInputDic.AddBool(RELOAD, 1f);
            _StickyInputDic.AddBool(AUTO_RELOAD, 1f);
        }

        private void Update()
        {
            if (ControlSys.RequireKey(FIRE, 0).Down) Fire();
            if (_StickyInputDic.GetBool(AUTO_RELOAD) ||
                ControlSys.RequireKey(RELOAD, 0).Down) Reload();
        }

        private void FixedUpdate()
        {
            _StickyInputDic.FixedUpdate();
            _Animator.SetBool(RELOAD, _StickyInputDic.GetBool(RELOAD));
        }

        public override void Fire()
        {
            if (AmmoGroup.MagazineEmpty())
            {
                _StickyInputDic.SetBool(AUTO_RELOAD, true);
                return;
            }

            _Arrow.Launch(LaunchForce, Damage);
            _Arrow = null;
            AmmoGroup.Magazine--;
            AmmoGroup.InvokeOnAmmoChanged();
            _Animator.SetTrigger(FIRE);
        }

        public override void Reload()
        {
            _StickyInputDic.SetBool(AUTO_RELOAD, false, true);
            if (!AmmoGroup.ReloadConfirm()) return;
            _StickyInputDic.SetBool(RELOAD, true);
        }

        public void ReloadCompleted()
        {
            _Arrow = ObjectPool.Get<CrossbowArrow>(Config.CrossbowArrowAssetPath);
            _Arrow.Install(Trans, ArrowPos);
            AmmoGroup.Reload();
            AmmoGroup.InvokeOnAmmoChanged();
        }

        public override void TakeOut()
        {
        }

        public override void PutBack()
        {
        }

        public override ItemAddData Drop()
        {
            return new ItemAddData(AmmoGroup.Magazine);
        }

        public override void ReadAdditionalData(ItemAddData add)
        {
            if (!add.TryGet(out int data)) return;
            if (data != 1 || AmmoGroup.Magazine != 0) return;
            _Arrow = ObjectPool.Get<CrossbowArrow>(Config.CrossbowArrowAssetPath);
            _Arrow.Install(Trans, ArrowPos);
            AmmoGroup.Magazine = 1;
            AmmoGroup.InvokeOnAmmoChanged();
        }
    }
}