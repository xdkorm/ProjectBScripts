using System.Collections.Generic;
using UnityEngine;

namespace ZigdarkS.ProjectB.Weapon.View
{
    public class WeaponView : MonoBehaviour
    {
        [Header("Effects")]
        [SerializeField] private ParticleSystem _muzzleFlash;
        [SerializeField] private Transform _muzzlePoint;

        [Header("Ammo Sockets")]
        [Tooltip("Точка вылета гильзы (обычно сбоку затвора)")]
        [SerializeField] private Transform _casingEjectPoint;
        [Tooltip("Точка, откуда выпадает магазин при перезарядке")]
        [SerializeField] private Transform _magazineDropPoint;
        [Tooltip("Сам меш магазина на пушке, чтобы можно было спрятать/показать во время реload")]
        [SerializeField] private GameObject _magazineVisual;

        [Header("Animations (Optional)")]
        [Tooltip("Аниматор самой пушки для затвора/магазина. Пока пушки статические — оставь пустым.")]
        [SerializeField] private Animator _weaponAnimator;

        private Dictionary<string, float> _clipLengths;

        public Transform MuzzlePoint => _muzzlePoint != null ? _muzzlePoint : transform;
        public Transform CasingEjectPoint => _casingEjectPoint != null ? _casingEjectPoint : MuzzlePoint;
        public Transform MagazineDropPoint => _magazineDropPoint != null ? _magazineDropPoint : transform;

        public void Initialize(ParticleSystem muzzleFlashPrefab)
        {
            if (_muzzleFlash == null && muzzleFlashPrefab != null)
            {
                _muzzleFlash = Instantiate(muzzleFlashPrefab, MuzzlePoint);
                _muzzleFlash.transform.localPosition = Vector3.zero;
                _muzzleFlash.transform.localRotation = Quaternion.identity;
            }

            CacheClipLengths();
        }

        private void CacheClipLengths()
        {
            _clipLengths = new Dictionary<string, float>();
            if (_weaponAnimator == null || _weaponAnimator.runtimeAnimatorController == null) return;

            foreach (var clip in _weaponAnimator.runtimeAnimatorController.animationClips)
            {
                _clipLengths[clip.name] = clip.length;
            }
        }

        public void PlayCycleAnimation(float duration)
        {
            // trigger анимации передёргивания затвора/помпы
        }

        public void PlayMuzzleFlash()
        {
            if (_muzzleFlash != null)
                _muzzleFlash.Play();
        }

        public void PlayWeaponShootAnimation()
        {
            if (_weaponAnimator != null)
            {
                _weaponAnimator.Play("Shoot", -1, 0f);
            }
        }

        public void PlayDrawAnimation(float targetDuration = 0f) => PlayScaled("Draw", targetDuration);
        public void PlayHolsterAnimation(float targetDuration = 0f) => PlayScaled("Holster", targetDuration);
        public void PlayReloadAnimation(float targetDuration = 0f) => PlayScaled("Reload", targetDuration);
        public void PlayReloadExitAnimation(float targetDuration = 0f) => PlayScaled("ReloadExit", targetDuration);

        /// <summary>
        /// Проигрывает состояние аниматора, подгоняя скорость так, чтобы клип уложился в targetDuration.
        /// Если targetDuration &lt;= 0 или клип не найден в кэше — играет с обычной скоростью (1x).
        /// </summary>
        private void PlayScaled(string stateName, float targetDuration)
        {
            if (_weaponAnimator == null) return;

            float speed = 1f;
            if (targetDuration > 0.0001f && _clipLengths != null && _clipLengths.TryGetValue(stateName, out float clipLength))
            {
                speed = clipLength / targetDuration;
            }

            _weaponAnimator.speed = speed;
            _weaponAnimator.Play(stateName, -1, 0f);
        }

        /// <summary>
        /// Прячет/показывает меш магазина на пушке (например, во время анимации перезарядки).
        /// </summary>
        public void SetMagazineVisualActive(bool isActive)
        {
            if (_magazineVisual != null)
                _magazineVisual.SetActive(isActive);
        }

        /// <summary>
        /// Динамически подменяет анимационный контроллер пушки (или персонажа) при смене режима стрельбы.
        /// </summary>
        public void ApplyAnimatorOverride(AnimatorOverrideController overrideController)
        {
            if (_weaponAnimator != null)
            {
                _weaponAnimator.runtimeAnimatorController = overrideController;
                CacheClipLengths(); // контроллер сменился — клипы могли смениться, пере-кэшируем
            }
        }
    }
}