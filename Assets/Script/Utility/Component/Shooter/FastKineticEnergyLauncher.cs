using System;
using Framework;
using UnityEngine;

namespace Thunder
{
    public class FastKineticEnergyLauncher : RangedWeaponLauncher
    {
        public FastKineticEnergyLauncher(BulletSpread bulletSpread) : base(bulletSpread)
        {
        }

        public override void FireOnce(Vector3 pos, Vector3 dir)
        {
            var result = BulletSpread.NextSpread();
            result.z = BulletSpread.ScaleFactor;
            var z = pos.normalized;
            var x = Vector3.Cross(Vector3.up, z);
            var matrix = Tools.BuildTransferMatrix(x, Vector3.Cross(z, x));
            result = matrix * result;

            Physics.Raycast(pos, result, out var hitInfo);
            HitSomething(hitInfo);
        }
    }
}
