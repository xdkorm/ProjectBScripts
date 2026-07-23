using UnityEngine;
using ZigdarkS.ProjectB.Weapon.Data;
using ZigdarkS.ProjectB.Weapon.Logic;
using ZigdarkS.ProjectB.Weapon.View;
using ZigdarkS.ProjectB.Service.Raycast;
using ZigdarkS.ProjectB.Service.Projectiles;

namespace ZigdarkS.ProjectB.Weapon.Factory
{
    public class WeaponFactory
    {
        private readonly RaycastFireService _fireService;
        private readonly IMovementSpreadProvider _movementSpreadProvider;
        private readonly WeaponEffectsCoordinator _effectsService;
        private readonly BallisticProjectileSimulator _ballisticService;

        public WeaponFactory(
            RaycastFireService fireService,
            IMovementSpreadProvider movementSpreadProvider,
            WeaponEffectsCoordinator effectsService,
            BallisticProjectileSimulator ballisticService)
        {
            _fireService = fireService;
            _movementSpreadProvider = movementSpreadProvider;
            _effectsService = effectsService;
            _ballisticService = ballisticService;
        }

        public IWeapon Create(WeaponConfig config, Transform handContainer)
        {
            if (config == null || config.Visuals.ModelPrefab == null)
            {
                Debug.LogError("[WeaponFactory] Не удается создать оружие: неверный конфиг или отсутствует префаб!");
                return null;
            }

            GameObject weaponObject = Object.Instantiate(config.Visuals.ModelPrefab, handContainer);
            weaponObject.transform.localPosition = Vector3.zero;
            weaponObject.transform.localRotation = Quaternion.identity;

            if (!weaponObject.TryGetComponent<WeaponView>(out var weaponView))
            {
                Debug.LogWarning($"[WeaponFactory] На префабе {config.Visuals.ModelPrefab.name} отсутствует WeaponView! Добавляем компонент принудительно.");
                weaponView = weaponObject.AddComponent<WeaponView>();
            }
            weaponView.Initialize(config.Visuals.MuzzleFlashPrefab);

            var ammoState = new AmmoState(config.Ammo);
            var reloadController = new ReloadController(config.Ammo, config.ReloadMode, ammoState, weaponView, _effectsService);

            var weaponInstance = new WeaponInstance(config, weaponView, _fireService,
                _movementSpreadProvider, _effectsService, _ballisticService, ammoState, reloadController);

            return weaponInstance;
        }
    }
}