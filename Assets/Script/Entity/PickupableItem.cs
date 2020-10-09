using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thunder.Sys;
using Thunder.Tool.ObjectPool;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Entity
{
    public class PickupableItem:BaseEntity,IItem,IObjectPool
    {
        public AssetId AssetId { get; set; }
        public int ItemId => _ItemId;

        [SerializeField]
        private int _ItemId;
        public float DropProtectedTime = 2;

        protected bool _CanPickup = true;
        private AutoCounter _DropCounter;
        private Rigidbody _Rb;

        protected override void Awake()
        {
            base.Awake();
            _CanPickup = false;
            _DropCounter = new AutoCounter(this, DropProtectedTime).
                OnComplete(() => _CanPickup = true).Complete(false);
            _Rb = GetComponent<Rigidbody>();
        }

        private void OnTriggerEnter(Collider collider)
        {
            var player = collider.GetComponent<Player>();
            if (player != null && _CanPickup)
                PublicEvents.PickupItem?.Invoke(ItemId);
        }

        public void Launch(Vector3 pos, Quaternion rot, Vector3 force)
        {
            _DropCounter.Recount();
            _CanPickup = false;
            _Trans.position = pos;
            _Trans.rotation = rot;
            _Rb.AddForce(force, ForceMode.Impulse);
        }

        public virtual void OpReset(){}

        public virtual void OpRecycle(){}

        public virtual void OpDestroy(){}

        protected virtual void Pickup()
        {
            ObjectPool.Ins.Recycle(this);
        }
    }
}
