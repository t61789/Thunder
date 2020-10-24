using System;
using Thunder.Sys;
using Thunder.Tool;
using UnityEngine;

namespace Thunder.Utility
{
    public class FpsMover : MonoBehaviour
    {
        public bool Moveable = true;
        public float GroundRayStartOffset;
        public float JumpForce;
        public float MoveDampFactor;
        public float WalkSpeed;
        public float GroundRayLength;
        public CtrlMode SquatCtrlMode;
        public float SquattingOffset;
        public float SquatWalkSpeedScale;

        private Transform _Trans;
        private Vector3 _Velocity;
        private CapsuleCollider _CapsuleCol;
        private bool _Squatting;
        private SwitchTrigger _Moving;
        private SwitchTrigger _Dangling;
        private Rigidbody _Rb;
        private InputSynchronizer _MoveSyner = new InputSynchronizer();
        private InputSynchronizer _JumpSyner = new InputSynchronizer();
        private InputSynchronizer _SquatSyner = new InputSynchronizer();

        private void Awake()
        {
            _Trans = transform;
            _CapsuleCol = GetComponent<CapsuleCollider>();
            _Rb = GetComponent<Rigidbody>();

            _Dangling = new SwitchTrigger(x=> PublicEvents.PlayerDangling?.Invoke(x));
            _Moving = new SwitchTrigger(x=>PublicEvents.PlayerMove?.Invoke(x));
        }

        private void Update()
        {
            _MoveSyner.Set(ControlSys.Ins.RequireKey("Axis1", 0));
            _JumpSyner.Set(ControlSys.Ins.RequireKey("Jump", 0));
            _SquatSyner.Set(ControlSys.Ins.RequireKey("Squat", 0));
        }

        private void FixedUpdate()
        {
            var groundRayhit = GroundDetect();
            _Dangling.Check(groundRayhit.normal==Vector3.zero);
            Move(_MoveSyner.Get(), groundRayhit.normal);
            HandleSquatJump(_JumpSyner.Get(), _SquatSyner.Get(), groundRayhit.normal);
        }

        private void Move(ControlInfo rawctrl, Vector3 groundNormal)
        {
            if (!Moveable) return;
            _Moving.Check(rawctrl.Axis==Vector3.zero);
            _Velocity = Vector3.Lerp(_Velocity, rawctrl.Axis, MoveDampFactor);

            var tempVelocity = _Velocity;
            tempVelocity.z = tempVelocity.y;
            tempVelocity.y = 0;
            Vector3 moveDir = _Trans.localToWorldMatrix * tempVelocity;
            moveDir = moveDir.ProjectToxz();
            var planeNormal = groundNormal == Vector3.zero ? Vector3.up : groundNormal;
            moveDir = Vector3.ProjectOnPlane(moveDir, planeNormal).normalized;

            _Rb.MovePosition(_Trans.position + moveDir * WalkSpeed);
        }

        private void HandleSquatJump(ControlInfo jumpctrl, ControlInfo squatctrl, Vector3 groundNormal)
        {
            switch (SquatCtrlMode)
            {
                case CtrlMode.Stay when _Squatting && squatctrl.Up:
                    _Squatting = false;
                    Squat(false);
                    PublicEvents.PlayerSquat?.Invoke(false);
                    break;
                case CtrlMode.Stay when !_Squatting && squatctrl.Down:
                    _Squatting = true;
                    Squat(true);
                    PublicEvents.PlayerSquat?.Invoke(true);
                    break;
                case CtrlMode.Switch when squatctrl.Down:
                    _Squatting = !_Squatting;
                    Squat(_Squatting);
                    PublicEvents.PlayerSquat?.Invoke(_Squatting);
                    break;
            }

            if (jumpctrl.Down && groundNormal != Vector3.zero)
            {
                _Rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
                PublicEvents.PlayerJump?.Invoke();
            }
        }

        private RaycastHit GroundDetect()
        {
            var offset = _CapsuleCol.center + Vector3.down * GetCapusleHeight(_CapsuleCol.height) / 2 +
                                    Vector3.up * GroundRayStartOffset;

            var casts = Physics.RaycastAll(_Trans.position + offset, Vector3.down, GroundRayLength);
            return casts.Length != 0 ? casts[0] : new RaycastHit();
        }

        private float GetCapusleHeight(float rawHeight)
        {
            var radius2 = _CapsuleCol.radius * 2;
            return rawHeight > radius2 ? rawHeight : radius2;
        }

        private void Squat(bool down)
        {
            _CapsuleCol.height += (down ? -1 : 1) * SquattingOffset;
            _CapsuleCol.center += (down ? -1 : 1) * Vector3.up * SquattingOffset / 2;
            if (down)
                WalkSpeed *= SquatWalkSpeedScale;
            else
                WalkSpeed /= SquatWalkSpeedScale;
        }
    }
}
