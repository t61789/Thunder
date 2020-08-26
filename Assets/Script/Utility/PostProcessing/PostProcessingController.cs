using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Thunder.Utility.PostProcessing;
using UnityEngine;

namespace Thunder.Utility
{
    [RequireComponent(typeof(Camera))]
    public class PostProcessingController:MonoBehaviour
    {
        public static PostProcessingController Instance;
        public Bsc Bsc;
        public EdgeDetect EdgeDetect;
        public GaussBlur GaussBlur;
        public Bloom Bloom;
        public MotionBlur MotionBlur;
        public SMotionBlur SMotionBlur;
        public DepthTexture DepthTexture;
        public DepthOfField DepthOfField;
        public LDepthOfField LDepthOfField;
        public Ssao Ssao;
        public AimScope AimScope;

        private RenderTexture _Souce;
        private Camera _Camera;
        private readonly List<BasePostProcessing> _Processings = new List<BasePostProcessing>();

        private void Start()
        {
            Instance = this;
            _Camera = GetComponent<Camera>();
            _Camera.depthTextureMode = DepthTextureMode.Depth |
                                        DepthTextureMode.DepthNormals;

            foreach (var processing in GetType().
                GetFields(BindingFlags.Public | BindingFlags.Instance).
                Where(x => x.FieldType.IsSubclassOf(typeof(BasePostProcessing))))
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
            RenderTexture buffer = RenderTexture.GetTemporary(_Souce.width, _Souce.height, 0);

            RenderTexture source = _Souce;
            RenderTexture dest = buffer;

            foreach (var processing in _Processings.Where(processing => processing.Enable))
            {
                processing.Process(source, dest);
                RenderTexture temp = source;
                source = dest;
                dest = temp;
            }

            Graphics.Blit(source, null as RenderTexture);
            RenderTexture.ReleaseTemporary(dest);
            RenderTexture.ReleaseTemporary(source);
        }
    }
}
