using UnityEngine;
using ZigdarkS.ProjectB.Service.Effects;
using ZigdarkS.ProjectB.Service.Pooling;
using ZigdarkS.ProjectB.World.Surfaces;

namespace ZigdarkS.ProjectB.Weapon.Logic
{
    /// <summary>
    /// Отвечает только за проигрывание звука попадания по конфигу поверхности.
    /// </summary>
    public class ImpactSoundPlayer
    {
        private readonly PrefabPoolRegistry _pools;
        private readonly GameObject _audioSourcePrefab;

        public ImpactSoundPlayer(PrefabPoolRegistry pools, GameObject audioSourcePrefab)
        {
            _pools = pools;
            _audioSourcePrefab = audioSourcePrefab;
        }

        public void Play(ImpactEffectDatabase.SurfaceImpactConfig entry, Vector3 point)
        {
            if (entry.Sounds == null || entry.Sounds.Length == 0 || _audioSourcePrefab == null)
                return;

            var clip = entry.Sounds[Random.Range(0, entry.Sounds.Length)];
            if (clip == null) return;

            var pool = _pools.GetPool(_audioSourcePrefab);
            var instance = pool.Get();
            instance.transform.position = point;

            if (instance.TryGetComponent<PooledAudioSource>(out var pooledAudio))
            {
                float pitch = Random.Range(entry.PitchRange.x, entry.PitchRange.y);
                pooledAudio.PlayAndReturn(clip, pool, entry.Volume, pitch);
            }
        }
    }
}