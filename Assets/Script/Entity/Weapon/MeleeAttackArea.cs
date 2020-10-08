using System;
using System.Collections.Generic;
using System.Linq;
using Thunder.Sys;
using Thunder.Tool.ObjectPool;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Entity.Weapon
{
    public class MeleeAttackArea:BaseEntity,IObjectPool
    {
        public event Action<Collider> HitEnter;
        public event Action<Collider> HitStay;
        public event Action<Collider> HitExit;

        private BoxCollider _Collider;
        private AutoCounter _LifeTimeCounter;

        protected override void Awake()
        {
            base.Awake();

            _LifeTimeCounter = new AutoCounter(this,0).OnComplete(() =>
            {
                ObjectPool.Ins.Recycle(this);
            }).Complete(false);
            _Collider = GetComponent<BoxCollider>();
        }

        public void Init(Vector3 startPos, Quaternion rot,Vector3 areaSize,float lifeTime)
        {
            _Collider.size = areaSize;
            _Collider.center = Vector3.forward * areaSize.z / 2;
            _Trans.position = startPos;
            _Trans.rotation = rot;
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

        public AssetId AssetId { get; set; }
        public void BeforeOpReset()
        {
        }

        public void BeforeOpRecycle()
        {
        }

        public void AfterOpDestroy()
        {
        }
    }
}
