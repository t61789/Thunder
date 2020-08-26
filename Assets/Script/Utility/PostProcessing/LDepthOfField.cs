using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Thunder.Utility.PostProcessing
{
    [Serializable]
    public class LDepthOfField:GaussBlur
    {
        [Range(0,1)]
        public float MiddleDepth = 0.5f;
        public Shader GaussBlurShader;

        private Material _GaussBlurMat;

        public override void Init()
        {
            base.Init();

            _GaussBlurMat = new Material(GaussBlurShader);
        }

        public override void Process(RenderTexture source, RenderTexture dest)
        {
            RenderTexture gauss = RenderTexture.GetTemporary(source.width,source.height,0);
            Blur(Iterations,BlurSpread,DownSample,source,gauss,_GaussBlurMat);
            _Mat.SetTexture("_Gauss",gauss);
            _Mat.SetFloat("_MiddleDepth",MiddleDepth);
            Graphics.Blit(source,dest,_Mat);
            RenderTexture.ReleaseTemporary(gauss);
        }
    }
}
