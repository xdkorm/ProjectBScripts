using UnityEngine;
using ZigdarkS.ProjectB.Enemy.Data;

namespace ZigdarkS.ProjectB.Enemy.Logic.States
{
    public class CombatState : IEnemyState
    {
        private float _flankCooldown;

        public void Enter(EnemyContext context)
        {
            context.CoverDestination = null;
            _flankCooldown = Random.Range(2f, 5f);
        }

        public void Update(EnemyContext context, ref EnemyAIState nextState)
        {
            if (context.Player == null)
            {
                nextState = EnemyAIState.Idle;
                return;
            }

            // ── Групповая координация ─────────────────────────────────────────
            if (context.Squad != null)
            {
                context.Squad.TryAssignRoles();

                // Назначили роль Suppressor — переходим
                if (context.SquadRole == SquadRole.Suppressor &&
                    context.IsStateAvailable(EnemyAIState.Suppress))
                {
                    nextState = EnemyAIState.Suppress;
                    return;
                }
            }

            // ── Перезарядка ───────────────────────────────────────────────────
            if (context.Model.CurrentAmmo <= 0 && !context.Model.IsReloading)
            {
                context.Combat.StartReload(context);
                nextState = context.IsStateAvailable(EnemyAIState.ReloadCover)
                    ? EnemyAIState.ReloadCover
                    : EnemyAIState.Combat; // не выходим если состояние выключено
                return;
            }

            if (context.Model.IsReloading)
            {
                if (context.IsStateAvailable(EnemyAIState.ReloadCover))
                    nextState = EnemyAIState.ReloadCover;
                return;
            }

            // ── Потеряли игрока ───────────────────────────────────────────────
            if (!context.Perception.CanSeePlayer)
            {
                if (context.Perception.TimeSinceLastSeen >= context.Config.LostSightSearchDelay)
                {
                    nextState = context.IsStateAvailable(EnemyAIState.Search)
                        ? EnemyAIState.Search
                        : EnemyAIState.Idle;
                }
                else
                {
                    nextState = context.IsStateAvailable(EnemyAIState.Chase)
                        ? EnemyAIState.Chase
                        : EnemyAIState.Idle;
                }
                return;
            }

            Vector3 playerPos = context.Player.EyesPosition;
            float   distance  = context.Perception.DistanceToPlayer;

            // ── Поведение по уровню угрозы ────────────────────────────────────
            switch (context.CurrentThreat)
            {
                case ThreatLevel.Critical:
                    // Критическое HP — только укрытие, стреляем редко
                    context.Navigator.ExecuteCoverBehavior(context, playerPos, distance);
                    context.Combat.TryShoot(context, context.Config.LongFireRate * 1.5f, playerPos);
                    return;

                case ThreatLevel.High:
                    // Высокая угроза — укрытие + обычная скорострельность
                    context.Navigator.ExecuteCoverBehavior(context, playerPos, distance);
                    context.Combat.TryShoot(context, context.Config.LongFireRate, playerPos);
                    return;

                case ThreatLevel.Normal:
                default:
                    // Нормально — агрессивный бой с флангированием
                    _flankCooldown -= Time.deltaTime;
                    if (_flankCooldown <= 0f
                        && distance >= context.Config.MidRangeMin
                        && distance <= context.Config.MidRangeMax)
                    {
                        context.Navigator.ExecuteFlank(context, playerPos);
                        context.Navigator.GenerateNewStrafeDirection(context);
                        _flankCooldown = Random.Range(4f, 8f);
                    }
                    else
                    {
                        context.Navigator.ExecuteCombatMovement(context, distance, playerPos);
                    }
                    break;
            }

            float fireRate = context.Combat.GetFireRateForDistance(context, distance);
            context.Combat.TryShoot(context, fireRate, playerPos);
        }

        public void Exit(EnemyContext context) { }
    }
}