using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Thunder.Utility.PostProcessing
{
    [Serializable]
    public class SMotionBlur : BasePostProcessing
    {
        [Range(0, 0.9f)]
        public float BlurAmount = 0.5f;
        private RenderTexture _AccumulationTexture;

        public override void Process(RenderTexture source, RenderTexture dest)
        {
            if (_AccumulationTexture == null || _AccumulationTexture.width != source.width || _AccumulationTexture.height != source.height)
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
