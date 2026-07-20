using UnityEngine;
using ZigdarkS.ProjectB.Weapon.View;

namespace ZigdarkS.ProjectB.Weapon.Logic
{
    /// <summary>
    /// Отвечает только за физический дроп магазина при перезарядке.
    /// Не использует пул — магазин выпадает единожды и живёт своей жизнью в мире
    /// (в отличие от VFX/декалей/звука, которые проигрываются часто и коротко).
    /// </summary>
    public class MagazineDropSpawner
    {
        private const float DownForce = 0.5f;
        private const float ForwardForce = 0.2f;

        public void Spawn(WeaponView view, GameObject magazinePrefab)
        {
            if (view == null || magazinePrefab == null) return;

            var dropPoint = view.MagazineDropPoint;
            var instance = Object.Instantiate(magazinePrefab, dropPoint.position, dropPoint.rotation);

            if (instance.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.AddForce(
                    Vector3.down * DownForce + dropPoint.forward * ForwardForce,
                    ForceMode.Impulse);
            }
        }
    }
}