using System;
using UnityEngine;

namespace Thunder.Utility.PostProcessing
{
    [Serializable]
    public class EdgeDetect : BasePostProcessing
    {
        public Color BackgroundColor = Color.white;
        public Color EdgeColor = Color.black;

        [Range(0, 3)] public float EdgeFactor = 1;

        [Range(0, 1)] public float EdgesOnly = 0;

        public override void Process(RenderTexture source, RenderTexture dest)
        {
            _Mat.SetFloat("_EdgesOnly", EdgesOnly);
            _Mat.SetColor("_EdgeColor", EdgeColor);
            _Mat.SetColor("_BackgroundColor", BackgroundColor);
            _Mat.SetFloat("_EdgeFactor", EdgeFactor);
            Graphics.Blit(source, dest, _Mat);
        }
    }
}