using Thunder.Sys;
using Thunder.Tool.ObjectPool;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Entity
{
    public class PickupableItem : BaseEntity, IItem, IObjectPool, IInteractive
    {
        protected bool _CanPickup;
        private AutoCounter _DropCounter;

        [SerializeField] private int _ItemId;

        private Rigidbody _Rb;
        public PickupItemAction Action = PickupItemAction.All;
        public float DropProtectedTime = 2;

        public void Interactive(ControlInfo info)
        {
            if (!info.Down || (Action & PickupItemAction.Directed) == 0) return;
            Pickup();
        }

        public int ItemId => _ItemId;
        public AssetId AssetId { get; set; }

        public virtual void OpReset()
        {
        }

        public virtual void OpRecycle()
        {
        }

        public virtual void OpDestroy()
        {
        }

        protected override void Awake()
        {
            base.Awake();
            _DropCounter = new AutoCounter(this, DropProtectedTime).OnComplete(() => _CanPickup = true).Complete(false);
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

        protected virtual void Pickup()
        {
            PublicEvents.PickupItem?.Invoke(ItemId);
            _CanPickup = false;
            ObjectPool.Ins.Recycle(this);
        }
    }
}