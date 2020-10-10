using System;
using UnityEngine;

namespace Thunder.Utility.PostProcessing
{
    [Serializable]
    public class Ssao : BasePostProcessing
    {
        public float Contrast = 1;
        public Texture2D Noise;
        public float SamplerFactor = 0.05f;

        [Range(0, 1)] public float SamplerRange;

        public override void Init()
        {
            base.Init();
            _Mat.SetTexture("_Noise", Noise);
        }

        public override void Process(RenderTexture source, RenderTexture dest)
        {
            _Mat.SetFloat("_SamplerRange", SamplerRange);
            _Mat.SetFloat("_SamplerFactor", SamplerFactor);
            _Mat.SetFloat("_Contrast", Contrast);
            _Mat.SetMatrix("_InverseProjection", Camera.main.projectionMatrix.inverse);
            Graphics.Blit(source, dest, _Mat);
        }
    }
}