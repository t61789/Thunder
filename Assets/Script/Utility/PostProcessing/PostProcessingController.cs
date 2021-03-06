﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Thunder
{
    [RequireComponent(typeof(Camera))]
    public class PostProcessingController : MonoBehaviour
    {
        public static PostProcessingController Instance;
        private readonly List<BasePostProcessing> _Processings = new List<BasePostProcessing>();
        private Camera _Camera;

        private RenderTexture _Souce;
        public AimScope AimScope;
        public Bloom Bloom;
        public Bsc Bsc;
        public DepthOfField DepthOfField;
        public DepthTexture DepthTexture;
        public EdgeDetect EdgeDetect;
        public GaussBlur GaussBlur;
        public LDepthOfField LDepthOfField;
        public MotionBlur MotionBlur;
        public SMotionBlur SMotionBlur;
        public Ssao Ssao;

        private void Start()
        {
            Instance = this;
            _Camera = GetComponent<Camera>();
            _Camera.depthTextureMode = DepthTextureMode.Depth |
                                       DepthTextureMode.DepthNormals;

            foreach (var processing in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.FieldType.IsSubclassOf(typeof(BasePostProcessing))))
                _Processings.Add(processing.GetValue(this) as BasePostProcessing);
            foreach (var processing in _Processings)
                processing.Init();
        }

        private void OnPreRender()
        {
            _Souce = RenderTexture.GetTemporary(Screen.width, Screen.height, 16);
            _Camera.targetTexture = _Souce;
        }

        private void OnPostRender()
        {
            _Camera.targetTexture = null;
            var buffer = RenderTexture.GetTemporary(_Souce.width, _Souce.height, 0);

            var source = _Souce;
            var dest = buffer;

            foreach (var processing in _Processings.Where(processing => processing.Enable))
            {
                processing.Process(source, dest);
                var temp = source;
                source = dest;
                dest = temp;
            }

            Graphics.Blit(source, null as RenderTexture);
            RenderTexture.ReleaseTemporary(dest);
            RenderTexture.ReleaseTemporary(source);
        }
    }
}