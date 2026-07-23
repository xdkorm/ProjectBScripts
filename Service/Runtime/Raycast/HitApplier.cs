using UnityEngine;
using ZigdarkS.ProjectB.Core.Combat;

namespace ZigdarkS.ProjectB.Service.Raycast
{
    public class HitApplier
    {
        public void Apply(RaycastHit hit, Vector3 direction, float damage, float knockbackForce)
        {
            if (damage <= 0f) return;

            if (hit.transform.TryGetComponent<IDirectionalDamageable>(out var directional))
            {
                directional.TakeDamage(damage, direction);
            }
            else if (hit.transform.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
            }

            if (hit.transform.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
            }
        }
    }
}