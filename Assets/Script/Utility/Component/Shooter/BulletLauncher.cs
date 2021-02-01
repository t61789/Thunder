using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;
using UnityEngine;

namespace Thunder
{
    public class BulletLauncher:RangedWeaponLauncher
    {
        public string BulletPrefabPath;
        public float BulletSpeed = 3f;

        private Collider _Collider;

        protected override void Awake()
        {
            base.Awake();// todo 建筑模型

            if(BulletPrefabPath.IsNullOrEmpty())
                Debug.Log($"{name} 的子弹发射器未指定子弹prefab");

            _Collider = GetComponent<Collider>();
        }

        public override void FireOnce(Vector3 pos, Vector3 dir)
        {
            var bullet = GameObjectPool.Get<Bullet>(BulletPrefabPath);
            bullet.OnHit += BulletHit;
            bullet.Init(gameObject,pos, dir, BulletSpeed);
        }

        private void BulletHit(Bullet bullet,HitInfo hitInfo)
        {
            bullet.OnHit -= BulletHit;
            HitSomething(hitInfo.GetEnumerable());
        }
    }
}
