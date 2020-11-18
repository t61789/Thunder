using System;
using Framework;

using UnityEngine;
using Package = Framework.Package;

namespace Thunder
{
    [RequireComponent(typeof(FpsCamera))]
    [RequireComponent(typeof(FpsMover))]
    [RequireComponent(typeof(Starvation))]
    public class Player : BaseEntity
    {
        public static Player Ins;

        public int PackageSize=30;
        public float DropItemForce = 2;
        public float InteractiveRange = 2;
        public float SquattingOffset = 0.5f;

        private Transform _WeaponAttachPoint;
        private Transform _PivotTrans;
        private InputSynchronizer _InteractiveSyn = new InputSynchronizer();

        public Dropper Dropper { private set; get; }
        public WeaponBelt WeaponBelt { private set; get; }
        public Package Package { private set; get; }
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

            WeaponBelt = new WeaponBelt(Config.WeaponBeltCellTypes, _WeaponAttachPoint);
            Dropper = new Dropper(DropItemForce, () => _PivotTrans.position, () => _PivotTrans.rotation);
            Package = new Package(PackageSize);
            ItemCombiner = new ItemCombiner(Package, DataBaseSys.GetTable("combine_expressions"));
            FpsCamera = GetComponent<FpsCamera>();
            FpsMover = GetComponent<FpsMover>();
            FpsCamera.SquattingOffset = FpsMover.SquattingOffset = SquattingOffset;
            Starvation = GetComponent<Starvation>();

            PublicEvents.DropItem.AddListener(Drop);
        }

        private void Update()
        {
            _InteractiveSyn.Set(ControlSys.RequireKey(Config.InteractiveKeyName, 0));
        }

        private void FixedUpdate() 
        {
            InteractiveDetect(
                _PivotTrans.position,
                _PivotTrans.rotation * Vector3.forward,
                InteractiveRange,
                _InteractiveSyn.Get());
        }

        private static void InteractiveDetect(Vector3 startPos, Vector3 dir, float range, ControlInfo info)
        {
            var hits = Physics.RaycastAll(startPos, dir, range);
            if (hits.Length == 0) return;
            hits[0].transform.GetComponent<IInteractive>().Interactive(info);
        }

        public void Drop(ItemGroup group)
        {
            Dropper.Drop(group);
        }
    }

    public class Dropper
    {
        private readonly float _LaunchForce;
        private readonly Func<Vector3> _Pos;
        private readonly Func<Quaternion> _Rot;

        public Dropper(float launchForce, Func<Vector3> posGetter, Func<Quaternion> rotGetter)
        {
            _LaunchForce = launchForce;
            _Pos = posGetter;
            _Rot = rotGetter;
        }

        public void Drop(ItemGroup group)
        {
            var item = ObjectPool.Get<PickupableItem>(ItemSys.Ins[group.Id].PickPrefabPath);
            item.ItemId = group.Id;
            var rot = _Rot();
            var force = rot * Vector3.forward * _LaunchForce;
            item.Launch(_Pos(), rot, force,group.Count);
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

        public void Pickup(ItemGroup group)
        {
            if (_WeaponBelt.IsWeapon(group.Id))
            {
                _WeaponBelt.AddWeapon(group.Id);
                return;
            }

            if (!_Package.CanPackage(group.Id)) return;
            _Package.AddItem(group, out _);
        }
    }
}