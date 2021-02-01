using System;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using Framework;
using UnityEngine;

namespace Thunder
{
    public abstract class RangedWeaponLauncher:MonoBehaviour
    {
        public Action<IEnumerable<HitInfo>> OnHit;

        public float OverHeat => BulletSpread.OverHeat;

        protected BulletSpread BulletSpread;

        protected virtual void Awake()
        {
            BulletSpread = GetComponent<BulletSpread>();
        }

        public abstract void FireOnce(Vector3 pos,Vector3 dir);

        protected void HitSomething(IEnumerable<HitInfo> hitInfo)
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
