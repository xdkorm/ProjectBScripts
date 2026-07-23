using UnityEngine;

namespace ZigdarkS.ProjectB.World.Surfaces
{
    // Вешается на геометрию уровня, если нужно переопределить дефолтную поверхность
    // (например, лужа поверх бетона, или лист металла с покрытием другого материала)
    public class SurfaceIdentifier : MonoBehaviour
    {
        [SerializeField] private SurfaceType _surfaceType = SurfaceType.Default;
        public SurfaceType SurfaceType => _surfaceType;
    }
}