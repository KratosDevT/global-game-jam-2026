using UnityEngine;

namespace Script.Sound
{
    
    [RequireComponent(typeof(AudioSource))]
    public class SoundHandler : MonoBehaviour
    {
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.loop = true;
        }

        private void Start()
        {
            _audioSource.Play();
        }
    }
}