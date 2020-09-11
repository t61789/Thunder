using Thunder.Tool;
using UnityEngine;

namespace Thunder.Utility
{
    public class AudioController : MonoBehaviour
    {
        public AudioClip[] AudioClips;

        private AudioSource _AudioSource;

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
