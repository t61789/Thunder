

using System;
using Framework;
using UnityEngine;

namespace Thunder
{
    public class Unarmed : BaseWeapon
    {
        public override float OverHeatFactor { get; }

        public override void Fire()
        {
        }

        public override void Reload()
        {
        }

        public override void TakeOut()
        {
        }

        public override void PutBack()
        {
        }

        public override string CompressItem()
        {
            return default;
        }

        public override void DecompressItem(string add)
        {
        }

        public override Action<HitInfo> GetBulletHitHook()
        {
            return null;
        }
    }
}