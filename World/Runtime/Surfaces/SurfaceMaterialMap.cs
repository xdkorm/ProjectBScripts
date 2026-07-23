using System.Collections.Generic;
using UnityEngine;

namespace ZigdarkS.ProjectB.World.Surfaces
{
    [CreateAssetMenu(menuName = "ProjectB/Surfaces/Surface Material Map")]
    public class SurfaceMaterialMap : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public SurfaceType Surface;
            public PhysicsMaterial PhysMaterial;
        }

        [SerializeField] private Entry[] _entries;
        [SerializeField] private SurfaceType _fallback = SurfaceType.Default;

        private Dictionary<PhysicsMaterial, SurfaceType> _lookup;

        public SurfaceType GetSurfaceType(PhysicsMaterial physMaterial)
        {
            if (physMaterial == null) return _fallback;

            _lookup ??= BuildLookup();

            return _lookup.TryGetValue(physMaterial, out var surface) ? surface : _fallback;
        }

        private Dictionary<PhysicsMaterial, SurfaceType> BuildLookup()
        {
            var dictionary = new Dictionary<PhysicsMaterial, SurfaceType>();
            foreach (var entry in _entries)
            {
                if (entry.PhysMaterial != null)
                    dictionary[entry.PhysMaterial] = entry.Surface;
            }
            return dictionary;
        }
    }
}