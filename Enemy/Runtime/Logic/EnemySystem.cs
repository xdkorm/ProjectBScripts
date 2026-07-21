using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using ZigdarkS.ProjectB.Enemy.Data;
using ZigdarkS.ProjectB.Enemy.View;

namespace ZigdarkS.ProjectB.Enemy.Logic
{
    public class EnemySystem : IStartable, ITickable, IDisposable
    {
        private readonly EnemyPresenter.Factory _factory;
        private readonly EnemyConfig _enemyConfig;
        private readonly List<EnemyPresenter> _activePresenters = new();

        public EnemySystem(EnemyPresenter.Factory factory, EnemyConfig enemyConfig)
        {
            _factory = factory;
            _enemyConfig = enemyConfig;
        }

        public void Start()
        {
            if (_enemyConfig == null)
            {
                Debug.LogError("[EnemySystem] EnemyConfig не назначен в GameLifetimeScope!");
                return;
            }

            var sceneEnemies = UnityEngine.Object.FindObjectsByType<EnemyView>(FindObjectsInactive.Exclude);
            foreach (var enemyView in sceneEnemies)
            {
                var enemyModel = new EnemyModel(_enemyConfig);
                var presenter = _factory.Invoke(enemyView, enemyModel);
                _activePresenters.Add(presenter);
            }
        }

        public void Tick()
        {
            for (int i = _activePresenters.Count - 1; i >= 0; i--)
            {
                var presenter = _activePresenters[i];

                if (presenter.IsDead)
                {
                    presenter.Dispose();
                    _activePresenters.RemoveAt(i);
                    continue;
                }

                presenter.Tick();
            }
        }

        public void Dispose()
        {
            foreach (var presenter in _activePresenters)
            {
                presenter.Dispose();
            }

            _activePresenters.Clear();
        }
    }
}
