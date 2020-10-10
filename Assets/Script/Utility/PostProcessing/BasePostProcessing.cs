using UnityEngine;

namespace Thunder.Utility.PostProcessing
{
    public abstract class BasePostProcessing
    {
        protected Material _Mat;
        public bool Enable;
        public Shader Shader;

        public virtual void Init()
        {
            _Mat = Shader == null ? null : new Material(Shader);
        }

        public abstract void Process(RenderTexture source, RenderTexture dest);
    }
}