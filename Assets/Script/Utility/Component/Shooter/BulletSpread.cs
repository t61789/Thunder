using System;
using Framework;
using UnityEngine;

namespace Thunder
{
    public class BulletSpread:MonoBehaviour
    {
        [Tooltip("噪声平滑度，越小越平滑")]
        [Range(0,1)]
        public float NoiseSmooth;
        [Tooltip("散射中心偏移的方向")]
        public Vector2 OffsetDir;
        [Tooltip("缩放参数")]
        public float ScaleFactor;

        public Spread SpreadX;
        public Spread SpreadY;
        public Spread SpreadO;

        private Spread[] _Spreads;
        private Matrix4x4 _OffsetMatrix;
        private float _CurAccuracy = 1;

        /// <summary>
        /// 三个放下上平均的过热系数
        /// </summary>
        public float OverHeat => GetAverageOverHeat();

        private void Awake()
        {
            _Spreads = new[] { SpreadX, SpreadY, SpreadO };
            foreach (var spread in _Spreads)
                spread.Init(NoiseSmooth);

            OffsetDir = OffsetDir.normalized;
            _OffsetMatrix = Tools.BuildTransferMatrix(
                Vector3.Cross(OffsetDir, Vector3.forward),
                OffsetDir,
                Vector3.forward);
        }

        private void FixedUpdate()
        {
            foreach (var spread in _Spreads)
            {
                spread.Update();
            }
        }

        /// <summary>
        /// 修改散射系数，spread = spreadBase * factor
        /// </summary>
        /// <param name="factor"></param>
        public void MulAccuracy(float factor)
        {
            float cFactor = factor / _CurAccuracy;
            foreach (var spread in _Spreads)
            {
                spread.SpreadValue.Min *= cFactor;
                spread.SpreadValue.Max *= cFactor;
                spread.OverHeatIntensity *= cFactor;
            }

            _CurAccuracy = factor;
        }

        /// <summary>
        /// 重置过热状态
        /// </summary>
        public void ResetSpread()
        {
            foreach (var spread in _Spreads)
                spread.SetOverHeatFactor(0, true);
            if (_CurAccuracy != 1)
                MulAccuracy(1);
        }

        /// <summary>
        /// 获取下一个散射位置
        /// </summary>
        /// <returns></returns>
        public Vector3 NextSpread()
        {
            var result = new Vector3
            {
                x = SpreadX.NextValue(),
                y = SpreadY.NextValue()
            };
            result.y += SpreadO.NextNaturalValue();

            return (_OffsetMatrix * result).ToV3Pos();
        }

        private float GetAverageOverHeat()
        {
            float result = 0;
            foreach (var spread in _Spreads)
            {
                result += spread.OverHeatFactor;
            }

            result /= _Spreads.Length;
            return result;
        }

        [Serializable]
        public class Spread
        {
            [Tooltip("散射的上下限")]
            public Range SpreadValue;
            [Tooltip("过热速度，越大过热越快")]
            public float OverHeatIntensity;
            [Tooltip("需要花多久从完全过热冷却到完全不过热")]
            public float ColdDownTime;
            [Tooltip("在一次触发后在进入冷却状态前的空档期")]
            public float ColdDownStayTime;

            private SimpleCounter _StayCounter;
            private PerlinNoise _SpreadNoise;

            public float OverHeatFactor { private set; get; }

            public void Update()
            {
                if (_StayCounter.Completed)
                {
                    OverHeatFactor = (OverHeatFactor - Time.fixedDeltaTime / ColdDownTime).Clamp01();
                }
            }

            public void Init(float smooth)
            {
                _StayCounter = new SimpleCounter(ColdDownStayTime);
                _SpreadNoise = new PerlinNoise(smooth);
            }

            public float NextFactor()
            {
                var result = Tools.Lerp(SpreadValue.Min, SpreadValue.Max, OverHeatFactor);
                OverHeatFactor = (OverHeatFactor + OverHeatIntensity).Clamp01();
                _StayCounter.Recount();
                return result * (_SpreadNoise.Next() - 0.5f) / 2;
            }

            public float NextValue()
            {
                return NextFactor() * (_SpreadNoise.Next() - 0.5f) / 2;
            }

            public float NextNaturalValue()
            {
                return NextFactor() * _SpreadNoise.Next();
            }

            public void SetOverHeatFactor(float factor, bool noStay)
            {
                if (noStay)
                    _StayCounter.Complete();
                OverHeatFactor = factor;
            }
        }
    }
}
