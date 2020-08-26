using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thunder.Sys;
using Thunder.Tool;
using Thunder.Tool.BuffData;
using Thunder.UI;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace Thunder.Entity
{
    public class Gun:BaseEntity
    {
        public float FireInterval = 0.2f;
        public float CameraRecoliDampTime = 0.1f;
        public Vector3 OverHeatFactor => _BulletSpread.OverHeatFactor;
        public float AimScopeFov = 20;

        private Animator _Animator;
        private float _FireIntervalCount;
        private Player _Player;
        private Vector2 _CurCameraRecoilAddition;
        private float _CameraRecoliDampTimeCount;
        private Vector2 _CameraStart;
        private Vector2 _CameraEnd;
        [SerializeField]
        private BulletSpread _BulletSpread;
        private bool _AimScopeEnable;
        private float _BaseAimScopeFov;
        private Camera _GunCamera;

        private const string Squat = "squat";
        private const string Hang = "hang";

        protected override void Awake()
        {
            base.Awake();

            _Animator = GetComponent<Animator>();
            _Trans = transform;
            Assert.IsNotNull(_Player = _Trans.parent.parent.parent.GetComponent<Player>(),
                $"枪械 {name} 安装位置不正确");
            _Player.OnSquat.AddListener(PlayerSquat);
            _Player.OnHanging.AddListener(PlayerHanging);
            _GunCamera = Camera.main;
            _BaseAimScopeFov = _GunCamera.fieldOfView;
        }

        private void Update()
        {
            bool param = Time.time - _FireIntervalCount >= FireInterval && Stable.Control.RequireKey("Fire1", 0).Stay;
            if (param)
            {
                _FireIntervalCount = Time.time;
                Fire();
            }
            _Animator.SetBool("Fire", param);

            param = Stable.Control.RequireKey("Reload", 0).Down;
            _Animator.SetBool("Reload", param);
        }

        private readonly List<Vector3> _Spreads = new List<Vector3>();

        private void Fire()
        {
            Vector3 dir = _BulletSpread.GetNextBulletDir(FireInterval);
            dir = Camera.main.transform.localToWorldMatrix * dir;
            Debug.DrawLine(Camera.main.transform.position, Camera.main.transform.position + dir, Color.red);

            RaycastHit[] hits = Physics.RaycastAll(_Trans.position, dir);
            if (hits != null && hits.Length > 0)
            {
                _Spreads.Add(hits[hits.Length-1].point);
                if (_Spreads.Count > 50)
                    _Spreads.RemoveAt(0);
            }

            Vector2 stay;
            _CameraEnd = _BulletSpread.GetNextCameraShake(out stay);
            _CameraStart = _CurCameraRecoilAddition;
            _Player.ViewRotTargetAddition(stay);
            _CameraRecoliDampTimeCount = Time.time;
        }

        private void FixedUpdate()
        {
            _BulletSpread.ColdDown(FireInterval);
            float x = Mathf.Clamp01((Time.time - _CameraRecoliDampTimeCount)/ CameraRecoliDampTime);
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

        private void PlayerSquat(bool squating,bool hanging)
        {
            if (squating)
            {
                AimPoint.Instance.AimSizeScale.AddBuff(_BulletSpread.SquatSpreadScale, Squat, BuffData.Operator.Mul, 0);
                _BulletSpread.SpreadScale.AddBuff(_BulletSpread.SquatSpreadScale, Squat, BuffData.Operator.Mul, 0);
                _BulletSpread.SetSpreadScale();
            }
            else
            {
                AimPoint.Instance.AimSizeScale.RemoveBuff(Squat);
                _BulletSpread.SpreadScale.RemoveBuff(Squat);
                _BulletSpread.SetSpreadScale();
            }
        }

        private void PlayerHanging(bool squating, bool hanging)
        {
            if (hanging)
            {
                AimPoint.Instance.AimSizeScale.AddBuff(_BulletSpread.HangingSpreadScale, Hang, BuffData.Operator.Mul, 0);
                _BulletSpread.SpreadScale.AddBuff(_BulletSpread.HangingSpreadScale, Hang, BuffData.Operator.Mul, 0);
                _BulletSpread.SetSpreadScale();
            }
            else
            {
                AimPoint.Instance.AimSizeScale.RemoveBuff(Hang);
                _BulletSpread.SpreadScale.RemoveBuff(Hang);
                _BulletSpread.SetSpreadScale();
            }
        }

        private void SwitchAimScope()
        {
            if (!_AimScopeEnable)
            {
                _GunCamera.fieldOfView = AimScopeFov;
            }
            else
            {
                _GunCamera.fieldOfView = _BaseAimScopeFov;
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
