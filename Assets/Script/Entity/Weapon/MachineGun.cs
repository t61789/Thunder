using System;
using System.Collections.Generic;
using Framework;

using UnityEngine;
using UnityEngine.Assertions;

namespace Thunder
{
    [RequireComponent(typeof(MachineGunFireControl))]
    [RequireComponent(typeof(RecoliController))]
    [RequireComponent(typeof(AimScopeController))]
    public class MachineGun : BaseWeapon
    {
        public CtrKeyRequest FireKey;
        public CtrKeyRequest ReloadKey;
        public CtrKeyRequest AimKey;
        public CtrKeyRequest SwitchFireKey;

        public float AimScopeFov = 20;
        public float CameraRecoliDampTime = 0.1f;
        public float Damage = 20;
        public string DrawAnimationName;
        public bool ScopeAllowed = false;
        public Vector3 MuzzleFirePos;
        public Sprite[] MuzzleFireSprites;

        private readonly List<Vector3> _Spreads = new List<Vector3>();
        private readonly StickyInputDic _StickyInputDic = new StickyInputDic();
        private bool _AimScopeBeforeReload;
        private bool _Reloading;
        private Animator _Animator;
        private RecoliController _Recoli;
        private AimScopeController _AimScope;

        private const string RELOAD = "Reload";

        public MachineGunFireControl MachineGunFireControl { private set; get; }
        public override float OverHeatFactor => MachineGunFireControl.OverHeatFactor;

        protected override void Awake()
        {
            base.Awake();

            _Animator = GetComponent<Animator>();
            player = Trans.parent.parent.parent.GetComponent<Player>();
            if(player==null)
                throw new Exception($"枪械 {name} 安装位置不正确");

            _StickyInputDic.AddBool(RELOAD, 0.7f);
            
            _AimScope = GetComponent<AimScopeController>();
            _AimScope.Init(player.FpsCamera.SensitiveScale);
            MachineGunFireControl = GetComponent<MachineGunFireControl>();
            _Recoli = GetComponent<RecoliController>();

            MachineGunFireControl.FireCheck = FireCheck;
        }

        private void Update()
        {
            var autoReload = Fire(ControlSys.RequireKey(FireKey));
            Reload(ControlSys.RequireKey(ReloadKey), autoReload);

            if (ControlSys.RequireKey(AimKey).Down)
                _AimScope.Switch();
            if (ControlSys.RequireKey(SwitchFireKey).Down)
                MachineGunFireControl.LoopBurstMode();

            _Animator.SetBool(RELOAD, _StickyInputDic.GetBool(RELOAD));
        }

        private void FixedUpdate()
        {
            _StickyInputDic.FixedUpdate();
        }

        public override void Reload()
        {
            AmmoGroup.Reload();
            AmmoGroup.InvokeOnAmmoChanged();

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
            _Recoli.Resets();
            MachineGunFireControl.Resets();
            _AimScope.Resets();
            _AimScopeBeforeReload = _Reloading = false;
        }

        public override ItemAddData Drop()
        {
            return new ItemAddData(AmmoGroup.Magazine);
        }

        public override void ReadAdditionalData(ItemAddData add)
        {
            add.TryGet(out AmmoGroup.Magazine);
        }

        public override void Fire()
        {
            var autoReload = Fire(ControlInfo.Open);
            Reload(ControlInfo.Open, autoReload);
        }
        
        private bool Fire(ControlInfo fireInfo)
        {
            var autoReload = false;
            var shootInfo = MachineGunFireControl.TryShoot(fireInfo);
            if (shootInfo.Shooted)
            {
                if (shootInfo.HitSomething)
                {
                    var hit = shootInfo.HitInfo;
                    _Spreads.Add(hit.point);
                    if (_Spreads.Count > 50)
                        _Spreads.RemoveAt(0);
                    BulletHoleManager.Create(hit.point, hit.normal);
                    hit.transform.GetComponent<IShootable>()?.GetShoot(hit.point, shootInfo.ShootDir, Damage);
                }

                _Recoli.Next();

                AmmoGroup.Magazine--;
                AmmoGroup.InvokeOnAmmoChanged();

                PublicEvents.GunFire?.Invoke();
            }
            else if (!shootInfo.FireCheckPass && fireInfo.Down)
            {
                autoReload = true;
            }

            return autoReload;
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

        private bool FireCheck()
        {
            return !AmmoGroup.MagazineEmpty();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            foreach (var pos in _Spreads) Gizmos.DrawSphere(pos, 0.1f);
        }
    }

    public class RecoliController : MonoBehaviour
    {
        public Vector2 RecoliBaseDir = Vector2.up;
        public float RecoliAngle = 30;
        public Range RecoliIntensity = (0.1f, 0.2f);
        public float RecoliForwardTime = 0.1f;
        public float RecoliBackTime = 0.7f;

        private FpsCamera _Camera;
        private float _RecoliAngleHalf;
        private Vector2 _RecoliTarget;
        private bool _Back;
        private SemiAutoCounter _RecoliCounter;

        public Vector2 CurFloatRecoli
        {
            get
            {
                var start = _Back ? _RecoliTarget : Vector2.zero;
                var end = _Back ? Vector2.zero : _RecoliTarget;
                return Vector2.Lerp(start, end, _RecoliCounter.Interpolant);
            }
        }

        private void Awake()
        {
            RecoliBaseDir = RecoliBaseDir.normalized;
            _RecoliAngleHalf = RecoliAngle / 2;
            _RecoliCounter = new SemiAutoCounter(RecoliForwardTime).OnComplete(SwitchDir);
            _Camera = GetPlayerCamera();
        }

        private void FixedUpdate()
        {
            _RecoliCounter.FixedUpdate();
            _Camera.ViewRotAddition(CurFloatRecoli);
        }

        /// <summary>
        /// 产生一次后坐力
        /// </summary>
        /// <returns>当前浮动后坐力位置</returns>
        public void Next()
        {
            _Camera.ViewRotTargetAddition(CurFloatRecoli);

            float angle = Tools.RandomFloat(-_RecoliAngleHalf, _RecoliAngleHalf);
            _RecoliTarget = Quaternion.AngleAxis(angle, Vector3.forward) *
                   RecoliBaseDir *
                   Tools.RandomFloat(RecoliIntensity.Min, RecoliIntensity.Max);
            _Back = false;
            _RecoliCounter.Recount(RecoliForwardTime);
        }

        public void Resets()
        {
            _Back = false;
            _RecoliCounter.Complete(false);
            _RecoliTarget = Vector2.zero;
        }

        private void SwitchDir()
        {
            if (!_Back)
            {
                _Back = true;
                _RecoliCounter.Recount(RecoliBackTime);
            }
            else
            {
                _Back = false;
            }
        }

        private static FpsCamera GetPlayerCamera()
        {
            return Player.Ins.FpsCamera;
        }
    }

    public class AimScopeController:MonoBehaviour
    {
        public float EnableFOV;
        public bool Allowed = true;

        private BuffData _PlayerSensitive;
        private Camera _MainCamera;
        private float _BaseFOV;
        private BuffData _AimScopeSensitiveScale;

        private const string AIM_POINT_UI_NAME = "aimPoint";

        private void Awake()
        {
            _BaseFOV = _MainCamera.fieldOfView;
            _AimScopeSensitiveScale = new BuffData(EnableFOV / _BaseFOV);
            _MainCamera = Camera.main;
        }

        private void OnDestroy()
        {
            _AimScopeSensitiveScale.Destroy();
        }

        public void Init(BuffData playerSensitive)
        {
            _PlayerSensitive = playerSensitive;
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