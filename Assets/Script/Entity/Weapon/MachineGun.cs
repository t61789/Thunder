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
        public Vector3 MuzzleFirePos;
        public Sprite[] MuzzleFireSprites;

        private SimpleCounter _FireCounter;
        private Animator _Animator;
        private Vector2 _CurCameraRecoilAddition;
        private float _CameraRecoliDampTimeCount;
        private Vector2 _CameraStart;
        private Vector2 _CameraEnd;
        [SerializeField]
        private BulletSpread _BulletSpread;
        private bool _AimScopeEnable;
        private float _BaseAimScopeFov;
        private Camera _GunCamera;
        private float _BurstCount;
        private BuffData _AimScopeSensitiveScale;
        private bool _AutoReload;
        private readonly StickyInputDic _StickyInputDic = new StickyInputDic();
        private bool _AimScopeBeforeReload;
        private bool _Reloading;
        private readonly int[] _FireModeLoop = { 0, 1, 3 };

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
            _GunCamera = Camera.main;
            _BaseAimScopeFov = _GunCamera.fieldOfView;
            _AimScopeSensitiveScale = AimScopeFov / _BaseAimScopeFov;
            _StickyInputDic.AddBool(RELOAD, 0.7f);
            _FireCounter = new SimpleCounter(FireInterval);
            _FireCounter.Complete();
        }

        private void Update()
        {
            if (!Safety) return;
            ControlInfo fire = ControlSys.Ins.RequireKey("Fire1", 0);
            bool param = true;

            if (_FireCounter.Completed)
            {
                if (AmmoGroup.Magzine == 0)
                {
                    if (fire.Down)
                        _AutoReload = true;
                    param = false;
                }
                else if (BurstMode == 0 && fire.Stay)
                    Fire();
                else if (fire.Down || _BurstCount > 0)
                {
                    if (_BurstCount == 0)
                        _BurstCount = BurstMode;
                    _BurstCount--;
                    Fire();
                }
                else
                    param = false;

                if (param)
                    _FireCounter.Recount();
            }
            _Animator.SetBool("Fire", param);

            param = AmmoGroup.ReloadConfirm() &&
                    !_Reloading &&
                    (ControlSys.Ins.RequireKey(RELOAD, 0).Down || _AutoReload);

            _StickyInputDic.SetBool(RELOAD, param);
            if (param && !_Reloading)
            {
                _AimScopeBeforeReload = _AimScopeEnable;
                if (_AimScopeEnable)
                    SwitchAimScope();
                _Reloading = true;
            }
            _Animator.SetBool(RELOAD, _StickyInputDic.GetBool(RELOAD));

            if (ControlSys.Ins.RequireKey("AimScope", 0).Down)
                SwitchAimScope();
            if (ControlSys.Ins.RequireKey("SwitchFireMode", 0).Down)
                LoopFireMode();
        }

        private readonly List<Vector3> _Spreads = new List<Vector3>();

        public override void Reload()
        {
            AmmoGroup.Reload();
            AmmoGroup.InvokeOnAmmoChanged();

            _AutoReload = false;

            if (_AimScopeBeforeReload)
                SwitchAimScope();

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

        private int LoopFireMode()
        {
            int index = _FireModeLoop.FindIndex(x => BurstMode == x);
            index++;
            index %= _FireModeLoop.Length;
            BurstMode = _FireModeLoop[index];
            PublicEvents.GunFireModeChange?.Invoke(BurstMode);
            return BurstMode;
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
            _CameraEnd = _BulletSpread.GetNextCameraShake(out stay);
            _CameraStart = _CurCameraRecoilAddition;
            _Player.ViewRotTargetAddition(stay);
            _CameraRecoliDampTimeCount = Time.time;

            AmmoGroup.Magzine--;
            AmmoGroup.InvokeOnAmmoChanged();


            PublicEvents.GunFire?.Invoke();
        }

        private void FixedUpdate()
        {
            _StickyInputDic.FixedUpdate();
            _BulletSpread.ColdDown(FireInterval);
            float x = Mathf.Clamp01((Time.time - _CameraRecoliDampTimeCount) / CameraRecoliDampTime);
            if (x >= 1 && _CameraEnd != Vector2.zero)
            {
                _CameraRecoliDampTimeCount = Time.time;
                _CameraStart = _CameraEnd;
                _CameraEnd = Vector2.zero;
            }
            else
            {
                _CurCameraRecoilAddition = Vector2.Lerp(_CameraStart, _CameraEnd,
                    Mathf.Sin(x * Mathf.PI / 2));

                _Player.ViewRotAddition(_CurCameraRecoilAddition);
            }

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

        private void SwitchAimScope()
        {
            if (!ScopeAllowed) return;

            if (!_AimScopeEnable)
            {
                _GunCamera.fieldOfView = AimScopeFov;
                PostProcessingController.Instance.AimScope.Enable = true;
                UISys.Ins.CloseUI("aimPoint");
                _Player.SensitiveScale.AddBuff(_AimScopeSensitiveScale, "AimScope", BuffData.Operator.Mul, 0);
                PostProcessingController.Instance.DepthOfField.Enable = true;
            }
            else
            {
                _GunCamera.fieldOfView = _BaseAimScopeFov;
                PostProcessingController.Instance.AimScope.Enable = false;
                UISys.Ins.OpenUI("aimPoint");
                _Player.SensitiveScale.RemoveBuff("AimScope");
                PostProcessingController.Instance.DepthOfField.Enable = false;
            }

            _AimScopeEnable = !_AimScopeEnable;
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
}
