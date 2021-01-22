using System;
using Framework;
using UnityEngine;

namespace Thunder
{
    public class FastKineticEnergyLauncher : RangedWeaponLauncher
    {
        public override void FireOnce(Vector3 pos, Vector3 dir)
        {
            var result = BulletSpread.NextSpread();
            result.z = BulletSpread.ScaleFactor;
            var z = pos.normalized;
            var x = Vector3.Cross(Vector3.up, z);
            var matrix = Tools.BuildTransferMatrix(x, Vector3.Cross(z, x));
            result = matrix * result;


            if (!Physics.Raycast(pos, result, out var info)) return;
            var hitInfo = new HitInfo
            {
                HitDir = info.point, 
                HitPos = result, 
                Collider = info.collider
            };
            HitSomething(hitInfo);
        }
    }
}
