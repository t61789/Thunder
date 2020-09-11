using System;
using UnityEngine;

namespace Thunder.Utility.PostProcessing
{
    [Serializable]
    public class AimScope : BasePostProcessing
    {
        public Texture2D AimScopeTex;

        public override void Init()
        {
            base.Init();
            _Mat.SetTexture("_AimScope", AimScopeTex);
            _Mat.SetFloat("_UVXScale", Screen.width / (float)Screen.height);
        }

        public override void Process(RenderTexture source, RenderTexture dest)
        {
            Graphics.Blit(source, dest, _Mat);
        }
    }
}
