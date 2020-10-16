using System;
using Thunder.Sys;
using Thunder.Tool;
using Thunder.Tool.BuffData;
using Thunder.Tool.ObjectPool;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Events;

namespace Thunder.Entity
{
    [RequireComponent(typeof(FpsCamera))]
    [RequireComponent(typeof(FpsMover))]
    public class Player : BaseEntity
    {
        public static Player Ins;

        //public int PackageSize;
        //public float DropItemForce = 2;
        //public float GroundRayLength = 0.1f;
        //public float GroundRayStartOffset = 0.05f;
        //public float InteractiveRange = 2;
        //public float JumpForce = 5;
        //public float JumpWeaponOffsetAngle = 7;
        //public bool Movable = true;
        //public float SquatOffset = 0.5f;
        //public float SquatWalkSpeedScale = 0.5f;
        //public Vector2 WalkShakeRange;
        //public float WalkShakeSpeed = 0.1f;
        //public float WalkSpeed = 0.1f;
        //public Vector2 WeaponWalkShakeRange;

        //[Range(0, 1)] public float ViewDampFactor = 0.7f;
        //[Range(0, 1)] public float WalkShakeDampFactor = 0.5f;
        //[Range(0, 1)] public float MoveDampFactor = 0.2f;
        //[Range(0, 1)] public float SmoothFactor = 0.2f;
        //[SerializeField] private Vector2 _Sensitive;

        //private bool _Squating;
        //private Vector2 _TargetRot;
        //private Vector3 _Velocity;
        //private float _WalkShakeDir = 1;
        //private float _WalkShakeOffsetT;
        //private float _WalkSpeedFactor = 1;
        //private Transform _WeaponAttachPoint;
        //private Vector2 _AdditionTargetRot;
        //private CapsuleCollider _CapsuleCol;
        //private Vector2 _CtrlDir;
        //private bool _FixedReaded;
        //private RaycastHit _GroundHit;
        //private Vector3 _GroundRayStartOffset;
        //private bool _Hanging;
        //private InputSynchronizer _InteractiveSynchronizer = new InputSynchronizer();
        //private Vector3 _PivotOffset;
        //private Transform _PivotTrans;
        //private Rigidbody _Rb;
        ///// <summary>
        /////     squating hanging
        ///// </summary>
        //[HideInInspector] public UnityEvent<bool, bool> OnHanging = new UnityEvent<bool, bool>();
        ///// <summary>
        /////     squating hanging
        ///// </summary>
        //[HideInInspector] public UnityEvent<bool, bool> OnSquat = new UnityEvent<bool, bool>();
        //[HideInInspector] public UnityEvent<bool> OnMove = new UnityEvent<bool>();
        //[HideInInspector] public BuffData SensitiveScale = 1;
        //[HideInInspector] public Vector3 WalkShakeOffset;
        //[HideInInspector] public bool Moving;
        //[HideInInspector] public bool Squatting;

        //public Dropper Dropper { private set; get; }
        //public WeaponBelt WeaponBelt { private set; get; }
        //public Package Package { private set; get; }
        //public ItemCombiner ItemCombiner { private set; get; }
        //public Vector2 Sensitive
        //{
        //    get => _Sensitive * SensitiveScale.CurData;
        //    set => _Sensitive = value;
        //}

        //protected override void Awake()
        //{
        //    base.Awake();

        //    Ins = this;

        //    _PivotTrans = _Trans.Find("Pivot");
        //    _PivotOffset = _PivotTrans.localPosition;
        //    _WeaponAttachPoint = _PivotTrans.Find("Weapon");
        //    _Rb = GetComponent<Rigidbody>();
        //    _CapsuleCol = GetComponent<CapsuleCollider>();

        //    _TargetRot.y = _Trans.localEulerAngles.y;
        //    _TargetRot.x = _PivotTrans.localEulerAngles.x;

        //    WeaponBelt = new WeaponBelt(GlobalSettings.WeaponBeltCellTypes, _WeaponAttachPoint);
        //    Dropper = new Dropper(DropItemForce, () => _PivotTrans.position, () => _PivotTrans.rotation);
        //    Package = new Package(PackageSize);
        //    ItemCombiner = new ItemCombiner(Package,DataBaseSys.Ins["combine_expressions"]);

        //    PublicEvents.DropItem.AddListener(Drop);
        //}

        //private void Update()
        //{
        //    // 视角
        //    _Trans.localEulerAngles = new Vector3(0,
        //        Tools.LerpAngle(_Trans.localEulerAngles.y, _TargetRot.y + _AdditionTargetRot.y, SmoothFactor), 0);
        //    _PivotTrans.localEulerAngles =
        //        new Vector3(
        //            Tools.LerpAngle(_PivotTrans.localEulerAngles.x, _TargetRot.x + _AdditionTargetRot.x, SmoothFactor),
        //            0, 0);


        //    // 移动
        //    Vector2 ctrlDir = ControlSys.Ins.RequireKey("Axis1", 0).Axis;
        //    ctrlDir = Movable ? ctrlDir : Vector2.zero;
        //    if (_FixedReaded || ctrlDir != Vector2.zero)
        //    {
        //        _CtrlDir = ctrlDir;
        //        _FixedReaded = false;
        //    }

        //    // 视角晃动
        //    var shakeTarget = Vector3.down * (_Squating ? SquatOffset : 0) + WalkShakeOffset.Mul(WalkShakeRange) +
        //                      _PivotOffset;
        //    _PivotTrans.localPosition = Vector3.Lerp(_PivotTrans.localPosition, shakeTarget, WalkShakeDampFactor);
        //    _WeaponAttachPoint.localPosition = Vector3.Lerp(_WeaponAttachPoint.localPosition,
        //        WalkShakeOffset.Mul(WeaponWalkShakeRange), WalkShakeDampFactor);

        //    // 根据垂直速度上下摆动
        //    var tempX = 1 - 1 / (Mathf.Abs(_Rb.velocity.y) + 1);
        //    tempX *= _Rb.velocity.y < 0 ? -1 : 1;
        //    tempX *= JumpWeaponOffsetAngle;
        //    _WeaponAttachPoint.localEulerAngles =
        //        new Vector3(Mathf.LerpAngle(_WeaponAttachPoint.localEulerAngles.x, tempX, WalkShakeDampFactor), 0);

        //    if (ControlSys.Ins.RequireKey("LockCursor", 0).Down)
        //        SwitchLockCursor();
        //    var c = ControlSys.Ins.RequireKey("Squat", 0);
        //    if (c.Down && Movable)
        //        Squat(true);
        //    else if (c.Up)
        //        Squat(false);
        //    if (ControlSys.Ins.RequireKey("Jump", 0).Down && Movable)
        //        Jump();

        //    WeaponBelt.InputCheck();

        //    _InteractiveSynchronizer.Set(ControlSys.Ins.RequireKey(GlobalSettings.InteractiveKeyName, 0));
        //}

        //private void FixedUpdate()
        //{
        //    // 视角
        //    Vector2 ctrlDir = ControlSys.Ins.RequireKey("Axis2", 0).Axis;

        //    var ctrlEuler = MoveToEuler(ctrlDir);
        //    if (ctrlDir != Vector2.zero)
        //        _TargetRot = EulerAdd(_TargetRot, ctrlEuler);


        //    // 移动
        //    _Velocity = Vector3.Lerp(_Velocity, _CtrlDir, MoveDampFactor);

        //    var tempVelocity = _Velocity;
        //    tempVelocity.z = tempVelocity.y;
        //    tempVelocity.y = 0;
        //    Vector3 moveDir = _PivotTrans.localToWorldMatrix * tempVelocity;
        //    moveDir = moveDir.ProjectToxz();
        //    var planeNormal = _GroundHit.normal == Vector3.zero ? Vector3.up : _GroundHit.normal;
        //    moveDir = Vector3.ProjectOnPlane(moveDir, planeNormal).normalized;

        //    _Rb.MovePosition(_Trans.position + moveDir * WalkSpeed);

        //    // 视角晃动
        //    if (_CtrlDir == Vector2.zero || _GroundHit.transform == null)
        //    {
        //        _WalkShakeDir = -1;
        //        _WalkShakeOffsetT = Mathf.PI / 2;
        //    }
        //    else
        //    {
        //        _WalkShakeOffsetT += _WalkShakeDir * WalkShakeSpeed;
        //        if (_WalkShakeOffsetT < 0)
        //            _WalkShakeOffsetT = Mathf.PI * 2;
        //    }

        //    // lemniscate of Gerono
        //    WalkShakeOffset.x = Mathf.Cos(_WalkShakeOffsetT);
        //    WalkShakeOffset.y = Mathf.Sin(_WalkShakeOffsetT * 2) / 2;

        //    _FixedReaded = true;

        //    // 地面检测
        //    _GroundRayStartOffset = _CapsuleCol.center + Vector3.down * GetCapusleHeight(_CapsuleCol.height) / 2 +
        //                            Vector3.up * GroundRayStartOffset;

        //    var casts = Physics.RaycastAll(_Trans.position + _GroundRayStartOffset, Vector3.down, GroundRayLength);
        //    _GroundHit = casts.Length != 0 ? casts[0] : new RaycastHit();
        //    if (!_Hanging && _GroundHit.transform == null)
        //    {
        //        _Hanging = true;
        //        OnHanging?.Invoke(_Squating, true);
        //    }
        //    else if (_Hanging && _GroundHit.transform != null)
        //    {
        //        _Hanging = false;
        //        OnHanging?.Invoke(_Squating, false);
        //    }

        //    InteractiveDetect(
        //        _PivotTrans.position,
        //        _PivotTrans.rotation * Vector3.forward,
        //        InteractiveRange,
        //        _InteractiveSynchronizer.Get());
        //}

        //public void ViewRotAddition(Vector2 rot)
        //{
        //    _AdditionTargetRot = MoveToEuler(rot);
        //}

        //public void ViewRotTargetAddition(Vector2 rot)
        //{
        //    _TargetRot = EulerAdd(_TargetRot, MoveToEuler(rot));
        //}

        //public void Drop(ItemGroup group)
        //{
        //    Dropper.Drop(group);
        //}

        //private void Squat(bool down)
        //{
        //    if (_Hanging) return;

        //    if (down && !_Squating)
        //    {
        //        _CapsuleCol.height -= SquatOffset;
        //        _Trans.position += Vector3.down * SquatOffset / 2;
        //        _Squating = true;

        //        var factor = SquatWalkSpeedScale / _WalkSpeedFactor;
        //        WalkSpeed *= factor;
        //        _WalkSpeedFactor = SquatWalkSpeedScale;

        //        OnSquat?.Invoke(true, _Hanging);
        //    }
        //    else if (!down && _Squating)
        //    {
        //        _CapsuleCol.height += SquatOffset;
        //        _Trans.position += Vector3.up * SquatOffset / 2;
        //        _Squating = false;

        //        var factor = 1 / _WalkSpeedFactor;
        //        WalkSpeed *= factor;
        //        _WalkSpeedFactor = 1;

        //        OnSquat?.Invoke(false, _Hanging);
        //    }
        //}

        //private void Jump()
        //{
        //    if (_GroundHit.transform == null || _Squating)
        //        return;
        //    _Rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
        //}

        //private float GetCapusleHeight(float rawHeight)
        //{
        //    var radius2 = _CapsuleCol.radius * 2;
        //    return rawHeight > radius2 ? rawHeight : radius2;
        //}

        //private Vector2 MoveToEuler(Vector2 move)
        //{
        //    return new Vector2(-move.y, move.x) * Sensitive;
        //}

        //private static void InteractiveDetect(Vector3 startPos, Vector3 dir, float range, ControlInfo info)
        //{
        //    var hits = Physics.RaycastAll(startPos, dir, range);
        //    if (hits.Length == 0) return;
        //    hits[0].transform.GetComponent<IInteractive>().Interactive(info);
        //}

        //private static Vector2 EulerAdd(Vector2 e1, Vector2 e2)
        //{
        //    e1.y += e2.y;
        //    e1.x = Mathf.Clamp(e1.x + e2.x, -90, 90);

        //    return e1;
        //}

        //private static void SwitchLockCursor()
        //{
        //    ControlSys.Ins.LockCursor = !ControlSys.Ins.LockCursor;
        //}

        //private void OnDrawGizmosSelected()
        //{
        //    Gizmos.color = Color.red;
        //    Gizmos.DrawLine(transform.position + _GroundRayStartOffset,
        //        transform.position + _GroundRayStartOffset + Vector3.down * GroundRayLength);
        //}

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

        protected override void Awake()
        {
            base.Awake();

            Ins = this;

            _PivotTrans = _Trans.Find("Pivot");
            _WeaponAttachPoint = _PivotTrans.Find("Weapon");

            WeaponBelt = new WeaponBelt(GlobalSettings.WeaponBeltCellTypes, _WeaponAttachPoint);
            Dropper = new Dropper(DropItemForce, () => _PivotTrans.position, () => _PivotTrans.rotation);
            Package = new Package(PackageSize);
            ItemCombiner = new ItemCombiner(Package, DataBaseSys.Ins["combine_expressions"]);
            FpsCamera = GetComponent<FpsCamera>();
            FpsMover = GetComponent<FpsMover>();
            FpsCamera.SquattingOffset = FpsMover.SquattingOffset = SquattingOffset;

            PublicEvents.DropItem.AddListener(Drop);
        }

        private void Update()
        {
            _InteractiveSyn.Set(ControlSys.Ins.RequireKey(GlobalSettings.InteractiveKeyName, 0));
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
            var item = ObjectPool.Ins.Alloc<PickupableItem>(ItemSys.Ins[group.Id].PickPrefabPath);
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