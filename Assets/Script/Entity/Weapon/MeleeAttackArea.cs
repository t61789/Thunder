using System;
using Framework;

using UnityEngine;

namespace Thunder
{
    public class MeleeAttackArea : BaseEntity, IObjectPool
    {
        private BoxCollider _Collider;
        private AutoCounter _LifeTimeCounter;

        public AssetId AssetId { get; set; }

        public void OpReset()
        {
        }

        public void OpPut()
        {
        }

        public void OpDestroy()
        {
        }

        public event Action<Collider> HitEnter;
        public event Action<Collider> HitStay;
        public event Action<Collider> HitExit;

        protected override void Awake()
        {
            base.Awake();

            _LifeTimeCounter = new AutoCounter(this, 0).OnComplete(() => { ObjectPool.Put(this); })
                .Complete(false);
            _Collider = GetComponent<BoxCollider>();
        }

        public void Init(Vector3 startPos, Quaternion rot, Vector3 areaSize, float lifeTime)
        {
            _Collider.size = areaSize;
            _Collider.center = Vector3.forward * areaSize.z / 2;
            Trans.position = startPos;
            Trans.rotation = rot;
            _LifeTimeCounter.Recount(lifeTime);
        }

        private void OnTriggerEnter(Collider col)
        {
            HitEnter?.Invoke(col);
        }

        private void OnTriggerStay(Collider col)
        {
            HitStay?.Invoke(col);
        }

        private void OnTriggerExit(Collider col)
        {
            HitExit?.Invoke(col);
        }
    }
}