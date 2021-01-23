using System;
using Framework;
using Thunder.UI;
using UnityEngine;
using Package = Framework.Package;

namespace Thunder
{
    [RequireComponent(typeof(FpsCamera))]
    [RequireComponent(typeof(FpsMover))]
    [RequireComponent(typeof(Starvation))]
    public class Player : BaseEntity, IHitAble
    {
        public static Player Ins;

        public float MaxHealth = 100;
        public float CurHealth = 100;
        public int PackageSize = 30;
        public float DropItemForce = 2;
        public float InteractiveRange = 2;
        public float SquattingOffset = 0.5f;
        public float PickupRange = 2;

        private Transform _WeaponAttachPoint;
        private Transform _PivotTrans;
        private InputSynchronizer _InteractiveSyn = new InputSynchronizer();

        public WeaponBelt WeaponBelt;
        public Dropper Dropper { private set; get; }
        public CommonPackage Package { private set; get; }
        public ItemCombiner ItemCombiner { private set; get; }
        public FpsCamera FpsCamera { private set; get; }
        public FpsMover FpsMover { private set; get; }
        public Starvation Starvation { private set; get; }

        protected override void Awake()
        {
            base.Awake();

            Ins = this;

            _PivotTrans = Trans.Find("Pivot");
            _WeaponAttachPoint = _PivotTrans.Find("Weapon");

            Package = new CommonPackage(PackageSize);
            WeaponBelt.Init(_WeaponAttachPoint);
            Dropper = new Dropper(DropItemForce, () => _PivotTrans.position, () => _PivotTrans.rotation);
            //ItemCombiner = new ItemCombiner(Package, DataBaseSys.GetTable("combine_expressions"));
            FpsCamera = GetComponent<FpsCamera>();
            FpsMover = GetComponent<FpsMover>();
            Starvation = GetComponent<Starvation>();

            PublicEvents.DropItem.AddListener(Drop);


        }

        private void Update()
        {
            _InteractiveSyn.Set(ControlSys.RequireKey(Config.InteractiveKeyName, 0));
            WeaponBelt.InputCheck();
        }

        private void FixedUpdate()
        {
            InteractiveDetect(
                _PivotTrans.position,
                _PivotTrans.rotation * Vector3.forward,
                InteractiveRange,
                _InteractiveSyn.Get());
        }

        private void OnDestroy()
        {
            Dropper.Destroy();
            WeaponBelt.Destroy();
            PublicEvents.DropItem.RemoveListener(Drop);
        }

        private static void InteractiveDetect(Vector3 startPos, Vector3 dir, float range, ControlInfo info)
        {
            var hits = Physics.RaycastAll(startPos, dir, range);
            if (hits.Length == 0) return;
            hits[0].transform.GetComponent<IInteractive>()?.Interactive(info);
        }

        public void Drop(ItemGroup group)
        {
            Dropper.Drop(group);
        }

        public void ReceiveItem(ItemGroup group)
        {
            int remaining;
            using (var info = WeaponBelt.PutItem(group, false))
                remaining = info.RemainingNum;
            if (remaining == 0)
                return;
            group.Count = remaining;

            using (var info = Package.PutItem(group, false))
                remaining = info.RemainingNum;
            if (remaining == 0)
                return;
            group.Count = remaining;

            Drop(group);
        }

        public void GetHit(Vector3 hitPos, Vector3 hitDir, float damage)
        {
            CurHealth -= damage;
            if (CurHealth <= 0)
            {
                CurHealth = 0;
                Dead();
            }

            Debug.Log(CurHealth);
        }

        private void Dead()
        {
            LogPanel.Ins.LogSystem("Game Over");
            Destroy(gameObject);
            PublicEvents.PlayerDead?.Invoke();
        }
    }

    public class Dropper
    {
        private readonly float _LaunchForce;
        private Func<Vector3> _Pos;
        private Func<Quaternion> _Rot;

        public Dropper(float launchForce, Func<Vector3> posGetter, Func<Quaternion> rotGetter)
        {
            _LaunchForce = launchForce;
            _Pos = posGetter;
            _Rot = rotGetter;
        }

        public void Drop(ItemGroup group)
        {
            var prefabPath = ItemSys.GetInfo(group.Id).PickPrefabPath;
            if (prefabPath.IsNullOrEmpty())
                prefabPath = Config.DefaultPickupableItemAssetPath;

            var item = ObjectPool.Get<PickupableItem>(prefabPath);
            var rot = _Rot();
            var force = rot * Vector3.forward * _LaunchForce;
            item.Launch(_Pos(), rot, force, group);
        }

        public void Destroy()
        {
            _Pos = null;
            _Rot = null;
        }
    }

    public class Picker
    {
        private readonly Package _Package;
        private readonly WeaponBelt _WeaponBelt;

        public Picker(Package package, WeaponBelt weaponBelt)
        {
            _Package = package;
            _WeaponBelt = weaponBelt;

            PublicEvents.PickupItem.AddListener(Pickup);
        }

        public void RemoveEventListener()
        {
            PublicEvents.PickupItem.RemoveListener(Pickup);
        }

        public void Pickup(ItemGroup group)
        {
            if (WeaponBelt.IsWeapon(group.Id))
            {
                _WeaponBelt.PutItem((group.Id, 1), false);
                return;
            }

            if (!_Package.CanPackage(group.Id)) return;
            _Package.PutItem(group, false);
        }
    }
}