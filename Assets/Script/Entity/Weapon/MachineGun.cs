using System;
using System.Collections.Generic;
using Framework;

using UnityEngine;
using UnityEngine.Assertions;

namespace Thunder
{
    [RequireComponent(typeof(RecoilController))]
    [RequireComponent(typeof(AimScopeController))]
    public class MachineGun : BaseWeapon
    {
        public CtrlKey FireKey;
        public CtrlKey ReloadKey;
        public CtrlKey AimKey;
        public CtrlKey SwitchFireKey;

        public float CameraRecoilDampTime = 0.1f;
        public float Damage = 20;
        public string DrawAnimationName;
        public Vector3 MuzzleFirePos;
        public Sprite[] MuzzleFireSprites;

        [SerializeField] private RecoilController _Recoil;
        [SerializeField] private AimScopeController _AimScope;

        private readonly List<Vector3> _Spreads = new List<Vector3>();
        private readonly StickyInputDic _StickyInputDic = new StickyInputDic();
        private bool _AimScopeBeforeReload;
        private bool _Reloading;
        private Animator _Animator;

        private const string RELOAD = "Reload";
        private const string FIRE = "Fire";

        public override float OverHeatFactor => 0;

        public override void Init(Transform weaponContainer, string addData)
        {
            base.Init(weaponContainer, addData);
            _Animator = GetComponent<Animator>();
            _StickyInputDic.AddBool(RELOAD, 0.7f);
            _Recoil.Init();
            _AimScope.Init(player.FpsCamera.SensitiveScale);

            _Recoil.OnFixedRecoil += InvokeOnFixedRecoil;
        }

        private void Update()
        {
            var autoReload = Fire(ControlSys.RequireKey(FireKey));
            Reload(ControlSys.RequireKey(ReloadKey), autoReload);

            if (ControlSys.RequireKey(AimKey).Down)
                _AimScope.Switch();
            if (ControlSys.RequireKey(SwitchFireKey).Down) { }
            //MachineGunFireControl.LoopBurstMode();

            _Animator.SetBool(RELOAD, _StickyInputDic.GetBool(RELOAD));
        }

        private void FixedUpdate()
        {
            _StickyInputDic.FixedUpdate();
            _Recoil.FixedUpdate();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _AimScope.OnDestroy();
            _Recoil.OnFixedRecoil -= InvokeOnFixedRecoil;
        }

        public override void Reload()
        {
            AmmoGroup.Reload();

            if (_AimScopeBeforeReload)
                _AimScope.Switch();

            _Reloading = false;
        }

        public override void TakeOut()
        {
            _Animator.Play(DrawAnimationName);
        }

        public override void PutBack()
        {
            _Recoil.Resets();
            _AimScope.Resets();
            _AimScopeBeforeReload = _Reloading = false;
        }

        public override string CompressItem()
        {
            return AmmoGroup.Magazine.ToString();
        }

        public override void DecompressItem(string add)
        {
            if (string.IsNullOrEmpty(add)) return;
            AmmoGroup.Magazine = int.Parse(add);
        }

        public override Action<HitInfo> GetBulletHitHook()
        {
            return BulletHit;
        }

        public override void Fire()
        {
            var autoReload = Fire(ControlInfo.Open);
            Reload(ControlInfo.Open, autoReload);
        }

        public override Vector2 GetFloatRecoil()
        {
            return _Recoil.CurFloatRecoil;
        }

        private bool Fire(ControlInfo fireInfo)
        {
            using (FireInfo shootInfo = ShootTrigger(Trans, fireInfo.Down, fireInfo.Stay))
            {
                _Animator.SetBool(FIRE, shootInfo.HasShot);

                if (shootInfo.HasShot)
                {
                    PublicEvents.GunFire?.Invoke();
                    _Recoil.Next();
                }

                return shootInfo.FireCheckPass &&
                       shootInfo.BurstRequirePass &&
                       shootInfo.IntervalPass &&
                       shootInfo.SafetyPass &&
                       !shootInfo.AmmoPass &&
                       fireInfo.Down;
            }
        }

        private void Reload(ControlInfo reloadInfo,bool autoReload)
        {
            var param = AmmoGroup.ReloadConfirm() &&
                        !_Reloading &&
                        (reloadInfo.Down || autoReload);

            _StickyInputDic.SetBool(RELOAD, param);
            if (param && !_Reloading)
            {
                _AimScopeBeforeReload = _AimScope.Enable;
                if (_AimScope.Enable)
                    _AimScope.Switch();
                _Reloading = true;
            }
        }

        private void BulletHit(HitInfo hitInfo)
        {
            hitInfo.Collider.GetComponent<IHitAble>().GetHit(
                hitInfo.HitPos,
                hitInfo.HitDir,
                Damage);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            foreach (var pos in _Spreads) Gizmos.DrawSphere(pos, 0.1f);
        }
    }

    [Serializable]
    public class RecoilController
    {
        public Vector2 BaseDir = Vector2.up;
        public float Angle = 30;
        public Range Intensity = (0.1f, 0.2f);
        public float ForwardTime = 0.1f;
        public float BackTime = 0.7f;
        [Range(0,1)]
        public float FixedPercent = 0.2f;
        public event Action<Vector2> OnFixedRecoil;

        private float _RecoilAngleHalf;
        private Vector2 _RecoilTarget;
        private bool _Back;
        private SemiAutoCounter _RecoilCounter;

        public Vector2 CurFloatRecoil
        {
            get
            {
                var start = _Back ? _RecoilTarget : Vector2.zero;
                var end = _Back ? Vector2.zero : _RecoilTarget;

                return Vector2.Lerp(start, end, _RecoilCounter.Interpolant);
            }
        }

        public void Init()
        {
            _Back = true;
            BaseDir = BaseDir.normalized;
            _RecoilAngleHalf = Angle / 2;
            _RecoilCounter = new SemiAutoCounter(ForwardTime).OnComplete(SwitchDir);
        }

        public void FixedUpdate()
        {
            _RecoilCounter.FixedUpdate();
        }

        /// <summary>
        /// 产生一次后坐力
        /// </summary>
        /// <returns>当前浮动后坐力位置</returns>
        public void Next()
        {
            OnFixedRecoil?.Invoke(CurFloatRecoil * FixedPercent);

            float angle = Tools.RandomFloat(-_RecoilAngleHalf, _RecoilAngleHalf);
            _RecoilTarget = Quaternion.AngleAxis(angle, Vector3.forward) *
                   BaseDir *
                   Tools.RandomFloat(Intensity.Min, Intensity.Max);
            _Back = false;
            _RecoilCounter.Recount(ForwardTime);

            //OnFixedRecoil?.Invoke(_RecoilTarget*FixedPercent);
        }

        public void Resets()
        {
            _Back = false;
            _RecoilCounter.Complete(false);
            _RecoilTarget = Vector2.zero;
        }

        private void SwitchDir()
        {
            if (_Back) return;

            _Back = true;
            _RecoilCounter.Recount(BackTime);
        }
    }
    
    [Serializable]
    public class AimScopeController
    {
        public float EnableFOV = 20;
        public bool Allowed = true;

        private BuffData _PlayerSensitive;
        private Camera _MainCamera;
        private float _BaseFOV;
        private BuffData _AimScopeSensitiveScale;

        private const string AIM_POINT_UI_NAME = "aimPoint";

        public void Init(BuffData playerSensitive)
        {
            _MainCamera = Camera.main;
            _BaseFOV = _MainCamera.fieldOfView;
            _AimScopeSensitiveScale = new BuffData(EnableFOV / _BaseFOV);
            _PlayerSensitive = playerSensitive;
        }

        public void OnDestroy()
        {
            _AimScopeSensitiveScale.Destroy();
        }

        public bool Enable { private set; get; }

        public void Resets()
        {
            if (Enable)
                Switch();
        }

        public void Switch()
        {
            if (!Allowed) return;

            if (!Enable)
            {
                _MainCamera.fieldOfView = EnableFOV;
                PostProcessingController.Instance.AimScope.Enable = true;
                UiSys.CloseUi(AIM_POINT_UI_NAME);
                _PlayerSensitive.AddBuff(_AimScopeSensitiveScale, Operator.Mul, 0);
                PostProcessingController.Instance.DepthOfField.Enable = true;
            }
            else
            {
                _MainCamera.fieldOfView = _BaseFOV;
                PostProcessingController.Instance.AimScope.Enable = false;
                UiSys.OpenUi(AIM_POINT_UI_NAME);
                _PlayerSensitive.RemoveBuff(_AimScopeSensitiveScale);
                PostProcessingController.Instance.DepthOfField.Enable = false;
            }

            Enable = !Enable;
        }
    }
}