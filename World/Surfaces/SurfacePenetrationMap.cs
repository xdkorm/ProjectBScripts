using System.Collections.Generic;
using UnityEngine;
using ZigdarkS.ProjectB.Core.Combat;

namespace ZigdarkS.ProjectB.World.Surfaces
{
    [CreateAssetMenu(menuName = "ProjectB/Surfaces/Penetration Resistance Map")]
    public class SurfacePenetrationMap : ScriptableObject
    {
        [System.Serializable]
        public class SurfacePenetrationConfig
        {
            [SerializeField] private SurfaceType _surface;
            public SurfaceType Surface => _surface;
            [SerializeField] private MaterialResistance _resistance;
            public MaterialResistance Resistance => _resistance;
        }

        [SerializeField] private SurfacePenetrationConfig[] _entries;
        [SerializeField] private MaterialResistance _default; 

        private Dictionary<SurfaceType, MaterialResistance> _lookup;

        public MaterialResistance GetMaterialResistance(SurfaceType type)
        {
            _lookup ??= BuildLookup();
            return _lookup.TryGetValue(type, out var resistance) ? resistance : _default;
        }

        private Dictionary<SurfaceType, MaterialResistance> BuildLookup()
        {
            var dictionary = new Dictionary<SurfaceType, MaterialResistance>();
            foreach (var entry in _entries)
            {
                if (entry != null)
                {
                    dictionary[entry.Surface] = entry.Resistance;
                }
            }
            return dictionary;
        }
    }
}