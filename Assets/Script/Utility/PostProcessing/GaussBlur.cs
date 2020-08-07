using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Thunder.Utility.PostProcessing
{
    [Serializable]
    public class GaussBlur:BasePostProcessing
    {
        [Range(0, 4)]
        public int Iterations = 3;
        [Range(0.2f, 3f)]
        public float BlurSpread = 0.6f;
        [Range(1, 8)]
        public int DownSample = 1;

        public override void Process(RenderTexture source, RenderTexture dest)
        {
            Blur(Iterations, BlurSpread, DownSample, source, dest,_Mat);
        }

        public static void Blur(int iterations, float blurSpread, int downSample, RenderTexture source,RenderTexture dest,Material gaussMaterial)
        {
            RenderTexture gaussTemp0 = RenderTexture.GetTemporary(source.width / downSample, source.height / downSample, 0);
            gaussTemp0.filterMode = FilterMode.Bilinear;
            Graphics.Blit(source, gaussTemp0);

            for (int i = 0; i < iterations; i++)
            {
                gaussMaterial.SetFloat("_BlurSize", 1 + i * blurSpread);
                RenderTexture gaussTemp1 = RenderTexture.GetTemporary(gaussTemp0.width, gaussTemp0.height, 0);
                Graphics.Blit(gaussTemp0, gaussTemp1, gaussMaterial, 0);
                RenderTexture.ReleaseTemporary(gaussTemp0);
                gaussTemp0 = RenderTexture.GetTemporary(gaussTemp1.width, gaussTemp1.height, 0);
                Graphics.Blit(gaussTemp1, gaussTemp0, gaussMaterial, 1);
                RenderTexture.ReleaseTemporary(gaussTemp1);
            }

            Graphics.Blit(gaussTemp0, dest);
            RenderTexture.ReleaseTemporary(gaussTemp0);
        }
    }
}
