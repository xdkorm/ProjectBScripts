using UnityEngine;
using UnityEngine.AI;
using ZigdarkS.ProjectB.Enemy.Data;

namespace ZigdarkS.ProjectB.Enemy.Logic.Navigation
{
    public class EnemyNavigator
    {
        public void MoveTo(EnemyContext context, Vector3 destination, float speed)
        {
            context.View.SetStopped(false);
            context.View.SetSpeed(speed);
            context.View.SetDestination(destination);
        }

        public void ExecuteStrafe(EnemyContext context, float interval, float speed, Vector3 targetDestination)
        {
            context.View.SetStopped(false);
            context.View.SetSpeed(speed);

            context.StrafeTimer += Time.deltaTime;
            if (context.StrafeTimer >= interval)
            {
                GenerateNewStrafeDirection(context);
                context.StrafeTimer = 0f;
            }

            context.View.SetDestination(targetDestination);
        }

        public void ExecuteCoverBehavior(EnemyContext context, Vector3 playerPos, float currentDistance)
        {
            var config = context.Config;
            context.View.SetStopped(false);
            context.View.SetSpeed(config.StrafeSpeed * 1.1f);

            // Ищем лучшую точку укрытия (с умным скорингом)
            if (context.CoverDestination == null)
                context.CoverDestination = FindBestCoverPoint(context, playerPos);

            if (context.CoverDestination != null)
            {
                context.View.SetDestination(context.CoverDestination.Value);
                return;
            }

            // Фолбэк: стрейф
            Vector3 normalizedDir = GetFlatDirectionTo(context.View.Position, playerPos);
            Vector3 tangent       = GetStrafeTangent(normalizedDir, context.StrafeDirection);

            Vector3 fallback;
            if (currentDistance < config.MidRangeMin)
                fallback = context.View.Position - normalizedDir * 3f + tangent * 2f;
            else if (currentDistance > config.MidRangeMax)
                fallback = context.View.Position + normalizedDir * 3f + tangent * 2f;
            else
                fallback = context.View.Position + tangent * 3f;

            context.View.SetDestination(fallback);
        }

        public void ExecuteCombatMovement(EnemyContext context, float distance, Vector3 playerPos)
        {
            var     config        = context.Config;
            Vector3 normalizedDir = GetFlatDirectionTo(context.View.Position, playerPos);
            Vector3 tangent       = GetStrafeTangent(normalizedDir, context.StrafeDirection);

            if (distance <= config.CloseRange)
            {
                Vector3 retreatTarget = context.View.Position - normalizedDir * 5f + tangent * 2f;
                ExecuteStrafe(context, 0.25f, config.CloseRetreatSpeed, retreatTarget);
            }
            else if (distance < config.MidRangeMin)
            {
                Vector3 backOffTarget = context.View.Position - normalizedDir * 3f + tangent * 3f;
                ExecuteStrafe(context, 0.5f, config.StrafeSpeed, backOffTarget);
            }
            else if (distance > config.MidRangeMax)
            {
                Vector3 approachTarget = context.View.Position + normalizedDir * 4f + tangent * 2f;
                ExecuteStrafe(context, 0.8f, config.StrafeSpeed, approachTarget);
            }
            else
            {
                Vector3 pureStrafeTarget = context.View.Position + tangent * 4f;
                ExecuteStrafe(context, 1.0f, config.StrafeSpeed, pureStrafeTarget);
            }
        }

        public void ExecuteFlank(EnemyContext context, Vector3 playerPos)
        {
            var     config        = context.Config;
            Vector3 normalizedDir = GetFlatDirectionTo(context.View.Position, playerPos);
            float   flipAngle     = context.StrafeDirection.x > 0 ? 75f : -75f;
            Vector3 flankDir      = Quaternion.Euler(0f, flipAngle, 0f) * normalizedDir;
            Vector3 flankTarget   = context.View.Position + flankDir * 5f;
            MoveTo(context, flankTarget, config.StrafeSpeed);
        }

        public void GenerateNewStrafeDirection(EnemyContext context)
        {
            context.StrafeDirection = Random.Range(0, 2) == 0 ? Vector3.right : Vector3.left;
        }

        // ── Умный поиск укрытия со скорингом ─────────────────────────────────

        /// <summary>
        /// Проверяет 8 точек вокруг врага, оценивает каждую по качеству
        /// и возвращает лучшую. Раньше возвращалась первая попавшаяся.
        /// </summary>
        private Vector3? FindBestCoverPoint(EnemyContext context, Vector3 playerPos)
        {
            var     config    = context.Config;
            Vector3 bestPoint = default;
            float   bestScore = float.MinValue;
            bool    found     = false;

            for (int i = 0; i < 8; i++)
            {
                float   angle    = i * 45f * Mathf.Deg2Rad;
                Vector3 testDir  = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                Vector3 testPoint = context.View.Position + testDir * config.CoverSearchRadius;

                if (!NavMesh.SamplePosition(testPoint, out NavMeshHit navHit,
                        config.CoverNavSampleRadius, NavMesh.AllAreas))
                    continue;

                Vector3 rayOrigin = navHit.position + Vector3.up * config.EyeHeight;
                Vector3 rayDir    = (playerPos - navHit.position).normalized;

                // Точка укрытия только если луч к игроку во что-то упирается (не попадает к нему напрямую)
                if (!Physics.Raycast(rayOrigin, rayDir, out RaycastHit hit, config.CoverRaycastRange))
                    continue;

                // Луч прошёл насквозь без препятствия — не укрытие
                // Если попал в игрока — не укрытие
                if (hit.transform != null && hit.transform.CompareTag("Player"))
                    continue;

                // ── Скоринг ──────────────────────────────────────────────
                float distToEnemy  = Vector3.Distance(context.View.Position, navHit.position);
                float distToPlayer = Vector3.Distance(navHit.position, playerPos);

                float score = 0f;
                // Предпочитаем ближайшие укрытия (меньше пути под огнём)
                score -= distToEnemy * 1.5f;
                // Предпочитаем умеренное расстояние от игрока (не слишком близко)
                score += Mathf.Clamp(distToPlayer / 8f, 0f, 3f);
                // Бонус если точка перпендикулярна линии огня (фланговое укрытие)
                Vector3 coverDir   = (navHit.position - playerPos).normalized;
                Vector3 enemyDir   = (context.View.Position - playerPos).normalized;
                float   perpBonus  = 1f - Mathf.Abs(Vector3.Dot(coverDir, enemyDir));
                score += perpBonus * 2f;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestPoint = navHit.position;
                    found     = true;
                }
            }

            return found ? bestPoint : (Vector3?)null;
        }

        // ── Утилиты ───────────────────────────────────────────────────────────

        private static Vector3 GetFlatDirectionTo(Vector3 from, Vector3 to)
        {
            Vector3 dir = to - from;
            dir.y = 0f;
            return dir.normalized;
        }

        private static Vector3 GetStrafeTangent(Vector3 normalizedDir, Vector3 strafeDirection)
        {
            return Vector3.Cross(normalizedDir, Vector3.up) * (strafeDirection.x > 0 ? 1f : -1f);
        }
    }
}