using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thunder.Entity;
using Thunder.Sys;
using Thunder.Tool;
using Thunder.Tool.BuffData;
using UnityEngine;
using UnityEngine.Events;

namespace Thunder.Utility
{
    /// <summary>
    /// 期望结构：
    /// Mover-Pivot/Weapon-Camera
    /// 脚本挂载于Mover之上
    /// </summary>
    public class FpsCamera:MonoBehaviour
    {
        public Vector2 WalkShakeRange;
        public float WalkShakeTime = 0.1f;
        public float WalkSpeed = 0.1f;
        public Vector2 WeaponWalkShakeRange;
        public float SquattingOffset;
        public float JumpWeaponOffsetAngle;
        [Range(0, 1)] public float ViewDampFactor = 0.7f;
        [Range(0, 1)] public float WalkShakeDampFactor = 0.5f;
        [Range(0, 1)] public float SmoothFactor = 0.2f;
        [SerializeField] private Vector2 _Sensitive;

        private Transform _Trans;
        private Rigidbody _Rb;
        private Vector2 _TargetRot;
        private Transform _WeaponAttachPoint;
        private Vector2 _AdditionTargetRot;
        private Vector3 _PivotOffset;
        private Transform _PivotTrans;
        private SimpleCounter _ShakeCounter;
        private bool _Moving;
        private bool _Dangling;
        private bool _Squatting;
        [HideInInspector] public BuffData SensitiveScale = new BuffData(1);

        private const float PI_3D2 = 3 * Mathf.PI / 2;
        private const float PI_7D2 = 7 * Mathf.PI / 2;
        private const float PI_2 = 2 * Mathf.PI;

        public Vector2 Sensitive
        {
            get => _Sensitive * SensitiveScale;
            set => _Sensitive = value;
        }

        private void Awake()
        {
            _Trans = transform;
            _Rb = GetComponent<Rigidbody>();
            _PivotTrans = _Trans.Find("Pivot");
            _PivotOffset = _PivotTrans.localPosition;
            _WeaponAttachPoint = _PivotTrans.Find("Weapon");

            _TargetRot.y = _Trans.localEulerAngles.y;
            _TargetRot.x = _PivotTrans.localEulerAngles.x;

            _ShakeCounter = new SimpleCounter(WalkShakeTime);

            PublicEvents.RecoliFloat.AddListener(ViewRotAddition);
            PublicEvents.RecoliFixed.AddListener(ViewRotTargetAddition);

            PublicEvents.PlayerMove.AddListener(x=>_Moving=x);
            PublicEvents.PlayerDangling.AddListener(x => _Dangling = x);
            PublicEvents.PlayerSquat.AddListener(x => _Squatting = x);
        }

        private void FixedUpdate()
        {
            //视角，读取输入
            Vector2 ctrlDir = ControlSys.Ins.RequireKey("Axis2", 0).Axis;

            var ctrlEuler = MoveToEuler(ctrlDir);
            if (ctrlDir != Vector2.zero)
                _TargetRot = EulerAdd(_TargetRot, ctrlEuler);
        }

        private void Update()
        {
            SetViewRot();

            SetPivotWeaponWalkShakeOffset(GetWalkShakeOffset());

            _WeaponAttachPoint.localEulerAngles = new Vector3(GetWeaponVerticalPitch(),0);

            if (ControlSys.Ins.RequireKey("LockCursor", 0).Down)
                SwitchLockCursor();
        }

        private void OnDestroy()
        {
            PublicEvents.RecoliFloat.RemoveListener(ViewRotAddition);
            PublicEvents.RecoliFixed.RemoveListener(ViewRotTargetAddition);
        }

        private void SetViewRot()
        {
            //视角
            _Trans.localEulerAngles = new Vector3(0,
                Tools.LerpAngle(_Trans.localEulerAngles.y, _TargetRot.y + _AdditionTargetRot.y, SmoothFactor), 0);
            _PivotTrans.localEulerAngles =
                new Vector3(
                    Tools.LerpAngle(_PivotTrans.localEulerAngles.x, _TargetRot.x + _AdditionTargetRot.x, SmoothFactor),
                    0, 0);
        }

        private Vector3 GetWalkShakeOffset()
        {
            if (!_Moving || _Dangling)
                _ShakeCounter.Recount();

            float t = Tools.LerpUc(PI_3D2, PI_7D2, _ShakeCounter.InterpolantUc);
            t = t.Repeat(PI_2);

            //lemniscate of Gerono
            return new Vector3(Mathf.Cos(t),
                Mathf.Sin(t * 2) / 2);
        }

        private void SetPivotWeaponWalkShakeOffset(Vector3 walkShakeOffset)
        {
            var shakeTarget = Vector3.down * (_Squatting ? SquattingOffset : 0) + walkShakeOffset.Mul(WalkShakeRange) +
                              _PivotOffset;
            _PivotTrans.localPosition = Vector3.Lerp(_PivotTrans.localPosition, shakeTarget, WalkShakeDampFactor);
            _WeaponAttachPoint.localPosition = Vector3.Lerp(_WeaponAttachPoint.localPosition,
                walkShakeOffset.Mul(WeaponWalkShakeRange), WalkShakeDampFactor);
        }

        private float GetWeaponVerticalPitch()
        {
            //根据垂直速度返回武器的俯仰角
            var tempX = 1 - 1 / (Mathf.Abs(_Rb.velocity.y) + 1);
            tempX *= _Rb.velocity.y < 0 ? -1 : 1;
            tempX *= JumpWeaponOffsetAngle;
            return Mathf.LerpAngle(_WeaponAttachPoint.localEulerAngles.x, tempX, WalkShakeDampFactor);
        }

        public void ViewRotAddition(Vector2 rot)
        {
            _AdditionTargetRot = MoveToEuler(rot);
        }

        public void ViewRotTargetAddition(Vector2 rot)
        {
            _TargetRot = EulerAdd(_TargetRot, MoveToEuler(rot));
        }

        private Vector2 MoveToEuler(Vector2 move)
        {
            return new Vector2(-move.y, move.x) * Sensitive;
        }

        private static Vector2 EulerAdd(Vector2 e1, Vector2 e2)
        {
            e1.y += e2.y;
            e1.x = Mathf.Clamp(e1.x + e2.x, -90, 90);

            return e1;
        }

        private static void SwitchLockCursor()
        {
            ControlSys.Ins.LockCursor = !ControlSys.Ins.LockCursor;
        }
    }
}
