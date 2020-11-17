using Framework;
using UnityEngine;

namespace Thunder
{
    public class AudioController : MonoBehaviour
    {
        private AudioSource _AudioSource;
        public AudioClip[] AudioClips;

        private void Awake()
        {
            _AudioSource = gameObject.AddComponent<AudioSource>();
        }

        public void PlayAudio(int index)
        {
            _AudioSource.Stop();
            if (AudioClips == null || !AudioClips.InRange(index)) return;
            _AudioSource.clip = AudioClips[index];
            _AudioSource.Play();
        }
    }
}