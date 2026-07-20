using UnityEngine;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Enemy.Data;
using ZigdarkS.ProjectB.Enemy.View;

namespace ZigdarkS.ProjectB.Enemy.Logic.Perception
{
    public class EnemyPerception
    {
        public bool      CanSeePlayer             { get; private set; }
        public Vector3?  LastKnownPlayerPosition  { get; private set; }
        public float     TimeSinceLastSeen        { get; private set; }
        public float     DistanceToPlayer         { get; private set; }

        private readonly RaycastHit[] _losHits = new RaycastHit[16];

        /// <summary>
        /// Обновляет восприятие. Профиль опциональный — если null, FOV не используется.
        /// </summary>
        public void Update(EnemyView enemyView, ITargetable target, float eyeHeight,
                           EnemyBehaviourProfile profile = null)
        {
            if (target == null)
            {
                CanSeePlayer = false;
                TimeSinceLastSeen += Time.deltaTime;
                return;
            }

            Vector3 eyePos    = enemyView.Position + Vector3.up * eyeHeight;
            Vector3 playerPos = target.EyesPosition;
            DistanceToPlayer  = Vector3.Distance(enemyView.Position, playerPos);

            // ── Arena Mode (ТЗ): классический Boomer Shooter — враг ВСЕГДА точно знает,
            // где игрок. Полностью обходим FOV и рейкаст LOS. ──────────────────────────
            if (profile != null && profile.IsArenaMode)
            {
                CanSeePlayer             = true;
                LastKnownPlayerPosition  = playerPos;
                TimeSinceLastSeen        = 0f;
                return;
            }

            // ── FOV проверка ──────────────────────────────────
            bool inFOV = true;
            if (profile != null && profile.UseFOV)
            {
                Vector3 toPlayer = (playerPos - eyePos).normalized;
                float   angle    = Vector3.Angle(enemyView.Forward, toPlayer);
                inFOV = angle <= profile.FOVAngle * 0.5f;
            }

            if (inFOV && HasLineOfSight(eyePos, playerPos, target.gameObject))
            {
                CanSeePlayer             = true;
                LastKnownPlayerPosition  = playerPos;
                TimeSinceLastSeen        = 0f;
            }
            else
            {
                CanSeePlayer      = false;
                TimeSinceLastSeen += Time.deltaTime;
            }
        }

        public bool IsPlayerInAggroRange(float aggroRange)
            => DistanceToPlayer <= aggroRange;

        public void ResetSearchTimer()
            => TimeSinceLastSeen = 0f;

        // ── Приватное ────────────────────────────────────────

        private bool HasLineOfSight(Vector3 startPos, Vector3 playerPos, GameObject playerGO)
        {
            Vector3 direction = (playerPos - startPos).normalized;
            float   distance  = Vector3.Distance(startPos, playerPos);
            int     hitCount  = Physics.RaycastNonAlloc(startPos, direction, _losHits, distance);

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = _losHits[i];
                if (hit.transform.TryGetComponent<EnemyView>(out _)) continue;
                if (hit.transform.gameObject != playerGO && !hit.transform.IsChildOf(playerGO.transform))
                    return false;
            }

            return true;
        }
    }
}