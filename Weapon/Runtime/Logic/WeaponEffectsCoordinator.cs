using UnityEngine;
using ZigdarkS.ProjectB.World.Surfaces;
using ZigdarkS.ProjectB.Service.Pooling;
using ZigdarkS.ProjectB.Weapon.View;
using ZigdarkS.ProjectB.Core.Combat;

namespace ZigdarkS.ProjectB.Weapon.Logic
{
    /// <summary>
    /// Фасад-оркестратор: связывает результат рейкаста с нужными
    /// спавнерами эффектов (частицы, декали, звук) и дропом магазина.
    /// Сам эффекты не создаёт — только координирует специализированные классы.
    /// </summary>
    public class WeaponEffectsCoordinator : IImpactEffectSpawner
    {
        private readonly ImpactEffectDatabase _database;
        private readonly ISurfaceResolver _surfaceResolver;
        private readonly ImpactParticleSpawner _particleSpawner;
        private readonly DecalSpawner _decalSpawner;
        private readonly ImpactSoundPlayer _soundPlayer;
        private readonly MagazineDropSpawner _magazineDropSpawner;

        public WeaponEffectsCoordinator(
            ImpactEffectDatabase database,
            ISurfaceResolver surfaceResolver,
            WeaponEffectsConfig config)
        {
            _database = database;
            _surfaceResolver = surfaceResolver;

            var poolRoot = new GameObject("[ImpactEffectsPool]").transform;
            var pools = new PrefabPoolRegistry(poolRoot);

            _particleSpawner = new ImpactParticleSpawner(pools);
            _decalSpawner = new DecalSpawner(pools);
            _soundPlayer = new ImpactSoundPlayer(pools, config.PooledAudioSourcePrefab);
            _magazineDropSpawner = new MagazineDropSpawner();
        }

        public void SpawnImpactEffect(GameObject weaponOverridePrefab, RaycastHit hit)
        {
            // sharedMaterial не создаёт дубликатов в памяти
            var surface = _surfaceResolver.Resolve(hit.collider, hit.collider.sharedMaterial);
            var entry = _database.GetConfigBySurface(surface);
            var particlePrefab = weaponOverridePrefab != null ? weaponOverridePrefab : entry.ParticlePrefab;

            if (particlePrefab != null)
                _particleSpawner.Spawn(particlePrefab, hit.point, hit.normal);

            if (entry.DecalPrefab != null)
                _decalSpawner.Spawn(entry.DecalPrefab, hit.point, hit.normal);

            _soundPlayer.Play(entry, hit.point);
        }

        public void SpawnMagazineDrop(WeaponView view, GameObject magazinePrefab)
        {
            _magazineDropSpawner.Spawn(view, magazinePrefab);
        }
    }
}