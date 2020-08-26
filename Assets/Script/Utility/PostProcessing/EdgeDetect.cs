﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Thunder.Utility.PostProcessing
{
    [Serializable]
    public class EdgeDetect:BasePostProcessing
    {
        [Range(0, 1)]
        public float EdgesOnly = 0;
        public Color EdgeColor = Color.black;
        public Color BackgroundColor = Color.white;
        [Range(0, 3)]
        public float EdgeFactor = 1;

        public override void Process(RenderTexture source,RenderTexture dest)
        {
            _Mat.SetFloat("_EdgesOnly", EdgesOnly);
            _Mat.SetColor("_EdgeColor", EdgeColor);
            _Mat.SetColor("_BackgroundColor", BackgroundColor);
            _Mat.SetFloat("_EdgeFactor", EdgeFactor);
            Graphics.Blit(source, dest, _Mat);
        }
    }
}