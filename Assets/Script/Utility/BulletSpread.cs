using System;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityAudioSource;
using Thunder.Tool.BuffData;
using Thunder.Utility.PostProcessing;
using UnityEngine;
using UnityEngine.Events;

namespace Thunder.Utility
{
    [Serializable]
    public class BulletSpread:IHostDestroyed
    {
        public float SquattingSpreadScale;
        public float DanglingSpreadScale;
        public float SpreadSmoothFactor;
        public Vector2 XOverHeatTime;
        public Vector2 YOverHeatTime;
        public Vector2 COverHeatTime;
        public float CameraRecoilSmoothFactor;
        public float FarSpreadFactor;
        [SerializeField] [Range(0, 1)] private float _CameraRecoilStay;
        [SerializeField] private float _CameraRecoil;
        [SerializeField] private Vector2 _SpreadC;
        [SerializeField] private Vector2 _SpreadX;
        [SerializeField] private Vector2 _SpreadY;

        private BuffData _DanglingSpreadScale;
        private BuffData _SquattingSpreadScale;
        private float _ColdDownCount;
        private float _SpreadXValue;
        private float _SpreadYValue;
        private float _SpreadCValue;
        [HideInInspector] public Vector3 OverHeatFactor; // X Y Center
        [HideInInspector] public BuffData SpreadScale = new BuffData(1);

        public void Init()
        {
            _DanglingSpreadScale = new BuffData(DanglingSpreadScale);
            _SquattingSpreadScale = new BuffData(SquattingSpreadScale);

            PublicEvents.PlayerSquat.AddListener(Squat);
            PublicEvents.PlayerDangling.AddListener(Dangling);
        }

        public Vector2 SpreadX
        {
            get => _SpreadX * SpreadScale;
            set => _SpreadX = value;
        }

        public Vector2 SpreadY
        {
            get => _SpreadY * SpreadScale;
            set => _SpreadY = value;
        }

        public Vector2 SpreadC
        {
            get => _SpreadC * SpreadScale;
            set => _SpreadC = value;
        }

        public float CameraRecoil
        {
            get => _CameraRecoil * SpreadScale;
            set => _CameraRecoil = value;
        }

        public float CameraRecoilStay
        {
            get => _CameraRecoilStay * SpreadScale;
            set => _CameraRecoilStay = value;
        }

        public Vector3 GetNextBulletDir(float fireInterval)
        {
            _ColdDownCount = Time.time;
            var perlinCoord = Time.time * SpreadSmoothFactor;
            var result = new Vector3();
            var perlin = Mathf.PerlinNoise(perlinCoord, 0) * 2 - 1;
            result.x = _SpreadXValue * perlin;

            perlin = Mathf.PerlinNoise(0, perlinCoord) * 2 - 1;
            result.y = _SpreadYValue * perlin;
            result.y += _SpreadCValue;

            _SpreadXValue = Mathf.Clamp(_SpreadXValue + GetSpeed(XOverHeatTime.x, SpreadX, fireInterval), SpreadX.x,
                SpreadX.y);
            OverHeatFactor.x = Mathf.InverseLerp(SpreadX.x, SpreadX.y, _SpreadXValue);
            _SpreadYValue = Mathf.Clamp(_SpreadYValue + GetSpeed(YOverHeatTime.x, SpreadY, fireInterval), SpreadY.x,
                SpreadY.y);
            OverHeatFactor.y = Mathf.InverseLerp(SpreadY.x, SpreadY.y, _SpreadYValue);
            _SpreadCValue = Mathf.Clamp(_SpreadCValue + GetSpeed(COverHeatTime.x, SpreadC, fireInterval), SpreadC.x,
                SpreadC.y);
            OverHeatFactor.z = Mathf.InverseLerp(SpreadC.x, SpreadC.y, _SpreadCValue);

            return result - Vector3.back * FarSpreadFactor;
        }

        public Vector2 GetNextCameraShake(out Vector2 finalRot)
        {
            var result = new Vector2();
            var perlinCoord = Time.time * CameraRecoilSmoothFactor;
            var perlin = Mathf.PerlinNoise(perlinCoord, 0);
            result.y = CameraRecoil * perlin;
            perlin = Mathf.PerlinNoise(0, perlinCoord) * 2 - 1;
            result.x = CameraRecoil * perlin;
            finalRot = result * CameraRecoilStay;

            return result;
        }

        /// <summary>
        ///     FixedUpdate
        /// </summary>
        public void ColdDown(float fireInterval)
        {
            if (Time.time - _ColdDownCount <= fireInterval) return;
            _SpreadXValue = Mathf.Clamp(_SpreadXValue - GetSpeed(XOverHeatTime.y, SpreadX), SpreadX.x, SpreadX.y);
            OverHeatFactor.x = Mathf.InverseLerp(SpreadX.x, SpreadX.y, _SpreadXValue);
            _SpreadYValue = Mathf.Clamp(_SpreadYValue - GetSpeed(YOverHeatTime.y, SpreadY), SpreadY.x, SpreadY.y);
            OverHeatFactor.y = Mathf.InverseLerp(SpreadY.x, SpreadY.y, _SpreadYValue);
            _SpreadCValue = Mathf.Clamp(_SpreadCValue - GetSpeed(COverHeatTime.y, SpreadC), SpreadC.x, SpreadC.y);
            OverHeatFactor.z = Mathf.InverseLerp(SpreadC.x, SpreadC.y, _SpreadCValue);
        }

        public void SetSpreadScale()
        {
            _SpreadXValue = Mathf.Lerp(SpreadX.x, SpreadX.y, OverHeatFactor.x);
            _SpreadYValue = Mathf.Lerp(SpreadY.x, SpreadY.y, OverHeatFactor.y);
            _SpreadCValue = Mathf.Lerp(SpreadC.x, SpreadC.y, OverHeatFactor.z);
        }

        public void Reset()
        {
            OverHeatFactor = Vector3.zero;
            SetSpreadScale();
        }

        public void HostDestroyed(object host)
        {
            SpreadScale.Destroy();
            _DanglingSpreadScale.Destroy();
            _SquattingSpreadScale.Destroy();
        }

        public void Squat(bool down)
        {
            if (down)
                SpreadScale.AddBuff(_SquattingSpreadScale, Operator.Mul);
            else
                SpreadScale.RemoveBuff(_SquattingSpreadScale);
            SetSpreadScale();
        }

        public void Dangling(bool enter)
        {
            if (enter)
                SpreadScale.AddBuff(_DanglingSpreadScale, Operator.Mul);
            else
                SpreadScale.RemoveBuff(_DanglingSpreadScale);
            SetSpreadScale();
        }

        private float GetSpeed(float time, Vector2 limit, float fireInterval = 0)
        {
            if (fireInterval == 0) return (limit.y - limit.x) * Time.fixedDeltaTime / time;
            return (limit.y - limit.x) / (time / fireInterval);
        }
    }
}