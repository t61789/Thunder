using Thunder.Entity;
using Thunder.Sys;
using Thunder.Tool;
using Thunder.Tool.ObjectPool;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Game.FlyingSaucer
{
    public class FlyingSaucer : BaseEntity, IShootable, IObjectPool
    {
        public float ForceScale;
        public float LifeTime = 5;
        public float FlyingDirForceFactor;

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
            float flyingForceFactor = _Rb.velocity.sqrMagnitude * FlyingDirForceFactor;

            _Trans.rotation = Quaternion.Lerp(_Trans.rotation,Quaternion.LookRotation(_Rb.velocity),flyingForceFactor);
            if (Time.time - _LifeTimeCount > LifeTime)
                ObjectPool.Ins.Recycle(this);
        }

        public void Launch(Vector3 force)
        {
            _Rb.AddForce(force, ForceMode.Impulse);
        }

        public void GetShoot(Vector3 hitPos, Vector3 hitDir, float damage)
        {
            _Rb.AddForceAtPosition(hitDir.normalized * damage * ForceScale, hitPos, ForceMode.Impulse);
            ObjectPool.Ins.Alloc<SelfDestroyPartical>(null, null, "hitParticle", x => x.transform.position = _Trans.position);
            ObjectPool.Ins.Recycle(this);
            PublicEvents.FlyingSaucerHit?.Invoke();
        }

        public void OpReset()
        {

        }

        public void OpRecycle()
        {

        }

        public void OpDestroy()
        {

        }

        public AssetId AssetId { get; set; }
    }
}
