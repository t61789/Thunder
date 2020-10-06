using Thunder.Sys;
using Thunder.Tool.ObjectPool;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Entity.Weapon
{
    public class CrossbowArrow:BaseEntity,IObjectPool
    {
        public float LifeTime;
        public float Damage;

        private AutoCounter _LifeTimeCounter;
        private Rigidbody _Rb;
        private bool _HitSomeone;
        private float _CarryDamage;

        protected override void Awake()
        {
            base.Awake();
            _LifeTimeCounter = new AutoCounter(this,LifeTime).
                OnComplete(()=>ObjectPool.Ins.Recycle(this)).Complete(false);
            _Rb = GetComponent<Rigidbody>();
        }

        public void Install(Transform parent, Vector3 localPos)
        {
            _Trans.SetParent(parent);
            _Trans.localPosition = localPos;
            _Trans.rotation = parent.rotation;
        }

        public void Launch(Vector3 impulseForce,float carryDamage)
        {
            _CarryDamage = carryDamage;
            _Trans.SetParent(Stable.Container);
            _Rb.useGravity = true;
            _Rb.AddForce(impulseForce, ForceMode.Impulse);
            _LifeTimeCounter.Recount();
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (_HitSomeone) return;

            collider.GetComponent<IShootable>()?.
                GetShoot(_Trans.position, _Rb.velocity.normalized, Damage + _CarryDamage);

            _Rb.velocity = Vector3.zero;
            _Rb.useGravity = false;
            _HitSomeone = true;
            _Trans.SetParent(collider.transform);
        }

        private void FixedUpdate()
        {
            if (_Rb.useGravity)
                _Trans.rotation = Quaternion.LookRotation(_Rb.velocity);
        }

        public AssetId AssetId { get; set; }

        public void BeforeOpReset()
        {
            _HitSomeone = false;
            _LifeTimeCounter.Complete(false);
            _Rb.useGravity = false;
        }

        public void BeforeOpRecycle()
        {
            
        }

        public void AfterOpDestroy()
        {
        }
    }
}
