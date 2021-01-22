using Framework;

using UnityEngine;

namespace Thunder
{
    public class PickupableItem : BaseEntity, IObjectPool, IInteractive
    {
        public PickupItemAction Action = PickupItemAction.All;
        public float DropProtectedTime = 1.5f;
        public ItemGroup ItemGroup;

        private Transform _Model;
        private Rigidbody _Rb;
        private SimpleCounter _DropCounter;

        protected override void Awake()
        {
            base.Awake();
            _DropCounter = new SimpleCounter(DropProtectedTime);
            _Rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            CheckPlayer();
        }

        private void CheckPlayer()
        {
            if (!_DropCounter.Completed || 
                Player.Ins==null ||
                !Action.HasFlag(PickupItemAction.UnDirected)) return;
            
            if((Trans.position - Player.Ins.Trans.position).sqrMagnitude <= Player.Ins.PickupRange* Player.Ins.PickupRange)
                Pickup();
        }

        public void Launch(Vector3 pos, Quaternion rot, Vector3 force,ItemGroup group)
        {
            _DropCounter.Recount();
            Trans.position = pos;
            Trans.rotation = rot;
            ItemGroup = group;
            _Rb.AddForce(force, ForceMode.Impulse);
        }

        protected virtual void Pickup()
        {
            Player.Ins.ReceiveItem(ItemGroup);
            ObjectPool.Put(this);
            PublicEvents.PickupItem?.Invoke(ItemGroup);
        }

        public void Interactive(ControlInfo info)
        {
            if (!info.Down || (Action & PickupItemAction.Directed) == 0) return;
            Pickup();
        }

        public AssetId AssetId { get; set; }

        public virtual void OpReset() { }

        public virtual void OpPut() { }

        public virtual void OpDestroy() { }
    }
}