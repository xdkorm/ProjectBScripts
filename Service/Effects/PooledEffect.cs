using UnityEngine;
using UnityEngine.Pool;

namespace ZigdarkS.ProjectB.Service.Effects
{
    [RequireComponent(typeof(ParticleSystem))]
    public class PooledEffect : MonoBehaviour
    {
        private ObjectPool<GameObject> _pool;
        private ParticleSystem _ps;

        private void Awake()
        {
            _ps = GetComponent<ParticleSystem>();
            var main = _ps.main;
            main.stopAction = ParticleSystemStopAction.Callback;
        }

        public void ReturnTo(ObjectPool<GameObject> pool) => _pool = pool;

        private void OnParticleSystemStopped()
        {
            _pool?.Release(gameObject);
        }
    }
}