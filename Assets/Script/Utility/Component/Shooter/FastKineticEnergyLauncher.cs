using System;
using System.Collections.Generic;
using Framework;
using UnityEngine;

namespace Thunder
{
    [RequireComponent(typeof(BulletSpread))]
    public class FastKineticEnergyLauncher : RangedWeaponLauncher
    {
        public override void FireOnce(Vector3 pos, Vector3 dir)
        {
            var result = BulletSpread.NextSpread();
            result.z = BulletSpread.ScaleFactor;
            var z = dir.normalized;
            var x = Tools.Cross(z, Vector3.up);
            var y = Tools.Cross(x, z);
            var matrix = Tools.BuildTransferMatrix(x, y, z, pos);
            result = matrix * result;

            HitSomething(RayHit(pos,result));
        }

        private static IEnumerable<HitInfo> RayHit(Vector3 origin,Vector3 dir)
        {
            foreach (var raycastHit in Physics.RaycastAll(origin, dir))
            {
                if (raycastHit.transform.gameObject == Player.Ins.gameObject) continue;

                var hitInfo = new HitInfo
                {
                    HitDir = (raycastHit.point - origin).normalized,
                    HitPos = dir,
                    Collider = raycastHit.collider
                };

                yield return hitInfo;
            }
        }
    }
}
