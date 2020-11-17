using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Thunder
{
    [Serializable]
    public class SMotionBlur : BasePostProcessing
    {
        private RenderTexture _AccumulationTexture;

        [Range(0, 0.9f)] public float BlurAmount = 0.5f;

        public override void Process(RenderTexture source, RenderTexture dest)
        {
            if (_AccumulationTexture == null || _AccumulationTexture.width != source.width ||
                _AccumulationTexture.height != source.height)
            {
                Object.DestroyImmediate(_AccumulationTexture);
                _AccumulationTexture = new RenderTexture(source.width, source.height, 0)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                Graphics.Blit(source, _AccumulationTexture);
            }

            _AccumulationTexture.MarkRestoreExpected();
            _Mat.SetFloat("_BlurAmount", 1 - BlurAmount);
            Graphics.Blit(source, _AccumulationTexture, _Mat);
            Graphics.Blit(_AccumulationTexture, dest);
        }
    }
}