using System;
using UnityEngine;

namespace Thunder
{
    [Serializable]
    public class DepthTexture : BasePostProcessing
    {
        public override void Process(RenderTexture source, RenderTexture dest)
        {
            var tempTexture = RenderTexture.GetTemporary(source.width, source.height, 0);
            Graphics.Blit(source, dest, _Mat);
        }
    }
}