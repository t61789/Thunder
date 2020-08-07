using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Thunder.Utility.PostProcessing
{
    [Serializable]
    public class DepthTexture:BasePostProcessing
    {
        public override void Process(RenderTexture source, RenderTexture dest)
        {
            RenderTexture tempTexture = RenderTexture.GetTemporary(source.width, source.height, 0);
            Graphics.Blit(source, dest, _Mat);
        }
    }
}
