using Tool;

using UnityEngine;

namespace Thunder.Utility
{
    public class SelfDestroyPartical : MonoBehaviour, IObjectPool
    {
        private ParticleSystem _Particle;

        public AssetId AssetId { get; set; }

        public void OpReset()
        {
        }

        public void OpRecycle()
        {
        }

        public void OpDestroy()
        {
        }

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
    }
}