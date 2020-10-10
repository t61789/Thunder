﻿using System;
using Thunder.Tool.BuffData;
using UnityEngine;

namespace Thunder.Utility
{
    [Serializable]
    public class BulletSpread
    {
        [SerializeField] private float _CameraRecoil;

        [SerializeField] [Range(0, 1)] private float _CameraRecoilStay;

        private float _ColdDownCount;

        [SerializeField] private Vector2 _SpreadC;

        private float _SpreadCValue;

        [SerializeField] private Vector2 _SpreadX;

        private float _SpreadXValue;

        [SerializeField] private Vector2 _SpreadY;

        private float _SpreadYValue;

        public float CameraRecoilSmoothFactor;
        public Vector2 COverHeatTime;
        public float FarSpreadFactor;
        public BuffData HangingSpreadScale;

        [HideInInspector] public Vector3 OverHeatFactor; // X Y Center

        [HideInInspector] public BuffData SpreadScale = 1;

        public float SpreadSmoothFactor;
        public BuffData SquatSpreadScale;

        public Vector2 XOverHeatTime;
        public Vector2 YOverHeatTime;

        public Vector2 SpreadX
        {
            get => _SpreadX * SpreadScale.CurData;
            set => _SpreadX = value;
        }

        public Vector2 SpreadY
        {
            get => _SpreadY * SpreadScale.CurData;
            set => _SpreadY = value;
        }

        public Vector2 SpreadC
        {
            get => _SpreadC * SpreadScale.CurData;
            set => _SpreadC = value;
        }

        public float CameraRecoil
        {
            get => _CameraRecoil * SpreadScale.CurData;
            set => _CameraRecoil = value;
        }

        public float CameraRecoilStay
        {
            get => _CameraRecoilStay * SpreadScale.CurData;
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

        private float GetSpeed(float time, Vector2 limit, float fireInterval = 0)
        {
            if (fireInterval == 0) return (limit.y - limit.x) * Time.fixedDeltaTime / time;
            return (limit.y - limit.x) / (time / fireInterval);
        }
    }
}