using System;
using BehaviorDesigner.Runtime.Tasks;
using Framework;
using UnityEngine;

namespace Thunder
{
    public abstract class FireControl
    {
        protected RangedWeaponLauncher _RangedWeaponLauncher;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fireCtrInfo"></param>
        /// <returns>射击状态</returns>
        public abstract object TryFire(ControlInfo fireCtrInfo);

        protected FireControl(RangedWeaponLauncher launcher)
        {
            _RangedWeaponLauncher = launcher;
        }
    }

    public abstract class RangedWeaponLauncher
    {
        public event Action<object> OnHit;

        protected BulletSpread BulletSpread;

        public float OverHeat => BulletSpread.OverHeat;

        protected RangedWeaponLauncher(BulletSpread bulletSpread)
        {
            BulletSpread = bulletSpread;
        }

        public abstract void FireOnce(Vector3 pos,Vector3 dir);

        protected void HitSomething(object hitInfo)
        {
            OnHit?.Invoke(hitInfo);
        }
    }

    public struct ShootInfo
    {
        public bool IntervalPass;
        public bool FireCheckPass;
        public bool FireRequirePass;
        public bool SafetyPass;
        public bool Shooted;
    }
}
