using System;
using UnityEngine;

namespace Thunder.Utility.PostProcessing
{
    [Serializable]
    public class LDepthOfField : GaussBlur
    {
        private Material _GaussBlurMat;
        public Shader GaussBlurShader;

        [Range(0, 1)] public float MiddleDepth = 0.5f;

        public override void Init()
        {
            base.Init();

            _GaussBlurMat = new Material(GaussBlurShader);
        }

        public override void Process(RenderTexture source, RenderTexture dest)
        {
            var gauss = RenderTexture.GetTemporary(source.width, source.height, 0);
            Blur(Iterations, BlurSpread, DownSample, source, gauss, _GaussBlurMat);
            _Mat.SetTexture("_Gauss", gauss);
            _Mat.SetFloat("_MiddleDepth", MiddleDepth);
            Graphics.Blit(source, dest, _Mat);
            RenderTexture.ReleaseTemporary(gauss);
        }
    }
}