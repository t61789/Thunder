using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thunder.Sys;
using Thunder.Tool.ObjectPool;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Entity.Weapon
{
    public class Crossbow:BaseWeapon
    {
        public Vector3 ArrowPos;
        public Vector3 LaunchForce;
        public float Damage;

        private readonly StickyInputDic _StickyInputDic = new StickyInputDic();
        private Animator _Animator;
        private CrossbowArrow _Arrow;
        private const string FIRE = "Fire1";
        private const string RELOAD = "Reload";
        private const string AUTO_RELOAD = "AutoReload";

        protected override void Awake()
        {
            base.Awake();

            _Animator = GetComponent<Animator>();
            _StickyInputDic.AddBool(RELOAD, 1f);
            _StickyInputDic.AddBool(AUTO_RELOAD, 1f);
        }

        private void Update()
        {
            if (ControlSys.Ins.RequireKey(FIRE, 0).Down)Fire();
            if (_StickyInputDic.GetBool(AUTO_RELOAD) || 
                ControlSys.Ins.RequireKey(RELOAD, 0).Down)Reload();
        }

        private void FixedUpdate()
        {
            _StickyInputDic.FixedUpdate();
            _Animator.SetBool(RELOAD,_StickyInputDic.GetBool(RELOAD));
        }

        protected override void PlayerSquat(bool squatting, bool hanging)
        {
            
        }

        protected override void PlayerHanging(bool squatting, bool hanging)
        {
            
        }

        public override void Fire()
        {
            if (AmmoGroup.MagzineEmpty())
            {
                _StickyInputDic.SetBool(AUTO_RELOAD,true);
                return;
            }
            _Arrow.Launch(LaunchForce,Damage);
            _Arrow = null;
            AmmoGroup.Magzine--;
            AmmoGroup.InvokeOnAmmoChanged();
            _Animator.SetTrigger(FIRE);
        }

        public override void Reload()
        {
            _StickyInputDic.SetBool(AUTO_RELOAD,false,true);
            if (!AmmoGroup.ReloadConfirm()) return;
            _StickyInputDic.SetBool(RELOAD, true);
        }

        public void ReloadCompleted()
        {
            _Arrow = ObjectPool.Ins.Alloc<CrossbowArrow>(GlobalSettings.CrossbowArrowAssetPath);
            _Arrow.Install(_Trans, ArrowPos);
            AmmoGroup.Reload();
            AmmoGroup.InvokeOnAmmoChanged();
        }

        public override void FillAmmo()
        {
            AmmoGroup.FillUp(false);
        }

        public override void TakeOut()
        {
        }

        public override void PutBack()
        {
        }

        public override void Drop()
        {
        }
    }
}
