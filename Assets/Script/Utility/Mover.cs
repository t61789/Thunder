using Framework;
using UnityEngine;

namespace Thunder
{
    public abstract class Mover : MonoBehaviour
    {
        public bool Moveable = true;
        public float GroundRayStartOffset = 0.1f;
        public float JumpForce = 5;
        public float MoveDampFactor = 0.2f;
        public float WalkSpeed = 0.1f;
        public float GroundRayLength = 0.2f;
        public CtrlMode SquatCtrlMode = CtrlMode.Stay;
        public float SquattingOffset = 0.5f;
        public float SquatWalkSpeedScale = 0.5f;

        public float CurSpeed => _Velocity.magnitude;

        protected Transform Trans;
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
            Trans = transform;
            _CapsuleCol = GetComponent<CapsuleCollider>();
            _Rb = GetComponent<Rigidbody>();

            _Dangling = new SwitchTrigger(x => PublicEvents.PlayerDangling?.Invoke(x));
            _Moving = new SwitchTrigger(x => PublicEvents.PlayerMove?.Invoke(x));
        }

        private void Update()
        {
            _MoveSyner.Set(GetMoveControl());
            _JumpSyner.Set(GetJumpControl());
            _SquatSyner.Set(GetSquatControl());
        }

        private void FixedUpdate()
        {
            var groundRayhit = GroundDetect();
            _Dangling.Check(groundRayhit.normal == Vector3.zero);
            Move(_MoveSyner.Get().Axis, groundRayhit.normal);
            HandleSquatJump(_JumpSyner.Get(), _SquatSyner.Get(), groundRayhit.normal);
        }

        private void OnDrawGizmosSelected()
        {
            var col = GetComponent<CapsuleCollider>();
            var offset = transform.position + col.center + Vector3.down * col.height / 2 +
                         Vector3.up * GroundRayStartOffset;

            Gizmos.DrawLine(offset, offset + Vector3.down * GroundRayLength);
        }

        private void Move(Vector3 moveDir, Vector3 groundNormal)
        {
            if (!Moveable) return;
            _Moving.Check(moveDir != Vector3.zero);
            _Velocity = Vector3.Lerp(_Velocity, moveDir, MoveDampFactor);

            var planeNormal = groundNormal == Vector3.zero ? Vector3.up : groundNormal;
            moveDir = Vector3.ProjectOnPlane(moveDir, planeNormal);

            _Rb.MovePosition(Trans.position + moveDir * WalkSpeed);
        }

        private void HandleSquatJump(ControlInfo jumpCtrl, ControlInfo squatCtrl, Vector3 groundNormal)
        {
            switch (SquatCtrlMode)
            {
                case CtrlMode.Stay when _Squatting && squatCtrl.Up:
                    _Squatting = false;
                    Squat(false);
                    PublicEvents.PlayerSquat?.Invoke(false);
                    break;
                case CtrlMode.Stay when !_Squatting && squatCtrl.Down:
                    _Squatting = true;
                    Squat(true);
                    PublicEvents.PlayerSquat?.Invoke(true);
                    break;
                case CtrlMode.Switch when squatCtrl.Down:
                    _Squatting = !_Squatting;
                    Squat(_Squatting);
                    PublicEvents.PlayerSquat?.Invoke(_Squatting);
                    break;
            }

            if (jumpCtrl.Down && groundNormal != Vector3.zero)
            {
                _Rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
                PublicEvents.PlayerJump?.Invoke();
            }
        }

        private RaycastHit GroundDetect()
        {
            var offset = _CapsuleCol.center + Vector3.down * GetCapsuleHeight(_CapsuleCol.height) / 2 +
                                    Vector3.up * GroundRayStartOffset;

            var casts = Physics.RaycastAll(Trans.position + offset, Vector3.down, GroundRayLength);
            return casts.Length != 0 ? casts[0] : new RaycastHit();
        }

        private float GetCapsuleHeight(float rawHeight)
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

        protected abstract ControlInfo GetMoveControl();

        protected abstract ControlInfo GetJumpControl();

        protected abstract ControlInfo GetSquatControl();
    }
}
