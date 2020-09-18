using Thunder.Sys;
using Thunder.Tool.ObjectPool;
using UnityEngine;

namespace Thunder.Utility
{
    public class SelfDestroyPartical : MonoBehaviour, IObjectPool
    {
        private ParticleSystem _Particle;

        public AssetId AssetId { get; set; }

        private void Awake()
        {
            _Particle = GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            _Particle.Play();
        }

        private void FixedUpdate()
        {
            if (!_Particle.isPlaying)
                ObjectPool.Ins.Recycle(this);
        }

        public void BeforeOpReset()
        {

        }

        public void BeforeOpRecycle()
        {
        }

        public void AfterOpDestroy()
        {

        }
    }
}
