using Thunder.Sys;
using Thunder.Tool;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Entity.Weapon
{
    public class LongSword : BaseWeapon
    {
        public float AttackAreaFadeTime = 0.5f;
        public Vector3 AttackRange;
        public float Damage = 20;

        public override float OverHeatFactor => 0;

        public override void Fire()
        {
            var rot = Camera.main.transform.rotation;
            var area = ObjectPool.Ins.Alloc<MeleeAttackArea>(GlobalSettings.MeleeAttackAreaAssetPath);
            area.Init(_Player.Trans.position, rot, AttackRange, AttackAreaFadeTime);
            area.HitEnter += HitTarget;
        }

        private void HitTarget(Collider target)
        {
            var shoot = target.GetComponent<IShootable>();
            shoot?.GetShoot(target.transform.position,
                target.transform.position - _Player.Trans.position,
                Damage);
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

        public override ItemAddData Drop()
        {
            return default;
        }

        public override void ReadAdditionalData(ItemAddData add)
        {
        }
    }
}