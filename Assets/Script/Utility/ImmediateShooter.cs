using System;
using Framework;
using UnityEngine;

namespace Thunder
{
    public class ImmediateShooter:MonoBehaviour
    {
        public float NoiseSmooth;
        public Vector2 OffsetDir;
        public float ScaleFactor;

        public Spread SpreadX;
        public Spread SpreadY;
        public Spread SpreadO;

        private Spread[] _Spreads;
        private Matrix4x4 _OffsetMatrix;

        private void Awake()
        {
            _Spreads = new[] {SpreadX, SpreadY, SpreadO};
            foreach (var spread in _Spreads)
                spread.Init(NoiseSmooth);

            OffsetDir = OffsetDir.normalized;
            _OffsetMatrix = Tools.BuildTransferMatrix(
                Vector3.Cross(OffsetDir,Vector3.forward),
                OffsetDir,
                Vector3.forward);
        }

        private void FixedUpdate()
        {
            foreach (var spread in _Spreads)
            {
                spread.FixedUpdate();
            }
        }

        public bool Shoot(Vector3 basePos, Vector3 baseDir, out RaycastHit hitInfo)
        {
            var result = Next();
            result.z = ScaleFactor;
            var z = baseDir.normalized;
            var x = Vector3.Cross(Vector3.up, baseDir.normalized);
            var matrix = Tools.BuildTransferMatrix(x, Vector3.Cross(z, x));
            result = matrix * result;

            return Physics.Raycast(basePos, result, out hitInfo);
        }

        private Vector3 Next()
        {
            var result = new Vector3
            {
                x = SpreadX.NextValue(), 
                y = SpreadY.NextValue()
            };
            result.y += SpreadO.NextNaturalValue();

            return (_OffsetMatrix * result).ToV3Pos();
        }

        [Serializable]
        public class Spread
        {
            public float SpreadValueMin;
            public float SpreadValueMax;
            public float OverHeatIntensity;
            public float ColdDownTime;
            public float ColdDownStayTime;

            private float _OverHeatFactor;
            private SimpleCounter _StayCounter;
            private PerlinNoise _SpreadNoise;

            public void FixedUpdate()
            {
                if (_StayCounter.Completed)
                {
                    _OverHeatFactor = (_OverHeatFactor - Time.fixedDeltaTime/ColdDownTime).Clamp01();
                }
            }

            public void Init(float smooth)
            {
                _StayCounter = new SimpleCounter(ColdDownStayTime);
                _SpreadNoise = new PerlinNoise(smooth);
            }

            public float NextFactor()
            {
                var result = Tools.Lerp(SpreadValueMin, SpreadValueMax, _OverHeatFactor);
                _OverHeatFactor = (_OverHeatFactor + OverHeatIntensity).Clamp01();
                _StayCounter.Recount();
                return result*(_SpreadNoise.Next()-0.5f)/2;
            }

            public float NextValue()
            {
                return NextFactor() * (_SpreadNoise.Next() - 0.5f) / 2;
            }

            public float NextNaturalValue()
            {
                return NextFactor() * _SpreadNoise.Next();
            }
        }
    }
}
