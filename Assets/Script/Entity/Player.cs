using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thunder.Sys;
using Thunder.Tool;
using UnityEngine;
using UnityEngine.Events;

namespace Thunder.Entity
{
    public class Player : BaseEntity
    {
        [Range(0, 1)]
        public float SensitiveTotal = 0.5f;
        public float SensitiveX = 1;
        public float SensitiveY = 1;
        [Range(0, 1)]
        public float SmoothFactor = 0.2f;
        public float GroundRayStartOffset = 0.05f;
        public float GroundRayLength = 0.1f;
        public float WalkSpeed = 0.1f;
        [Range(0, 1)]
        public float MoveDampFactor = 0.2f;
        public float WalkShakeSpeed = 0.1f;
        public Vector2 WalkShakeRange;
        [Range(0, 1)]
        public float WalkShakeDampFactor = 0.5f;
        public Vector2 WeaponWalkShakeRange;
        [Range(0, 1)]
        public float WeaponWalkShakeDampFactor = 0.5f;
        [HideInInspector]
        public Vector3 WalkShakeOffset;
        /// <summary>
        /// squating hanging
        /// </summary>
        [HideInInspector]
        public UnityEvent<bool, bool> OnSquat;
        /// <summary>
        /// squating hanging
        /// </summary>
        [HideInInspector]
        public UnityEvent<bool, bool> OnHanging;
        [HideInInspector]
        public UnityEvent<bool> OnMove;
        public float JumpForce = 5;
        public float SquatOffset = 0.5f;
        public float JumpWeaponOffsetAngle = 7;
        public float SquatWalkSpeedScale = 0.5f;

        private Transform _PivotTrans;
        private Transform _WeaponAttachPoint;
        private Rigidbody _Rb;
        private CapsuleCollider _CapsuleCol;
        private Vector3 _GroundRayStartOffset;
        private RaycastHit _GroundHit;
        private Vector3 _Velocity;
        private Vector2 _TargetRot;
        private float _WalkShakeDir = 1;
        private Vector3 _WeaponAttachPointOffset;
        private float _WalkShakeOffsetT;
        private Vector2 _AdditionRot;
        private Vector2 _AdditionTargetRot;
        private bool _Squating;
        private bool _Hanging;
        private float _WalkSpeedFactor = 1;
        private Vector3 _TargetMoveDir;

        protected override void Awake()
        {
            base.Awake();

            _PivotTrans = _Trans.Find("Pivot");
            _WeaponAttachPoint = _PivotTrans.Find("Weapon");
            _WeaponAttachPointOffset = _WeaponAttachPoint.localPosition;
            _Rb = GetComponent<Rigidbody>();
            _CapsuleCol = GetComponent<CapsuleCollider>();

            _TargetRot.y = _Trans.localEulerAngles.y;
            _TargetRot.x = _PivotTrans.localEulerAngles.x;
        }

        private void Update()
        {
            // 视角
            Vector2 ctrlDir = Stable.Control.RequireKey("Axis2", 0).Axis;
            ctrlDir += _AdditionTargetRot;
            _AdditionTargetRot = Vector2.zero;

            if (ctrlDir != Vector2.zero)
            {
                _TargetRot.y += ctrlDir.x * SensitiveTotal * 2 * SensitiveX;
                _TargetRot.x = Mathf.Clamp(_TargetRot.x - ctrlDir.y * SensitiveTotal * 2 * SensitiveY, -90, 90);
            }

            float tempX = _TargetRot.x + _AdditionRot.x;
            tempX = tempX < 0 ? tempX + 360 : tempX;

            _Trans.localEulerAngles = new Vector3(0, Mathf.LerpAngle(_Trans.localEulerAngles.y, _TargetRot.y + _AdditionRot.y, SmoothFactor), 0);
            _PivotTrans.localEulerAngles = new Vector3(Mathf.LerpAngle(_PivotTrans.localEulerAngles.x, tempX, SmoothFactor), 0, 0);

            // 移动
            ctrlDir = Stable.Control.RequireKey("Axis1", 0).Axis;
            _Velocity = Vector3.Lerp(_Velocity, ctrlDir, MoveDampFactor);

            Vector3 tempVelocity = _Velocity;
            tempVelocity.z = tempVelocity.y;
            tempVelocity.y = 0;
            Vector3 moveDir = _PivotTrans.localToWorldMatrix * tempVelocity;
            moveDir = moveDir.ProjectToxz();
            Vector3 planeNormal = _GroundHit.normal == Vector3.zero ? Vector3.up : _GroundHit.normal;
            moveDir = Vector3.ProjectOnPlane(moveDir, planeNormal).normalized;

            _TargetMoveDir = moveDir;

            // 视角晃动
            if (ctrlDir == Vector2.zero || _GroundHit.transform == null)
            {
                _WalkShakeDir = -1;
                _WalkShakeOffsetT = Mathf.PI / 2;
            }
            else
            {
                _WalkShakeOffsetT += _WalkShakeDir * WalkShakeSpeed;
                if (_WalkShakeOffsetT < 0)
                    _WalkShakeOffsetT = Mathf.PI * 2;
            }

            // lemniscate of Gerono
            WalkShakeOffset.x = Mathf.Cos(_WalkShakeOffsetT);
            WalkShakeOffset.y = Mathf.Sin(_WalkShakeOffsetT * 2) / 2;
            _PivotTrans.localPosition = Vector3.Lerp(_PivotTrans.localPosition, WalkShakeOffset.VecMul(WalkShakeRange) + Vector3.down * (_Squating ? SquatOffset : 0), WalkShakeDampFactor);
            _WeaponAttachPoint.localPosition = _WeaponAttachPointOffset + Vector3.Lerp(_WeaponAttachPoint.localPosition - _WeaponAttachPointOffset, WalkShakeOffset.VecMul(WeaponWalkShakeRange), WeaponWalkShakeDampFactor);

            tempX = 1 - 1 / (Mathf.Abs(_Rb.velocity.y) + 1);
            tempX *= _Rb.velocity.y < 0 ? -1 : 1;
            tempX *= JumpWeaponOffsetAngle;

            _WeaponAttachPoint.localEulerAngles = new Vector3(Mathf.LerpAngle(_WeaponAttachPoint.localEulerAngles.x,tempX, WalkShakeDampFactor),0);


            if (Stable.Control.RequireKey("LockCursor", 0).Down)
                SwitchLockCursor();
            ControlInfo c = Stable.Control.RequireKey("Squat", 0);
            if (c.Down)
                Squat(true);
            else if (c.Up)
                Squat(false);
            if (Stable.Control.RequireKey("Jump", 0).Down)
                Jump();
        }

        public void ViewRotAddition(Vector2 rot)
        {
            _AdditionRot = rot;
        }

        public void ViewRotTargetAddition(Vector2 rot)
        {
            _AdditionTargetRot.x = rot.y;
            _AdditionTargetRot.y = -rot.x;
        }

        private void FixedUpdate()
        {
            _GroundRayStartOffset = _CapsuleCol.center + Vector3.down * GetCapusleHeight(_CapsuleCol.height) / 2 +
                                    Vector3.up * GroundRayStartOffset;

            var casts = Physics.RaycastAll(_Trans.position + _GroundRayStartOffset, Vector3.down, GroundRayLength);
            _GroundHit = casts.Length != 0 ? casts[0] : new RaycastHit();
            if (!_Hanging && _GroundHit.transform == null)
            {
                _Hanging = true;
                OnHanging?.Invoke(_Squating, true);
            }
            else if (_Hanging && _GroundHit.transform != null)
            {
                _Hanging = false;
                OnHanging?.Invoke(_Squating, false);
            }

            _Rb.MovePosition(_Trans.position + _TargetMoveDir * WalkSpeed);
        }

        private static void SwitchLockCursor()
        {
            Stable.Control.LockCursor = !Stable.Control.LockCursor;
        }

        private void Squat(bool down)
        {
            if (_Hanging) return;

            if (down && !_Squating)
            {
                _CapsuleCol.height -= SquatOffset;
                _Trans.position += Vector3.down * SquatOffset / 2;
                _Squating = true;

                float factor = SquatWalkSpeedScale / _WalkSpeedFactor;
                WalkSpeed *= factor;
                _WalkSpeedFactor = SquatWalkSpeedScale;

                OnSquat?.Invoke(true, _Hanging);
            }
            else if (!down && _Squating)
            {
                _CapsuleCol.height += SquatOffset;
                _Trans.position += Vector3.up * SquatOffset / 2;
                _Squating = false;

                float factor = 1 / _WalkSpeedFactor;
                WalkSpeed *= factor;
                _WalkSpeedFactor = 1;

                OnSquat?.Invoke(false, _Hanging);
            }
        }

        private void Jump()
        {
            if (_GroundHit.transform == null || _Squating)
                return;
            _Rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
        }

        private float GetCapusleHeight(float rawHeight)
        {
            float radius2 = _CapsuleCol.radius * 2;
            return rawHeight > radius2 ? rawHeight : radius2;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + _GroundRayStartOffset, transform.position + _GroundRayStartOffset + Vector3.down * GroundRayLength);
        }
    }
}
