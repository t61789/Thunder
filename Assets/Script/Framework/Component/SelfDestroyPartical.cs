using UnityEngine;

namespace Framework
{
    public class SelfDestroyPartical : MonoBehaviour, IObjectPool
    {
        private ParticleSystem _Particle;

        public AssetId AssetId { get; set; }

        public void OpReset(){}

        public void OpPut(){}

        public void OpDestroy(){}

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
                GameObjectPool.Put(this);
        }
    }
}