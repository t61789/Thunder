using Thunder.Sys;
using Thunder.Tool.ObjectPool;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Entity
{
    public class PickupableItem:BaseEntity,IItem,IObjectPool,IInteractive
    {
        public AssetId AssetId { get; set; }
        public int ItemId => _ItemId;
        
        [SerializeField]
        private int _ItemId;
        public float DropProtectedTime = 2;
        public PickupItemAction Action = PickupItemAction.All;

        protected bool _CanPickup;
        private AutoCounter _DropCounter;
        private Rigidbody _Rb;

        protected override void Awake()
        {
            base.Awake();
            _DropCounter = new AutoCounter(this, DropProtectedTime).
                OnComplete(() => _CanPickup = true).Complete(false);
            _Rb = GetComponent<Rigidbody>();
        }

        private void OnTriggerEnter(Collider collider)
        {
            var player = collider.GetComponent<Player>();
            if (player == null || !_CanPickup || (Action & PickupItemAction.UnDirected) == 0) return;
                Pickup();
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
            PublicEvents.PickupItem?.Invoke(ItemId);
            _CanPickup = false;
            ObjectPool.Ins.Recycle(this);
        }

        public void Interactive(ControlInfo info)
        {
            if (!info.Down || (Action & PickupItemAction.Directed) == 0) return;
                Pickup();
        }
    }
}
