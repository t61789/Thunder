using UnityEngine;

namespace Thunder.Utility.PostProcessing
{
    public abstract class BasePostProcessing
    {
        public bool Enable;
        public Shader Shader;
        protected Material _Mat;

        public virtual void Init()
        {
            _Mat = Shader==null?null:new Material(Shader);
        }

        public abstract void Process(RenderTexture source, RenderTexture dest);
    }
}
