using Framework;

using UnityEngine;

namespace Thunder
{
    public class CrossbowArrow : BaseEntity, IObjectPool
    {
        private float _CarryDamage;
        private bool _HitSomeone;

        private AutoCounter _LifeTimeCounter;
        private Rigidbody _Rb;
        public float Damage;
        public float LifeTime;

        public AssetId AssetId { get; set; }

        public void OpReset()
        {
            _HitSomeone = false;
            _LifeTimeCounter.Complete(false);
            _Rb.useGravity = false;
        }

        public void OpPut()
        {
        }

        public void OpDestroy()
        {
        }

        protected override void Awake()
        {
            base.Awake();
            _LifeTimeCounter = new AutoCounter(this, LifeTime).OnComplete(() => ObjectPool.Put(this))
                .Complete(false);
            _Rb = GetComponent<Rigidbody>();
        }

        public void Install(Transform parent, Vector3 localPos)
        {
            Trans.SetParent(parent);
            Trans.localPosition = localPos;
            Trans.rotation = parent.rotation;
        }

        public void Launch(Vector3 impulseForce, float carryDamage)
        {
            _CarryDamage = carryDamage;
            Trans.SetParent(GameCore.Container);
            _Rb.useGravity = true;
            _Rb.AddForce(impulseForce, ForceMode.Impulse);
            _LifeTimeCounter.Recount();
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (_HitSomeone) return;

            collider.GetComponent<IShootable>()
                ?.GetShoot(Trans.position, _Rb.velocity.normalized, Damage + _CarryDamage);

            _Rb.velocity = Vector3.zero;
            _Rb.useGravity = false;
            _HitSomeone = true;
            Trans.SetParent(collider.transform);
        }

        private void FixedUpdate()
        {
            if (_Rb.useGravity)
                Trans.rotation = Quaternion.LookRotation(_Rb.velocity);
        }
    }
}