using UnityEngine;

namespace ZigdarkS.ProjectB.World.Surfaces
{
    public interface ISurfaceResolver
    {
        SurfaceType Resolve(Collider collider, PhysicsMaterial physMaterial);
    }

    public class SurfaceResolver : ISurfaceResolver
    {
        private readonly SurfaceMaterialMap _map; // ScriptableObject: PhysicMaterial -> SurfaceType

        public SurfaceResolver(SurfaceMaterialMap map) => _map = map;

        public SurfaceType Resolve(Collider collider, PhysicsMaterial physMaterial)
        {
            if (collider.TryGetComponent<SurfaceIdentifier>(out var id))
                return id.SurfaceType;

            return _map.GetSurfaceType(physMaterial); // fallback Default внутри
        }
    }
}