using UnityEngine;
using ZigdarkS.ProjectB.Enemy.Data;

namespace ZigdarkS.ProjectB.Enemy.Logic.Combat
{
    /// <summary>
    /// Определяет с какой стороны относительно врага прилетел смертельный удар.
    /// Считается в момент смерти (DeadState.Enter), от направления полёта последнего хита.
    /// </summary>
    public static class DeathDirectionCalculator
    {
        public static DeathDirection Calculate(Vector3 victimForward, Vector3 victimRight, Vector3 hitDirection)
        {
            if (hitDirection.sqrMagnitude < 0.0001f)
                return DeathDirection.Front; // хит неизвестен (например, урон от скрипта без направления) — дефолт

            // hitDirection — это направление ПОЛЁТА удара (от источника к жертве).
            // Чтобы понять, ОТКУДА пришёл удар относительно жертвы, разворачиваем вектор.
            Vector3 cameFrom = -hitDirection;
            cameFrom.y = 0f;
            cameFrom.Normalize();

            Vector3 flatForward = victimForward; flatForward.y = 0f; flatForward.Normalize();
            Vector3 flatRight   = victimRight;   flatRight.y   = 0f; flatRight.Normalize();

            float dotForward = Vector3.Dot(cameFrom, flatForward);
            float dotRight   = Vector3.Dot(cameFrom, flatRight);

            if (Mathf.Abs(dotForward) >= Mathf.Abs(dotRight))
                return dotForward >= 0f ? DeathDirection.Front : DeathDirection.Back;

            return dotRight >= 0f ? DeathDirection.Right : DeathDirection.Left;
        }
    }
}