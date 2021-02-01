

using System;
using System.Collections.Generic;
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

        protected override void OnHit(IEnumerable<HitInfo> hitInfos)
        {
            
        }
    }
}