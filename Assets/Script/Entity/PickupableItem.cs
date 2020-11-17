﻿using Framework;

using UnityEngine;

namespace Thunder
{
    public class PickupableItem : BaseEntity, IObjectPool, IInteractive,IItem
    {
        public PickupItemAction Action = PickupItemAction.All;
        public float DropProtectedTime = 2;
        public int Count=1;

        protected bool CanPickup;
        private Rigidbody _Rb;
        private AutoCounter _DropCounter;

        public void Interactive(ControlInfo info)
        {
            if (!info.Down || (Action & PickupItemAction.Directed) == 0) return;
                Pickup();
        }

        public AssetId AssetId { get; set; }

        public virtual void OpReset()
        {
        }

        public virtual void OpPut()
        {
        }

        public virtual void OpDestroy()
        {
        }

        public ItemId ItemId { get; set; }

        protected override void Awake()
        {
            base.Awake();
            _DropCounter = new AutoCounter(this, DropProtectedTime).OnComplete(() => CanPickup = true).Complete(false);
            _Rb = GetComponent<Rigidbody>();
        }

        private void OnTriggerEnter(Collider collider)
        {
            var player = collider.GetComponent<Player>();
            if (player == null || !CanPickup || (Action & PickupItemAction.UnDirected) == 0) return;
            Pickup();
        }

        public void Launch(Vector3 pos, Quaternion rot, Vector3 force,int count)
        {
            _DropCounter.Recount();
            CanPickup = false;
            Trans.position = pos;
            Trans.rotation = rot;
            _Rb.AddForce(force, ForceMode.Impulse);
            Count = count;
        }

        protected virtual void Pickup()
        {
            PublicEvents.PickupItem?.Invoke((ItemId, Count));
            CanPickup = false;
            ObjectPool.Put(this);
        }

    }
}