using System;
using Thunder.Tool;
using UnityEngine;

namespace Thunder.Utility.PostProcessing
{
    [Serializable]
    public class DepthOfField : BasePostProcessing
    {
        public enum Quality
        {
            High,
            Middle
        }

        private Material _HighQualityMat;
        private Material _MiddleQualityMat;
        public int BlurSeg = 4;
        public float DepthScale = 0.5f;
        public float DistanceThreshold = 0.5f;

        [Range(0, 1)] public float FocalDepth = 0.5f;

        public Shader HighQualityShader;

        [Range(1, 10)] public int Iterations = 1;

        public float MaxBlurSize = 0.5f;
        public Shader MiddleQualityShader;
        public Quality ProcessQuality;

        public override void Init()
        {
            base.Init();
            _HighQualityMat = new Material(HighQualityShader);
            _MiddleQualityMat = new Material(MiddleQualityShader);
        }

        public override void Process(RenderTexture source, RenderTexture dest)
        {
            switch (ProcessQuality)
            {
                case Quality.Middle:
                    MiddleQuality(source, dest);
                    break;
                case Quality.High:
                    HighQuality(source, dest);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void MiddleQuality(RenderTexture source, RenderTexture dest)
        {
            var gaussTemp0 = RenderTexture.GetTemporary(source.width, source.height, 0);
            gaussTemp0.filterMode = FilterMode.Bilinear;
            Graphics.Blit(source, gaussTemp0);

            _MiddleQualityMat.SetFloat("_MaxBlurSize", MaxBlurSize);
            _MiddleQualityMat.SetVector("_FocalDepthPos", new Vector4(0.5f, 0.5f, 0, 0));
            _MiddleQualityMat.SetFloat("_DistanceThreshold", DistanceThreshold);

            for (var i = 0; i < Iterations; i++)
            {
                var gaussTemp1 = RenderTexture.GetTemporary(gaussTemp0.width, gaussTemp0.height, 0);
                _MiddleQualityMat.SetInt("_BlurVertical", 1);
                Graphics.Blit(gaussTemp0, gaussTemp1, _MiddleQualityMat);
                RenderTexture.ReleaseTemporary(gaussTemp0);

                gaussTemp0 = RenderTexture.GetTemporary(gaussTemp1.width, gaussTemp1.height, 0);
                _MiddleQualityMat.SetInt("_BlurVertical", 0);
                Graphics.Blit(gaussTemp1, gaussTemp0, _MiddleQualityMat);
                RenderTexture.ReleaseTemporary(gaussTemp1);
            }

            Graphics.Blit(gaussTemp0, dest);
            RenderTexture.ReleaseTemporary(gaussTemp0);
        }

        private void HighQuality(RenderTexture source, RenderTexture dest)
        {
            _MiddleQualityMat.SetFloat("_MaxBlurSize", MaxBlurSize);
            _MiddleQualityMat.SetFloat("_FocalDepth", FocalDepth);

            var result0 = source.GetTemporary();
            Graphics.Blit(source, result0, _HighQualityMat, 2);

            var segWidth = 1f / BlurSeg;
            for (var segCount = BlurSeg - 1; segCount >= 0; segCount--)
            {
                _HighQualityMat.SetFloat("_SegMin", DepthScales(segCount * segWidth, DepthScale));
                _HighQualityMat.SetFloat("_SegMax", DepthScales((segCount + 1) * segWidth, DepthScale));

                var gaussTemp0 = source.GetTemporary();
                for (var i = 0; i < Iterations; i++)
                {
                    var gaussTemp1 = source.GetTemporary();
                    _HighQualityMat.SetInt("_BlurVertical", 1);
                    Graphics.Blit(i == 0 ? source : gaussTemp0, gaussTemp1, _HighQualityMat, 0);
                    gaussTemp0.ReleaseTemporary();

                    SwitchBuffer(ref gaussTemp0, ref gaussTemp1);

                    gaussTemp1 = source.GetTemporary();
                    _HighQualityMat.SetInt("_BlurVertical", 0);
                    Graphics.Blit(gaussTemp0, gaussTemp1, _HighQualityMat, 0);
                    gaussTemp0.ReleaseTemporary();

                    SwitchBuffer(ref gaussTemp0, ref gaussTemp1);
                }

                var result1 = source.GetTemporary();

                _HighQualityMat.SetTexture("_IterationTex", gaussTemp0);
                Graphics.Blit(result0, result1, _HighQualityMat, 1);
                result0.ReleaseTemporary();
                SwitchBuffer(ref result0, ref result1);
                gaussTemp0.ReleaseTemporary();
            }

            Graphics.Blit(result0, dest);
            result0.ReleaseTemporary();

            // todo 正确的效果，可接受的深度值
        }

        private float DepthScales(float f, float scale)
        {
            return f + (1 - f) * scale;
        }

        private void SwitchBuffer(ref RenderTexture r0, ref RenderTexture r1)
        {
            var temp = r0;
            r0 = r1;
            r1 = temp;
        }
    }
}