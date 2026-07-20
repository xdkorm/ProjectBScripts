using System;
using UnityEngine;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Enemy.Data;
using ZigdarkS.ProjectB.Enemy.View;
using ZigdarkS.ProjectB.Enemy.Logic.Perception;
using ZigdarkS.ProjectB.Enemy.Logic.Navigation;
using ZigdarkS.ProjectB.Enemy.Logic.Combat;

namespace ZigdarkS.ProjectB.Enemy.Logic
{
    public class EnemyContext
    {
        // ── Core ──────────────────────────────────────────────
        public EnemyView              View            { get; }
        public EnemyModel             Model           { get; }
        public EnemyConfig            Config          { get; }
        public EnemyPerception        Perception      { get; }
        public EnemyNavigator         Navigator       { get; }
        public EnemyCombatService     Combat          { get; }
        public InstanceProvider<ITargetable>         PlayerProvider  { get; }

        // ── Профиль и системы (могут быть null) ───────────────
        /// <summary>Настройки архетипа с префаба. null = всё включено.</summary>
        public EnemyBehaviourProfile  Profile         { get; }

        /// <summary>Слух врага. null если UseHearing = false в профиле.</summary>
        public EnemySoundPerception   SoundPerception { get; }

        /// <summary>Отряд, в котором состоит враг. null если нет EnemySquadMember.</summary>
        public EnemySquad             Squad           { get; set; }

        // ── Состояние AI (изменяется состояниями) ─────────────
        public SquadRole              SquadRole       { get; set; } = SquadRole.None;
        public ThreatLevel            CurrentThreat   { get; set; } = ThreatLevel.Normal;

        /// <summary>
        /// Проверяет, зарегистрировано ли состояние в машине состояний.
        /// Устанавливается из EnemyBrain после создания EnemyStateMachine.
        /// Используй перед любым переходом в опциональное состояние.
        /// </summary>
        public Func<EnemyAIState, bool> IsStateAvailable { get; set; } = _ => true;

        // ── Таймеры и данные боя ──────────────────────────────
        public float    NextFireTime      { get; set; }
        public float    StrafeTimer       { get; set; }
        public float    ReloadTimer       { get; set; }
        public float    LaserHideTimer    { get; set; }
        public bool     IsLaserActive     { get; set; }
        public Vector3  StrafeDirection   { get; set; }
        public Vector3? CoverDestination  { get; set; }
        public float    SearchElapsed     { get; set; }

        // ── Удобный доступ к игроку ───────────────────────────
        public ITargetable Player => PlayerProvider.Instance;

        public EnemyContext(
            EnemyView             view,
            EnemyModel            model,
            EnemyConfig           config,
            EnemyPerception       perception,
            EnemyNavigator        navigator,
            EnemyCombatService    combat,
            InstanceProvider<ITargetable>        playerProvider,
            EnemyBehaviourProfile profile        = null,
            EnemySoundPerception  soundPerception = null)
        {
            View            = view;
            Model           = model;
            Config          = config;
            Perception      = perception;
            Navigator       = navigator;
            Combat          = combat;
            PlayerProvider  = playerProvider;
            Profile         = profile;
            SoundPerception = soundPerception;
        }
    }
}