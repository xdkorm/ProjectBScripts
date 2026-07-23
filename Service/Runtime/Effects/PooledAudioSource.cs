using UnityEngine;
using UnityEngine.Pool;

namespace ZigdarkS.ProjectB.Service.Effects
{
    [RequireComponent(typeof(AudioSource))]
    public class PooledAudioSource : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        private ObjectPool<GameObject> _pool;
        private float _returnAt;
        private bool _waitingForReturn;

        private void Awake()
        {
            if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
        }

        public void PlayAndReturn(AudioClip clip, ObjectPool<GameObject> pool, float volume = 1f, float pitch = 1f)
        {
            _pool = pool;
            _audioSource.pitch = pitch;
            _audioSource.PlayOneShot(clip, volume);

            _returnAt = Time.time + clip.length / Mathf.Max(0.01f, pitch);
            _waitingForReturn = true;
        }

        private void Update()
        {
            if (_waitingForReturn && Time.time >= _returnAt)
            {
                _waitingForReturn = false;
                _pool?.Release(gameObject);
            }
        }
    }
}