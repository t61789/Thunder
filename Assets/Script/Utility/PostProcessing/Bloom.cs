using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Thunder.Utility.PostProcessing
{
    [Serializable]
    public class Bloom:GaussBlur
    {
        [Range(0, 4)]
        public float LuminanceThreshold = 0.6f;
        public Shader GaussBlurShader;

        private Material _GaussBlurMaterial;

        public override void Init()
        {
            base.Init();

            _GaussBlurMaterial = new Material(GaussBlurShader);
        }

        public override void Process(RenderTexture source, RenderTexture dest)
        {
            RenderTexture tempTexture = RenderTexture.GetTemporary(source.width, source.height, 0);
            _Mat.SetFloat("_LuminanceThreshold", LuminanceThreshold);
            Graphics.Blit(source, tempTexture, _Mat, 0);
            RenderTexture afterBlur = RenderTexture.GetTemporary(source.width, source.height, 0);
            Blur(Iterations, BlurSpread, DownSample, tempTexture, afterBlur, _GaussBlurMaterial);
            RenderTexture.ReleaseTemporary(tempTexture);
            _Mat.SetTexture("_Bloom", afterBlur);
            Graphics.Blit(source, dest, _Mat, 1);
            RenderTexture.ReleaseTemporary(afterBlur);
        }
    }
}
