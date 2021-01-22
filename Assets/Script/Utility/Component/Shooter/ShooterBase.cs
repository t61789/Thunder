using System;
using BehaviorDesigner.Runtime.Tasks;
using Framework;
using UnityEngine;

namespace Thunder
{
    public abstract class RangedWeaponLauncher:MonoBehaviour
    {
        public Action<HitInfo> OnHit;

        public float OverHeat => BulletSpread.OverHeat;

        protected BulletSpread BulletSpread;

        protected virtual void Awake()
        {
            BulletSpread = GetComponent<BulletSpread>();
            var baseWeapon = GetComponent<BaseWeapon>();
            baseWeapon.SetLauncher(this);
            OnHit = baseWeapon.GetBulletHitHook();
        }

        public abstract void FireOnce(Vector3 pos,Vector3 dir);

        protected void HitSomething(HitInfo hitInfo)
        {
            OnHit?.Invoke(hitInfo);
        }
    }

    public struct HitInfo
    {
        public Vector3 HitPos;
        public Vector3 HitDir;
        public Collider Collider;
    }
}
