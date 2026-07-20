using VContainer;
using VContainer.Unity;
using UnityEngine;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Service.Weapon;
using ZigdarkS.ProjectB.Player.Logic;
using ZigdarkS.ProjectB.Player.Logic.Movement;
using ZigdarkS.ProjectB.Player.View;
using ZigdarkS.ProjectB.Player.HUD;
using ZigdarkS.ProjectB.Player.Data;
using ZigdarkS.ProjectB.Service.Input;
using ZigdarkS.ProjectB.Service.Raycast;
using ZigdarkS.ProjectB.Weapon.Factory;
using ZigdarkS.ProjectB.Weapon.Inventory;
using ZigdarkS.ProjectB.Player.Logic.Movement.States;
using ZigdarkS.ProjectB.Player.HUD.Crosshair;
using ZigdarkS.ProjectB.Enemy.Data;
using ZigdarkS.ProjectB.Enemy.Logic;
using ZigdarkS.ProjectB.Enemy.Logic.Combat;
using ZigdarkS.ProjectB.Enemy.Logic.Navigation;
using ZigdarkS.ProjectB.Enemy.Logic.Perception;
using ZigdarkS.ProjectB.World.Surfaces;
using ZigdarkS.ProjectB.Weapon.Logic;
using ZigdarkS.ProjectB.Player.Logic.Weapon;

namespace ZigdarkS.ProjectB.Infrastructure
{
    public class GameLifetimeScope : LifetimeScope
    {
        [Header("Scene References")]
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private PlayerContainer _playerContainerPrefab;
        [SerializeField] private HUDView _playerHUDView;
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private PlayerLoadoutConfig _startingLoadout;
        [SerializeField] private CrosshairView _crosshairView;
        [SerializeField] private CrosshairConfig _crosshairConfig;
        [SerializeField] private EnemyConfig _enemyConfig;
        [SerializeField] private ImpactEffectDatabase _impactEffectDatabase;
        [SerializeField] private SurfaceMaterialMap _surfaceMaterialMap;
        [SerializeField] private WeaponEffectsConfig _weaponEffectsConfig;

        protected override void Configure(IContainerBuilder builder)
        {
//---------------------------------------------------------------------------------------------------//
///////////////////////////////////////////////////////////////////////////////////////////////////////
                            // 1. Монобехи и Префабы с инспектора
///////////////////////////////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------------------------//
            builder.RegisterComponent(_spawnPoint);
            builder.RegisterComponent(_playerHUDView);
            builder.RegisterComponent(_crosshairView);
//---------------------------------------------------------------------------------------------------//
            builder.RegisterInstance(_playerContainerPrefab);
            builder.RegisterInstance(_playerConfig);
            builder.RegisterInstance(_startingLoadout);
            builder.RegisterInstance(_crosshairConfig);
            builder.RegisterInstance(_enemyConfig);
            builder.RegisterInstance(_impactEffectDatabase);
            builder.RegisterInstance(_surfaceMaterialMap);
            builder.RegisterInstance(_weaponEffectsConfig);
//---------------------------------------------------------------------------------------------------//
///////////////////////////////////////////////////////////////////////////////////////////////////////
                            // 2. Чистые C# Сервисы (Синглы и Скоупед)
///////////////////////////////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------------------------//
            builder.Register<PlayerModel>(Lifetime.Scoped);
            builder.Register<WeaponFactory>(Lifetime.Scoped);
            builder.Register<CrosshairModel>(Lifetime.Scoped);
            builder.Register<EnemyNavigator>(Lifetime.Scoped);
            builder.Register<EnemyLimbService>(Lifetime.Transient);

            // Регистрируем основной провайдер для игрока
            builder.Register<InstanceProvider<PlayerView>>(Lifetime.Scoped);

            // Создаем "мост", который превращает PlayerView в ITargetable для врагов
            builder.Register<InstanceProvider<ITargetable>>(container =>
            {
                var playerProvider = container.Resolve<InstanceProvider<PlayerView>>();
                var targetableProvider = new InstanceProvider<ITargetable>();

                // Подписываемся на события спавна игрока и перенаправляем их врагам
                playerProvider.OnSpawned += view => targetableProvider.Register(view);
                playerProvider.OnDespawned += () => targetableProvider.Unregister();

                // На случай, если на момент резолва игрок уже существует
                if (playerProvider.Instance != null)
                {
                    targetableProvider.Register(playerProvider.Instance);
                }

                return targetableProvider;
            }, Lifetime.Scoped);

            builder.Register<EnemyStateFactory>(Lifetime.Scoped);
            builder.Register<EnemyCombatService>(Lifetime.Scoped);
            builder.Register<MovementCalculator>(Lifetime.Scoped);
            builder.Register<MouseLookController>(Lifetime.Scoped);
            builder.Register<MovementStateFactory>(Lifetime.Scoped);
            builder.Register<VaultDetector>(Lifetime.Scoped);
            builder.Register<PlayerSettings>(Lifetime.Scoped);
            builder.Register<WeaponInventory>(Lifetime.Scoped).As<IWeaponInventory>().AsSelf();
//---------------------------------------------------------------------------------------------------//
            builder.Register<HitApplier>(Lifetime.Singleton);
            builder.Register<RaycastFireService>(Lifetime.Singleton);
            builder.Register<EnemySquadRegistry>(Lifetime.Singleton);
            builder.Register<ISurfaceResolver, SurfaceResolver>(Lifetime.Singleton);
            builder.Register<InputService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<WeaponEffectsService>(Lifetime.Singleton);
            builder.Register<ISoundEventBus, SoundEventBus>(Lifetime.Singleton);
            builder.Register<IFovCalculator>(Lifetime.Singleton);
//---------------------------------------------------------------------------------------------------//
///////////////////////////////////////////////////////////////////////////////////////////////////////
                            // Состояния движения (Transient)
///////////////////////////////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------------------------//
            builder.Register<IdleState>(Lifetime.Transient);
            builder.Register<WalkingState>(Lifetime.Transient);
            builder.Register<SprintingState>(Lifetime.Transient);
            builder.Register<AirborneState>(Lifetime.Transient);
            builder.Register<CrouchingState>(Lifetime.Transient);
            builder.Register<SlidingState>(Lifetime.Transient);
            builder.Register<AirborneCrouchState>(Lifetime.Transient);
            builder.Register<VaultingState>(Lifetime.Transient);
            builder.Register<MantlingState>(Lifetime.Transient);
//---------------------------------------------------------------------------------------------------//
///////////////////////////////////////////////////////////////////////////////////////////////////////
                            // Состояния AI врагов (Transient)
///////////////////////////////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------------------------//
            builder.Register<ProjectB.Enemy.Logic.States.IdleState>(Lifetime.Transient);
            builder.Register<ProjectB.Enemy.Logic.States.ChaseState>(Lifetime.Transient);
            builder.Register<ProjectB.Enemy.Logic.States.CombatState>(Lifetime.Transient);
            builder.Register<ProjectB.Enemy.Logic.States.ReloadCoverState>(Lifetime.Transient);
            builder.Register<ProjectB.Enemy.Logic.States.SearchState>(Lifetime.Transient);
            builder.Register<ProjectB.Enemy.Logic.States.DeadState>(Lifetime.Transient);
            builder.Register<ProjectB.Enemy.Logic.States.AlertState>(Lifetime.Transient);
            builder.Register<ProjectB.Enemy.Logic.States.SuppressState>(Lifetime.Transient);
            builder.Register<ProjectB.Enemy.Logic.States.PatrolState>(Lifetime.Transient);
//---------------------------------------------------------------------------------------------------//
///////////////////////////////////////////////////////////////////////////////////////////////////////
                            // 3. Фабрики для MVP
///////////////////////////////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------------------------//
            builder.Register<PlayerPresenter.Factory>(container =>
            {
                return new PlayerPresenter.Factory(view =>
                    new PlayerPresenter(
                        container.Resolve<PlayerModel>(), 
                        view,
                        container.Resolve<PlayerSettings>()));
            }, Lifetime.Scoped);

            builder.Register<EnemyPresenter.Factory>(container => (view, model) =>
            {
                return new EnemyPresenter(
                    view,
                    model,
                    container.Resolve<EnemyConfig>(),
                    container.Resolve<InstanceProvider<ITargetable>>(), // ЗАМЕНЕНО С PlayerView НА ITargetable
                    container.Resolve<EnemyStateFactory>(),
                    container.Resolve<EnemyNavigator>(),
                    container.Resolve<EnemyCombatService>(),
                    container.Resolve<EnemyLimbService>(),
                    container.Resolve<ISoundEventBus>(),
                    container.Resolve<EnemySquadRegistry>());
            }, Lifetime.Scoped);
//---------------------------------------------------------------------------------------------------//
///////////////////////////////////////////////////////////////////////////////////////////////////////
                            // 4. Точки входа (Entry Points) и Системы
///////////////////////////////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------------------------//
            builder.RegisterEntryPoint<HUDPresenter>();
            builder.RegisterEntryPoint<PlayerSpawner>().AsSelf();
            builder.RegisterEntryPoint<EnemySystem>();
            builder.RegisterEntryPoint<MovementSystem>().AsSelf();
            builder.RegisterEntryPoint<CrosshairPresenter>();
            builder.RegisterEntryPoint<MovementSpreadService>().AsSelf();
            builder.RegisterEntryPoint<BallisticProjectileService>().AsSelf();
            builder.RegisterEntryPoint<CameraSwayController>();
            builder.RegisterEntryPoint<WeaponSwitchingSystem>();
            builder.RegisterEntryPoint<FireModeSystem>();
            builder.RegisterEntryPoint<WeaponAdsSystem>();
            builder.RegisterEntryPoint<WeaponReloadSystem>();
            builder.RegisterEntryPoint<WeaponActionCycleSystem>();
            builder.RegisterEntryPoint<PlayerShootingSystem>();
            builder.Register<IActivePlayerView, ActivePlayerViewTracker>(Lifetime.Singleton).AsSelf();
        }
    }
}