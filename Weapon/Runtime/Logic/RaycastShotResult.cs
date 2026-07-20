using UnityEngine;

namespace ZigdarkS.ProjectB.Weapon.Logic
{
    public readonly struct RaycastShotResult
    {
        public readonly bool DidHit;
        public readonly Vector3 EndPoint;
        public readonly Vector3 Normal;
        public readonly Collider HitCollider;

        public RaycastShotResult(bool didHit, Vector3 endPoint, Vector3 normal, Collider hitCollider)
        {
            DidHit = didHit;
            EndPoint = endPoint;
            Normal = normal;
            HitCollider = hitCollider;
        }
    }
}