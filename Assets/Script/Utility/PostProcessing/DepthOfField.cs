using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Rendering;

namespace Thunder.Utility.PostProcessing
{
    [Serializable]
    public class DepthOfField:BasePostProcessing
    {
        [Range(1,10)]
        public int Iterations = 1;
        public float MaxBlurSize = 0.5f;
        [Range(0, 1)]
        public float MiddleDepth = 0.5f;
        public float DistanceThreshold = 0.5f;

        public override void Process(RenderTexture source, RenderTexture dest)
        {
            RenderTexture gaussTemp0 = RenderTexture.GetTemporary(source.width, source.height, 0);
            gaussTemp0.filterMode = FilterMode.Bilinear;
            Graphics.Blit(source, gaussTemp0);

            _Mat.SetFloat("_MaxBlurSize", MaxBlurSize);
            _Mat.SetFloat("_MiddleDepth", MiddleDepth);
            _Mat.SetFloat("_DistanceThreshold", DistanceThreshold);

            for (int i = 0; i < Iterations; i++)
            {
                RenderTexture gaussTemp1 = RenderTexture.GetTemporary(gaussTemp0.width, gaussTemp0.height, 0);
                _Mat.SetInt("_BlurVertical", 1);
                Graphics.Blit(gaussTemp0, gaussTemp1, _Mat);
                RenderTexture.ReleaseTemporary(gaussTemp0);

                gaussTemp0 = RenderTexture.GetTemporary(gaussTemp1.width, gaussTemp1.height, 0);
                _Mat.SetInt("_BlurVertical", 0);
                Graphics.Blit(gaussTemp1, gaussTemp0, _Mat);
                RenderTexture.ReleaseTemporary(gaussTemp1);
            }

            Graphics.Blit(gaussTemp0, dest);
            RenderTexture.ReleaseTemporary(gaussTemp0);
        }
    }
}
