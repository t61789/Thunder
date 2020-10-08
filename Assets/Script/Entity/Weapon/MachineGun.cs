using System.Collections.Generic;
using Thunder.Sys;
using Thunder.Tool;
using Thunder.Tool.BuffData;
using Thunder.UI;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace Thunder.Entity.Weapon
{
    public class MachineGun:BaseWeapon
    {
        public bool Safety = false;
        public float FireInterval = 0.2f;
        public float CameraRecoliDampTime = 0.1f;
        public Vector3 OverHeatFactor => _BulletSpread.OverHeatFactor;
        public float AimScopeFov = 20;
        /// <summary>
        /// 0为无限连发模式，其余正整数为连射次数
        /// </summary>
        public int BurstMode = 0;
        public bool ScopeAllowed = false;
        public float Damage = 20;
        public string DrawAnimationName;
        public Vector3 MuzzleFirePos;
        public Sprite[] MuzzleFireSprites;

        private RecoliController _Recoli;
        private BurstController _Burst;
        private AimScopeController _AimScope;
        private Animator _Animator;
        [SerializeField]
        private BulletSpread _BulletSpread;
        private readonly StickyInputDic _StickyInputDic = new StickyInputDic();
        private bool _AimScopeBeforeReload;
        private bool _Reloading;

        private const string SQUAT = "squat";
        private const string HANG = "hang";
        private const string RELOAD = "Reload";

        protected override void Awake()
        {
            base.Awake();

            _Animator = GetComponent<Animator>();
            _Trans = transform;
            _Player = _Trans.parent.parent.parent.GetComponent<Player>();
            Assert.IsNotNull(_Player,
                $"枪械 {name} 安装位置不正确");
            _Player.OnSquat.AddListener(PlayerSquat);

            _Player.OnHanging.AddListener(PlayerHanging);
            _StickyInputDic.AddBool(RELOAD, 0.7f);
            _Burst = new BurstController(BurstMode,FireInterval);
            _Recoli =new RecoliController(CameraRecoliDampTime);
            _AimScope = new AimScopeController(_Player.SensitiveScale,AimScopeFov);
        }

        private void Update()
        {
            if (!Safety) return;

            ControlInfo fire = ControlSys.Ins.RequireKey(GlobalSettings.FireKeyName, 0);
            bool autoReload;
            bool param = _Burst.FireCheck(fire,!AmmoGroup.MagzineEmpty(),out autoReload);
            if(param)Fire();

            param = AmmoGroup.ReloadConfirm() &&
                    !_Reloading &&
                    (ControlSys.Ins.RequireKey(RELOAD, 0).Down || autoReload);
            _StickyInputDic.SetBool(RELOAD, param);
            if (param && !_Reloading)
            {
                _AimScopeBeforeReload = _AimScope.Enable;
                if (_AimScope.Enable)
                    _AimScope.Switch();
                _Reloading = true;
            }
            _Animator.SetBool(RELOAD, _StickyInputDic.GetBool(RELOAD));

            if (ControlSys.Ins.RequireKey(GlobalSettings.AimScopeKeyName, 0).Down)
                _AimScope.Switch();
            if (ControlSys.Ins.RequireKey(GlobalSettings.SwitchFireModeKeyName, 0).Down)
                _Burst.LoopMode();
        }

        private readonly List<Vector3> _Spreads = new List<Vector3>();

        public override void Reload()
        {
            AmmoGroup.Reload();
            AmmoGroup.InvokeOnAmmoChanged();

            if (_AimScopeBeforeReload)
                _AimScope.Switch();

            _Reloading = false;
        }

        public void SetSafety(int value)
        {
            Safety = value != 0;
        }

        public override void FillAmmo()
        {
            AmmoGroup.FillUp(false);
            AmmoGroup.InvokeOnAmmoChanged();
        }

        public override void TakeOut()
        {
            _Animator.Play(DrawAnimationName);
        }

        public override void PutBack()
        {
            Safety = true;
            _Recoli.Reset();
            _Burst.Reset();
            _AimScope.Reset();
            _BulletSpread.Reset();
            _AimScopeBeforeReload = _Reloading = false;
        }

        public override void Drop()
        {
        }

        public override void Fire()
        {
            Vector3 dir = _BulletSpread.GetNextBulletDir(FireInterval);
            dir = Camera.main.transform.localToWorldMatrix * dir;

            RaycastHit hit;
            if (Physics.Raycast(_Trans.position, dir, out hit))
            {
                _Spreads.Add(hit.point);
                if (_Spreads.Count > 50)
                    _Spreads.RemoveAt(0);
                BulletHoleManager.Create(hit.point, hit.normal);
                hit.transform.GetComponent<IShootable>()?.GetShoot(hit.point, dir, Damage);
            }

            Vector2 stay;
            _Recoli.TriggerRecoli(_BulletSpread.GetNextCameraShake(out stay),stay);

            AmmoGroup.Magzine--;
            AmmoGroup.InvokeOnAmmoChanged();


            PublicEvents.GunFire?.Invoke();
        }

        private void FixedUpdate()
        {
            _StickyInputDic.FixedUpdate();

            _BulletSpread.ColdDown(FireInterval);

            // 设置后坐力
            Vector2 addition, stay;
            _Recoli.GetRecoli(out addition,out stay);
            _Player.ViewRotAddition(addition);
            if(stay!=Vector2.zero)_Player.ViewRotTargetAddition(stay);

            AimPoint.Instance.SetAimValue(OverHeatFactor.Average());
        }

        protected override void PlayerSquat(bool squating, bool hanging)
        {
            if (squating)
            {
                AimPoint.Instance.AimSizeScale.AddBuff(_BulletSpread.SquatSpreadScale, SQUAT, BuffData.Operator.Mul, 0);
                _BulletSpread.SpreadScale.AddBuff(_BulletSpread.SquatSpreadScale, SQUAT, BuffData.Operator.Mul, 0);
                _BulletSpread.SetSpreadScale();
            }
            else
            {
                AimPoint.Instance.AimSizeScale.RemoveBuff(SQUAT);
                _BulletSpread.SpreadScale.RemoveBuff(SQUAT);
                _BulletSpread.SetSpreadScale();
            }
        }

        protected override void PlayerHanging(bool squating, bool hanging)
        {
            if (hanging)
            {
                AimPoint.Instance.AimSizeScale.AddBuff(_BulletSpread.HangingSpreadScale, HANG, BuffData.Operator.Mul, 0);
                _BulletSpread.SpreadScale.AddBuff(_BulletSpread.HangingSpreadScale, HANG, BuffData.Operator.Mul, 0);
                _BulletSpread.SetSpreadScale();
            }
            else
            {
                AimPoint.Instance.AimSizeScale.RemoveBuff(HANG);
                _BulletSpread.SpreadScale.RemoveBuff(HANG);
                _BulletSpread.SetSpreadScale();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            foreach (var pos in _Spreads)
            {
                Gizmos.DrawSphere(pos, 0.1f);
            }
        }
    }

    public class BurstController
    {
        public int BurstMode;
        public float FireInterval
        {
            get => _FireCounter.TimeLimit;
            set => _FireCounter.Recount(value);
        }

        private int _BurstCount;
        private readonly SimpleCounter _FireCounter;
        private readonly int[] _FireModeLoop = { 0, 1, 3 };

        public BurstController(int burstMode,float fireInterval)
        {
            BurstMode = burstMode;
            _FireCounter = (SimpleCounter) new SimpleCounter(fireInterval).Complete();
        }

        public void Reset()
        {
            _BurstCount = 0;
            _FireCounter.Complete();
            BurstMode = _FireModeLoop[0];
        }

        public void LoopMode()
        {
            int index = _FireModeLoop.FindIndex(x => BurstMode == x);
            index++;
            index %= _FireModeLoop.Length;
            BurstMode = _FireModeLoop[index];
            PublicEvents.GunFireModeChange?.Invoke(BurstMode);
        }

        public bool FireCheck(ControlInfo input,bool hasAmmo,out bool autoReload)
        {
            autoReload = false;
            if (!_FireCounter.Completed) return false;
            bool result = false;
            bool param = true;
            if (!hasAmmo)
            {
                if (input.Down)
                    autoReload = true;
                param = false;
            }
            else if (BurstMode == 0 && input.Stay)
                result = true;
            else if (input.Down || _BurstCount > 0)
            {
                if (_BurstCount == 0)
                    _BurstCount = BurstMode;
                _BurstCount--;
                result = true;
            }
            else
                param = false;

            if (param)
                _FireCounter.Recount();

            return result;
        }
    }

    public class RecoliController
    {
        private readonly SimpleCounter _RecoliCounter;
        private Vector2 _CameraStart;
        private Vector2 _CameraEnd;
        private Vector2 _SaveStay;

        public RecoliController(float recoliTime)
        {
            _RecoliCounter = new SimpleCounter(recoliTime);
        }

        public void Reset()
        {
            _RecoliCounter.Complete();
            _CameraEnd = _CameraStart = _SaveStay = Vector2.zero;
        }

        public void GetRecoli(out Vector2 addition,out Vector2 stay)
        {
            float x = _RecoliCounter.Interpolant;
            addition = Vector2.zero;
            if (x >= 1 && _CameraEnd != Vector2.zero)
            {
                _RecoliCounter.Recount();
                _CameraStart = _CameraEnd;
                _CameraEnd = Vector2.zero;
            }
            else
                addition = Vector2.Lerp(_CameraStart, _CameraEnd,
                    Mathf.Sin(x * Mathf.PI / 2));

            stay = _SaveStay;
            _SaveStay = Vector2.zero;
        }

        public void TriggerRecoli(Vector2 addition,Vector2 stay)
        {
            _SaveStay += stay;
            _CameraStart = Vector2.Lerp(_CameraStart, _CameraEnd,
                Mathf.Sin(_RecoliCounter.Interpolant * Mathf.PI / 2));
            _CameraEnd = addition;
            _RecoliCounter.Recount();
        }
    }

    public class AimScopeController
    {
        public bool Allowed=true;
        public bool Enable { private set; get; }

        private readonly float _EnableFOV;
        private readonly float _BaseFOV;
        private readonly BuffData _AimScopeSensitiveScale;
        private readonly BuffData _PlayerSensitive;
        private readonly Camera _MainCamera;

        private const string AIM_POINT_UI_NAME = "aimPoint";
        private const string BUFF_NAME = "aimScope";

        public AimScopeController(BuffData playerSensitive, float fov)
        {
            _PlayerSensitive = playerSensitive;
            _MainCamera = Camera.main;
            _BaseFOV = _MainCamera.fieldOfView;
            _EnableFOV = fov;
            _AimScopeSensitiveScale = _EnableFOV/_BaseFOV;
        }

        public void Reset()
        {
            if(Enable)
                Switch();
        }

        public void Switch()
        {
            if (!Allowed) return;

            if (!Enable)
            {
                _MainCamera.fieldOfView = _EnableFOV;
                PostProcessingController.Instance.AimScope.Enable = true;
                UISys.Ins.CloseUI(AIM_POINT_UI_NAME);
                _PlayerSensitive.AddBuff(_AimScopeSensitiveScale, BUFF_NAME, BuffData.Operator.Mul, 0);
                PostProcessingController.Instance.DepthOfField.Enable = true;
            }
            else
            {
                _MainCamera.fieldOfView = _BaseFOV;
                PostProcessingController.Instance.AimScope.Enable = false;
                UISys.Ins.OpenUI(AIM_POINT_UI_NAME);
                _PlayerSensitive.RemoveBuff(BUFF_NAME);
                PostProcessingController.Instance.DepthOfField.Enable = false;
            }

            Enable = !Enable;
        }
    }
}
