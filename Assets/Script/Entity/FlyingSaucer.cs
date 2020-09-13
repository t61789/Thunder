using Thunder.Sys;
using Thunder.Tool.ObjectPool;
using Thunder.Utility;
using Thunder.Utility.Events;
using UnityEngine;

namespace Thunder.Entity
{
    public class FlyingSaucer : BaseEntity, IShootable, IObjectPool
    {
        public float ForceScale;
        public float LifeTime = 5;

        protected Rigidbody _Rb;
        protected float _LifeTimeCount;


        protected override void Awake()
        {
            base.Awake();
            _Rb = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            _LifeTimeCount = Time.time;
        }

        private void FixedUpdate()
        {
            _Trans.rotation = Quaternion.LookRotation(_Rb.velocity);
            if (Time.time - _LifeTimeCount > LifeTime)
                Stable.ObjectPool.Recycle(this);
        }

        public void Launch(Vector3 force)
        {
            _Rb.AddForce(force, ForceMode.Impulse);
        }

        public void GetShoot(Vector3 hitPos, Vector3 hitDir, float damage)
        {
            _Rb.AddForceAtPosition(hitDir.normalized * damage * ForceScale, hitPos, ForceMode.Impulse);
            Stable.ObjectPool.Alloc<SelfDestroyPartical>(null, null, "hitParticle", x => x.transform.position = _Trans.position);
            Stable.ObjectPool.Recycle(this);
            FlyingSaucerHit.Event?.Invoke();
        }

        public void BeforeOpReset()
        {

        }

        public void BeforeOpRecycle()
        {

        }

        public void AfterOpDestroy()
        {

        }

        public AssetId AssetId { get; set; }
    }
}
