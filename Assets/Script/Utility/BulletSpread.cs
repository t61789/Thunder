using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Thunder.Utility
{
    [Serializable]
    public class BulletSpread
    {
        public Vector2 SpreadX;
        public Vector2 SpreadY;
        public Vector2 SpreadCenter;
        public Vector2 XOverHeatTime;
        public Vector2 YOverHeatTime;
        public Vector2 CenterOverHeatTime;
        public float SpreadSmoothFactor;
        public float FarSpreadFactor;
        public float CameraRecoil;
        [Range(0,1)]
        public float CameraRecoilStay;
        public float CameraRecoilSmoothFactor;

        private float _SpreadX;
        private float _SpreadY;
        private float _SpreadCenter;
        private float _ColdDownCount;

        public Vector3 GetNextBulletDir(float fireInterval)
        {
            _ColdDownCount = Time.time;
            float perlinCoord = Time.time * SpreadSmoothFactor;
            Vector3 result = new Vector3();
            float perlin = Mathf.PerlinNoise(perlinCoord, 0) * 2 - 1;
            result.x = _SpreadX * perlin;

            perlin = Mathf.PerlinNoise(0, perlinCoord) * 2 - 1;
            result.y = _SpreadY * perlin;
            result.y += _SpreadCenter;

            _SpreadX = Mathf.Clamp(_SpreadX + GetSpeed(XOverHeatTime.x, SpreadX,fireInterval), SpreadX.x, SpreadX.y);
            _SpreadY = Mathf.Clamp(_SpreadY + GetSpeed(YOverHeatTime.x, SpreadY, fireInterval), SpreadY.x, SpreadY.y);
            _SpreadCenter = Mathf.Clamp(_SpreadCenter + GetSpeed(CenterOverHeatTime.x, SpreadCenter, fireInterval), SpreadCenter.x, SpreadCenter.y);

            return result - Vector3.back * FarSpreadFactor;
        }

        public Vector2 GetNextCameraShake(out Vector2 finalRot)
        {
            Vector2 result = new Vector2();
            float perlinCoord = Time.time * CameraRecoilSmoothFactor;
            float perlin = Mathf.PerlinNoise(perlinCoord, 0) * 2 - 1;
            result.y = CameraRecoil * perlin;
            perlin = Mathf.PerlinNoise(0, perlinCoord);
            result.x = -CameraRecoil * perlin;
            finalRot = result * CameraRecoilStay;

            return result;
        }

        /// <summary>
        /// FixedUpdate
        /// </summary>
        public void ColdDown(float fireInterval)
        {
            if (Time.time - _ColdDownCount <= fireInterval) return;
            _SpreadX = Mathf.Clamp(_SpreadX - GetSpeed(XOverHeatTime.y, SpreadX), SpreadX.x, SpreadX.y);
            _SpreadY = Mathf.Clamp(_SpreadY - GetSpeed(YOverHeatTime.y, SpreadY), SpreadY.x, SpreadY.y);
            _SpreadCenter = Mathf.Clamp(_SpreadCenter - GetSpeed(CenterOverHeatTime.y, SpreadCenter), SpreadCenter.x, SpreadCenter.y);
        }

        private float GetSpeed(float time,Vector2 limit, float fireInterval=0)
        {
            if (fireInterval == 0) return (limit.y - limit.x) * Time.fixedDeltaTime / time;
            return (limit.y - limit.x) / (time / fireInterval);
        }
    }
}
