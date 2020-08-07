using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thunder.Sys;
using Thunder.Tool;
using UnityEngine;

namespace Thunder.Entity
{
    public class Player : BaseEntity
    {
        [Range(0,1)]
        public float SensitiveTotal = 0.5f;
        public float SensitiveX = 1;
        public float SensitiveY = 1;
        [Range(0, 1)]
        public float SmoothFactor = 0.2f;
        public float GroundRayStartOffset = 0.05f;
        public float GroundRayLength = 0.1f;
        public float MoveSpeed = 0.5f;
        [Range(0,1)]
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

        private Transform _PivotTrans;
        private Transform _WeaponAttachPoint;
        private Rigidbody _Rb;
        private CapsuleCollider _CapsuleCol;
        private Vector3 _GroundRayStart;
        private RaycastHit _GroundHit;
        private Vector3 _Velocity;
        private Vector2 _TargetRot;
        private float _WalkShakeDir = 1;
        private Vector3 _WeaponAttachPointOffset;
        private float _WalkShakeOffsetT;
        private Vector2 _AdditionRot;
        private Vector2 _AdditionTargetRot;

        protected override void Awake()
        {
            base.Awake();

            _PivotTrans = _Trans.Find("Pivot");
            _WeaponAttachPoint = _PivotTrans.Find("Weapon");
            _WeaponAttachPointOffset = _WeaponAttachPoint.localPosition;
            _Rb = GetComponent<Rigidbody>();
            _CapsuleCol = GetComponent<CapsuleCollider>();
            _GroundRayStart = _Trans.position + _CapsuleCol.center + Vector3.down * _CapsuleCol.height / 2 +
                              Vector3.up * GroundRayStartOffset;

            _TargetRot.y = _Trans.localEulerAngles.y;
            _TargetRot.x = _PivotTrans.localEulerAngles.x;
        }

        private void Update()
        {
            // 视角
            Vector2 ctrlDir = Stable.Control.RequireKey("Axis2",0).Axis;
            ctrlDir += _AdditionTargetRot;
            _AdditionTargetRot = Vector2.zero;

            if (ctrlDir != Vector2.zero)
            {
                _TargetRot.y += ctrlDir.x * SensitiveTotal * 2 * SensitiveX;
                _TargetRot.x = Mathf.Clamp(_TargetRot.x - ctrlDir.y * SensitiveTotal * 2 * SensitiveY, -90, 90);
            }

            float tempX = _TargetRot.x + _AdditionRot.x;
            tempX = tempX < 0 ? tempX + 360 : tempX;

            _Trans.localEulerAngles = new Vector3(0, Mathf.LerpAngle(_Trans.localEulerAngles.y,_TargetRot.y + _AdditionRot.y, SmoothFactor), 0);
            _PivotTrans.localEulerAngles = new Vector3(Mathf.LerpAngle(_PivotTrans.localEulerAngles.x, tempX, SmoothFactor), 0, 0);

            // 移动
            ctrlDir = Stable.Control.RequireKey("Axis1", 0).Axis;
            _Velocity = Vector3.Lerp(_Velocity,ctrlDir,MoveDampFactor);

            Vector3 tempVelocity = _Velocity;
            tempVelocity.z = tempVelocity.y;
            tempVelocity.y = 0;
            Vector3 moveDir = _PivotTrans.localToWorldMatrix * tempVelocity;
            moveDir = moveDir.ProjectToxz();
            Vector3 planeNormal = _GroundHit.normal == Vector3.zero ? Vector3.up : _GroundHit.normal;
            moveDir = Vector3.ProjectOnPlane(moveDir, planeNormal).normalized;

            // 视角晃动
            if (ctrlDir == Vector2.zero)
            {
                _WalkShakeDir = -1;
                _WalkShakeOffsetT = Mathf.PI/2;
            }
            else
            {
                _WalkShakeOffsetT += _WalkShakeDir * WalkShakeSpeed;
                if (_WalkShakeOffsetT < 0)
                    _WalkShakeOffsetT = Mathf.PI * 2;
            }

            // lemniscate of Gerono
            WalkShakeOffset.x = Mathf.Cos(_WalkShakeOffsetT);
            WalkShakeOffset.y = Mathf.Sin(_WalkShakeOffsetT *2)/2;
            _PivotTrans.localPosition = Vector3.Lerp(_PivotTrans.localPosition, WalkShakeOffset.VecMul(WalkShakeRange), WalkShakeDampFactor);
            _WeaponAttachPoint.localPosition = _WeaponAttachPointOffset + Vector3.Lerp(_WeaponAttachPoint.localPosition- _WeaponAttachPointOffset, WalkShakeOffset.VecMul(WeaponWalkShakeRange), WeaponWalkShakeDampFactor);


            _Rb.MovePosition(_Trans.position + moveDir * MoveSpeed);

            if (Stable.Control.RequireKey("LockCursor", 0).Down)
                SwitchLockCursor();
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
            var casts = Physics.RaycastAll(_GroundRayStart, Vector3.down, GroundRayLength);
            _GroundHit = casts.Length != 0 ? casts[0] : new RaycastHit();
        }

        private static void SwitchLockCursor()
        {
            Stable.Control.LockCursor = !Stable.Control.LockCursor;
        }
    }
}
