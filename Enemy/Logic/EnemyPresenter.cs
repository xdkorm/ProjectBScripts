using System;
using UnityEngine;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Core.Audio;
using ZigdarkS.ProjectB.Core.Combat;
using ZigdarkS.ProjectB.Enemy.Data;
using ZigdarkS.ProjectB.Enemy.Logic.Combat;
using ZigdarkS.ProjectB.Enemy.Logic.Navigation;
using ZigdarkS.ProjectB.Enemy.View;

namespace ZigdarkS.ProjectB.Enemy.Logic
{
    public class EnemyPresenter : IDisposable
    {
        public delegate EnemyPresenter Factory(EnemyView view, EnemyModel model);

        private readonly EnemyView  _view;
        private readonly EnemyModel _model;
        private readonly EnemyBrain _brain;
        private readonly EnemyLimbService _limbService;

        public bool IsDead => _model.IsDead;

        public EnemyPresenter(
            EnemyView                       view,
            EnemyModel                      model,
            EnemyConfig                     config,
            InstanceProvider<ITargetable>    playerProvider,
            EnemyStateFactory               stateFactory,
            EnemyNavigator                  navigator,
            EnemyCombatService              combat,
            EnemyLimbService                limbService,
            ISoundEventBus                  soundBus,
            EnemySquadRegistry              squadRegistry)
        {
            _view  = view;
            _model = model;
            _limbService = limbService;

            foreach (var limb in view.GetComponentsInChildren<EnemyLimb>())
            {
                _limbService.Register(limb);
            }
            _view.Initialize();

            _brain = new EnemyBrain(
                view, model, config, playerProvider,
                stateFactory, navigator, combat,
                soundBus, squadRegistry);
        }

        public void Tick()
        {
            if (IsDead) return;
            _brain.Tick();
        }

        public void Dispose()
        {
            if (_view != null)
            {
                _view.HideLaser();
            }

            _brain.Dispose();
        }
    }
}