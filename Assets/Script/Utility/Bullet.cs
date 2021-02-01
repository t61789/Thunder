using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;
using UnityEngine;

namespace Thunder
{
    public class Bullet:BaseEntity,IObjectPool
    {
        public float LifeTime = 5f;
        public event Action<Bullet,HitInfo> OnHit; 

        private Rigidbody _Rb;
        private GameObject _Avoid;
        private SimpleCounter _LifeTimeCounter;

        protected override void Awake()
        {
            base.Awake();
            _Rb = GetComponent<Rigidbody>();
            _LifeTimeCounter = new SimpleCounter(LifeTime);
        }

        private void OnTriggerEnter(Collider col)
        {
            if (col.gameObject == _Avoid) return;

            var hitInfo = new HitInfo()
            {
                Collider = col,
                HitDir = Trans.rotation * Vector3.forward,
                HitPos = Trans.position
            };

            OnHit?.Invoke(this,hitInfo);
            GameObjectPool.Put(this);
        }

        public void Init(GameObject avoid,Vector3 pos, Vector3 dir, float speed)
        {
            _Avoid = avoid;
            Trans.position = pos;
            Trans.rotation = Quaternion.LookRotation(dir);
            _Rb.velocity = dir.normalized * speed;
            _LifeTimeCounter.Recount();
        }

        private void FixedUpdate()
        {
            if (_LifeTimeCounter.Completed)
            {
                GameObjectPool.Put(this);
            }
        }

        public AssetId AssetId { get; set; }

        public void OpReset() { }

        public void OpPut() { }

        public void OpDestroy() { }
    }
}
