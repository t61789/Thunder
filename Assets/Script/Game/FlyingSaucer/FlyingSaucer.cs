using Framework;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Game.FlyingSaucer
{
    public class FlyingSaucer : BaseEntity, IShootable, IObjectPool
    {
        protected float _LifeTimeCount;

        protected Rigidbody _Rb;
        public float FlyingDirForceFactor;
        public float ForceScale;
        public float LifeTime = 5;

        public void OpReset()
        {
        }

        public void OpPut()
        {
        }

        public void OpDestroy()
        {
        }

        public AssetId AssetId { get; set; }

        public void GetShoot(Vector3 hitPos, Vector3 hitDir, float damage)
        {
            _Rb.AddForceAtPosition(hitDir.normalized * damage * ForceScale, hitPos, ForceMode.Impulse);
            ObjectPool.Get<SelfDestroyPartical>("hitParticle")
                .transform.position = Trans.position;
            ObjectPool.Put(this);
            PublicEvents.FlyingSaucerHit?.Invoke();
        }


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
            var flyingForceFactor = _Rb.velocity.sqrMagnitude * FlyingDirForceFactor;

            Trans.rotation =
                Quaternion.Lerp(Trans.rotation, Quaternion.LookRotation(_Rb.velocity), flyingForceFactor);
            if (Time.time - _LifeTimeCount > LifeTime)
                ObjectPool.Put(this);
        }

        public void Launch(Vector3 force)
        {
            _Rb.AddForce(force, ForceMode.Impulse);
        }
    }
}