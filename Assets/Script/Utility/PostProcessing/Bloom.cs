using System;
using UnityEngine;

namespace Thunder
{
    [Serializable]
    public class Bloom : GaussBlur
    {
        private Material _GaussBlurMaterial;
        public Shader GaussBlurShader;

        [Range(0, 4)] public float LuminanceThreshold = 0.6f;

        public override void Init()
        {
            base.Init();

            _GaussBlurMaterial = new Material(GaussBlurShader);
        }

        public override void Process(RenderTexture source, RenderTexture dest)
        {
            var tempTexture = RenderTexture.GetTemporary(source.width, source.height, 0);
            _Mat.SetFloat("_LuminanceThreshold", LuminanceThreshold);
            Graphics.Blit(source, tempTexture, _Mat, 0);
            var afterBlur = RenderTexture.GetTemporary(source.width, source.height, 0);
            Blur(Iterations, BlurSpread, DownSample, tempTexture, afterBlur, _GaussBlurMaterial);
            RenderTexture.ReleaseTemporary(tempTexture);
            _Mat.SetTexture("_Bloom", afterBlur);
            Graphics.Blit(source, dest, _Mat, 1);
            RenderTexture.ReleaseTemporary(afterBlur);
        }
    }
}