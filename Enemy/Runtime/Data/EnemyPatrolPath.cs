using UnityEngine;

namespace ZigdarkS.ProjectB.Enemy.Logic.Navigation
{
    /// <summary>
    /// Добавь на префаб врага. Задай точки патрулирования в инспекторе.
    /// Точки видны в Scene View как жёлтые сферы и линии между ними.
    ///
    /// КАК НАСТРОИТЬ:
    /// 1. Создай пустые GameObject-ы на сцене в нужных местах (назови WP_01, WP_02 и т.д.)
    /// 2. Добавь их в массив Waypoints
    /// 3. Выбери режим Loop или Ping-Pong
    /// </summary>
    public class EnemyPatrolPath : MonoBehaviour
    {
        [Tooltip("Точки маршрута. Создай пустые GameObject-ы на сцене и перетащи сюда.")]
        [SerializeField] private Transform[] _waypoints;

        [Tooltip("true = зациклить A→B→C→A | false = пинг-понг A→B→C→B→A")]
        [SerializeField] private bool _loop = true;

        [Tooltip("Время ожидания на каждой точке (секунды)")]
        [SerializeField] [Range(0f, 10f)] private float _waitTime = 1.5f;

        public bool   HasPath      => _waypoints != null && _waypoints.Length > 1;
        public float  WaitTime     => _waitTime;
        public bool   Loop         => _loop;
        public int    WaypointCount => _waypoints?.Length ?? 0;

        public Vector3 GetWaypoint(int index)
        {
            if (_waypoints == null || index < 0 || index >= _waypoints.Length) return transform.position;
            return _waypoints[index] != null ? _waypoints[index].position : transform.position;
        }

        /// <summary>Возвращает следующий индекс с учётом режима и обновляет направление для пинг-понга.</summary>
        public int GetNextIndex(int current, ref bool forward)
        {
            if (_waypoints == null || _waypoints.Length == 0) return 0;

            if (_loop)
                return (current + 1) % _waypoints.Length;

            if (forward)
            {
                if (current + 1 >= _waypoints.Length) { forward = false; return Mathf.Max(0, current - 1); }
                return current + 1;
            }
            else
            {
                if (current - 1 < 0) { forward = true; return Mathf.Min(_waypoints.Length - 1, current + 1); }
                return current - 1;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_waypoints == null || _waypoints.Length == 0) return;
            Gizmos.color = Color.yellow;
            for (int i = 0; i < _waypoints.Length; i++)
            {
                if (_waypoints[i] == null) continue;
                Gizmos.DrawSphere(_waypoints[i].position, 0.3f);
                UnityEditor.Handles.Label(_waypoints[i].position + Vector3.up * 0.5f, $"WP {i}");

                int next = _loop
                    ? (i + 1) % _waypoints.Length
                    : Mathf.Min(i + 1, _waypoints.Length - 1);
                if (next != i && _waypoints[next] != null)
                    Gizmos.DrawLine(_waypoints[i].position, _waypoints[next].position);
            }
        }
#endif
    }
}