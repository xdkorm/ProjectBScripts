using System;
using UnityEngine;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Enemy.View;

namespace ZigdarkS.ProjectB.Enemy.Logic.Combat
{
    public class EnemyLimb : MonoBehaviour, IDamageable
    {
        [Header("--- Настройки ---")]
        [SerializeField] private LimbDamageSettings _damageSettings;
        [SerializeField] private LimbGoreSettings _goreSettings;

        [Header("--- Ссылки ---")]
        [SerializeField]
        [Tooltip("Ссылка на главный класс EnemyView на теле врага.")]
        private EnemyView _mainView;

        public LimbDamageSettings DamageSettings => _damageSettings;
        public LimbGoreSettings GoreSettings => _goreSettings;
        public EnemyView MainView => _mainView;

        // Сервис сам решает, оторвать лимб или нет; View лишь сообщает о попадании.
        public event Action<EnemyLimb, float> OnHit;

        public void TakeDamage(float damage)
        {
            OnHit?.Invoke(this, damage);
        }

        // Вызывается сервисом, когда принято решение "оторвать".
        // Здесь остаётся только то, что физически обязано жить на GameObject лимба.
        public void PlayBreakEffects(Vector3 flyDirection)
        {
            if (_goreSettings.HideOriginalLimb)
            {
                transform.localScale = Vector3.zero;
            }

            if (_goreSettings.ObjectsToDestroyOnBreak != null)
            {
                foreach (GameObject obj in _goreSettings.ObjectsToDestroyOnBreak)
                {
                    if (obj != null) Destroy(obj);
                }
            }

            if (_goreSettings.SpawnSeveredLimb && _goreSettings.SeveredLimbPrefabs != null)
            {
                Vector3 direction = flyDirection.sqrMagnitude > 0.0001f ? flyDirection.normalized : Vector3.up;

                foreach (GameObject prefab in _goreSettings.SeveredLimbPrefabs)
                {
                    if (prefab == null) continue;

                    GameObject spawnedGore = Instantiate(prefab, transform.position, transform.rotation);

                    if (spawnedGore.TryGetComponent<Rigidbody>(out var rb))
                    {
                        Vector3 force = (direction + UnityEngine.Random.onUnitSphere * 0.5f).normalized * 5f;
                        rb.AddForce(force, ForceMode.Impulse);
                    }
                }
            }
        }
    }
}