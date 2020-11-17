using System;
using UnityEngine;

namespace Thunder
{
    [Serializable]
    public class Bsc : BasePostProcessing
    {
        [Range(0, 3f)] public float Brightness = 1f;

        [Range(0, 3f)] public float Contrast = 1f;

        [Range(0, 3f)] public float Saturation = 1f;

        public override void Process(RenderTexture source, RenderTexture dest)
        {
            _Mat.SetFloat("_Brightness", Brightness);
            _Mat.SetFloat("_Saturation", Saturation);
            _Mat.SetFloat("_Contrast", Contrast);
            Graphics.Blit(source, dest, _Mat);
        }
    }
}