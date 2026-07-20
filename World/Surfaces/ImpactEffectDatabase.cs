using System.Collections.Generic;
using UnityEngine;

namespace ZigdarkS.ProjectB.World.Surfaces
{
    [CreateAssetMenu(menuName = "ProjectB/Surfaces/Impact Effect Database")]
    public class ImpactEffectDatabase : ScriptableObject
    {
        [System.Serializable]
        public class SurfaceImpactConfig
        {
            [SerializeField] private SurfaceType _surface;
            public SurfaceType Surface => _surface;

            [SerializeField] private GameObject _particlePrefab;
            public GameObject ParticlePrefab => _particlePrefab;

            [SerializeField] private GameObject _decalPrefab;
            public GameObject DecalPrefab => _decalPrefab;

            [SerializeField] private AudioClip[] _sounds;
            public AudioClip[] Sounds => _sounds;

            [SerializeField] [Range(0f, 1f)] private float _volume = 1f;
            public float Volume => _volume;

            [SerializeField] private Vector2 _pitchRange = new Vector2(1f, 1f); 
            public Vector2 PitchRange => _pitchRange;
        }

        [SerializeField] private SurfaceImpactConfig[] _entries;
        [SerializeField] private SurfaceImpactConfig _defaultConfig;

        private Dictionary<SurfaceType, SurfaceImpactConfig> _lookup;

        public SurfaceImpactConfig GetConfigBySurface(SurfaceType type)
        {
            _lookup ??= BuildLookup();
            return _lookup.TryGetValue(type, out var entry) ? entry : _defaultConfig;
        }

        private Dictionary<SurfaceType, SurfaceImpactConfig> BuildLookup()
        {
            var dictionary = new Dictionary<SurfaceType, SurfaceImpactConfig>();
            foreach (var entry in _entries)
            {
                if (entry != null)
                {
                    dictionary[entry.Surface] = entry; 
                }
            }
            return dictionary;
        }
    }
}