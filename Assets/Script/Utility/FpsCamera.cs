
using Framework;
using UnityEngine;

namespace Thunder
{
    /// <summary>
    /// 期望结构：
    /// Mover-Pivot/Weapon-Camera
    /// 脚本挂载于Mover之上
    /// </summary>
    [RequireComponent(typeof(FpsMover))]
    public class FpsCamera : MonoBehaviour
    {
        public Vector2 WalkShakeRange;
        public float WalkShakeTime = 0.1f;
        public float WalkShakeDampTime = 0.7f;
        public float WalkSpeed = 0.1f;
        public Vector2 WeaponWalkShakeRange;
        public float JumpWeaponOffsetAngle;
        [Range(0, 1)] public float ViewDampFactor = 0.7f;
        [Range(0, 1)] public float WalkShakeDampFactor = 0.2f;
        [Range(0, 1)] public float SmoothFactor = 0.2f;
        [SerializeField] private Vector2 _Sensitive = new Vector2(0.3f,0.3f);

        public static FpsCamera Ins { private set; get; }

        private Camera _Camera;
        private Transform _Trans;
        private Rigidbody _Rb;
        private Vector2 _TargetRot;
        private Transform _WeaponAttachPoint;
        private Vector2 _AdditionTargetRot;
        private Vector3 _PivotOffset;
        private Transform _PivotTrans;
        private SimpleCounter _ShakeCounter;
        private BaseWeapon _Weapon;
        private bool _Dangling;
        private bool _Squatting;
        private FpsMover _Mover;
        [HideInInspector] public BuffData SensitiveScale = new BuffData(1);

        private const float PI_3D2 = 3 * Mathf.PI / 2;
        private const float PI_7D2 = 7 * Mathf.PI / 2;

        public Vector2 Sensitive
        {
            get => _Sensitive * SensitiveScale;
            set => _Sensitive = value;
        }

        private void Awake()
        {
            Ins = this;

            _Trans = transform;
            _Rb = GetComponent<Rigidbody>();
            _PivotTrans = _Trans.Find("Pivot");
            _Camera = _PivotTrans.Find("PlayerCamera").GetComponent<Camera>();
            _PivotOffset = _PivotTrans.localPosition;
            _WeaponAttachPoint = _PivotTrans.Find("Weapon");
            _Mover = GetComponent<FpsMover>();

            _TargetRot.y = _Trans.localEulerAngles.y;
            _TargetRot.x = _PivotTrans.localEulerAngles.x;

            _ShakeCounter = new SimpleCounter(WalkShakeTime);

            PublicEvents.PlayerDangling.AddListener(SetDangling);
            PublicEvents.PlayerSquat.AddListener(SetSquatting);

            PublicEvents.TakeOutWeapon.AddListener(BindWeapon);
            PublicEvents.PutBackWeapon.AddListener(UnBindWeapon);

            PublicEvents.StartingBuildingMode.AddListener(SwitchEnable);
            PublicEvents.EndBuildingMode.AddListener(SwitchEnable);
        }

        private void Update()
        {
            Vector2 ctrlDir = ControlSys.RequireKey("Axis2", 0).Axis;
            var ctrlEuler = MoveToEuler(ctrlDir);
            if (ctrlDir != Vector2.zero)
                _TargetRot = EulerAdd(_TargetRot, ctrlEuler);

            SetViewRot();

            SetPivotWeaponWalkShakeOffset();

            _WeaponAttachPoint.localEulerAngles = new Vector3(GetWeaponVerticalPitch(), 0);

            if (ControlSys.RequireKey("LockCursor", 0).Down)
                SwitchLockCursor();
        }

        private void FixedUpdate()
        {
            if (_Weapon != null)
            {
                _AdditionTargetRot = MoveToEuler(_Weapon.GetFloatRecoil());
            }
        }

        private void OnDestroy()
        {
            PublicEvents.PlayerDangling.RemoveListener(SetDangling);
            PublicEvents.PlayerSquat.RemoveListener(SetSquatting);

            PublicEvents.TakeOutWeapon.RemoveListener(BindWeapon);
            PublicEvents.PutBackWeapon.RemoveListener(UnBindWeapon);

            PublicEvents.StartingBuildingMode.RemoveListener(SwitchEnable);
            PublicEvents.EndBuildingMode.RemoveListener(SwitchEnable);
        }

        private void SwitchEnable()
        {
            _Camera.enabled = !_Camera.enabled;
        }

        private void SwitchEnable(Process p)
        {
            _Camera.enabled = !_Camera.enabled;
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

        private void SetPivotWeaponWalkShakeOffset()
        {
            float curSpeed = _Mover.CurSpeed;
            if (_Dangling || Tools.IfApproximate(0,curSpeed,0.001))
                _ShakeCounter.Recount();

            float t = Tools.LerpUc(PI_3D2, PI_7D2, _ShakeCounter.InterpolantUc);

            //lemniscate of Gerono
            var walkShakeOffset = new Vector3(Mathf.Cos(t),
                Mathf.Sin(t * 2) / 2);

            var shakeTarget = Vector3.down * (_Squatting ? _Mover.SquattingOffset : 0) + walkShakeOffset;
            var lerp = Tools.InLerp(0, _Mover.WalkSpeed, curSpeed);
            shakeTarget = Tools.Lerp(Vector3.zero, shakeTarget, lerp);

            _PivotTrans.localPosition = Tools.Lerp(
                _PivotTrans.localPosition, 
                shakeTarget.Mul(WalkShakeRange) + _PivotOffset,
                WalkShakeDampFactor);

            _WeaponAttachPoint.localPosition = Tools.Lerp(
                _WeaponAttachPoint.localPosition,
                -shakeTarget.Mul(WeaponWalkShakeRange),
                  WalkShakeDampFactor);
        }

        private float GetWeaponVerticalPitch()
        {
            //根据垂直速度返回武器的俯仰角
            var tempX = 1 - 1 / (Mathf.Abs(_Rb.velocity.y) + 1);
            tempX *= _Rb.velocity.y < 0 ? -1 : 1;
            tempX *= JumpWeaponOffsetAngle;
            return Mathf.LerpAngle(_WeaponAttachPoint.localEulerAngles.x, tempX, WalkShakeDampFactor);
        }

        private void AddFixedRecoil(Vector2 rot)
        {
            _TargetRot = EulerAdd(_TargetRot, MoveToEuler(rot));
        }

        private Vector2 MoveToEuler(Vector2 move)
        {
            return new Vector2(-move.y, move.x) * Sensitive;
        }

        private void SetDangling(bool dangling)
        {
            _Dangling = dangling;
        }

        private void SetSquatting(bool squatting)
        {
            _Squatting = squatting;
        }

        private void BindWeapon(BaseWeapon weapon)
        {
            _Weapon = weapon;
            _Weapon.OnFixedRecoil += AddFixedRecoil;
        }

        private void UnBindWeapon(BaseWeapon weapon)
        {
            if (_Weapon == null) return;
            _Weapon.OnFixedRecoil -= AddFixedRecoil;
            _Weapon = null;
        }

        private static Vector2 EulerAdd(Vector2 e1, Vector2 e2)
        {
            e1.y += e2.y;
            e1.x = Mathf.Clamp(e1.x + e2.x, -90, 90);

            return e1;
        }

        private static void SwitchLockCursor()
        {
            ControlSys.LockCursor = !ControlSys.LockCursor;
        }
    }
}
