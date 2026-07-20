using UnityEngine;

namespace ZigdarkS.ProjectB.Enemy.Data
{
    /// <summary>
    /// Настройки архетипа врага, выставляются левел-дизайнером в инспекторе на префабе.
    /// View читает этот компонент и передаёт данные дальше в Model/Context —
    /// сам компонент никакой бизнес-логики не содержит (чистый источник конфигурации).
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyBehaviourProfile : MonoBehaviour
    {
        [Header("━━━ Разрешённые состояния AI ━━━")]
        [SerializeField] private bool _canPatrol      = true;
        [SerializeField] private bool _allowChase     = true;
        [SerializeField] private bool _allowSearch    = true;
        [SerializeField] private bool _canTakeCover   = true;
        [SerializeField] private bool _allowAlert     = true;
        [SerializeField] private bool _allowSuppress  = true;

        [Header("━━━ Ключевые тумблеры поведения (ТЗ) ━━━")]
        [Tooltip("Может ли враг переходить на бег (Investigate/погоня). false = всегда идёт шагом.")]
        [SerializeField] private bool _canRun = true;

        [Tooltip("Выключение всех стелс/слух/патрульных механик.")]
        [SerializeField] private bool _isArenaMode = false;

        [Header("━━━ Восприятие ━━━")]
        [SerializeField] private bool  _useHearing = true;
        [SerializeField] private bool  _useFOV     = true;
        [SerializeField] [Range(30f, 360f)] private float _fovAngle = 180f;

        [Header("━━━ Групповое поведение ━━━")]
        [SerializeField] private bool _participateInSquad = true;

        // ── Свойства ────────────────────────────────────────────────────────
        // ВАЖНО: в Arena Mode стелс/слух/патруль отключаются автоматически прямо здесь —
        // остальному коду (EnemyBrain, состояния) не нужно отдельно проверять IsArenaMode
        // для каждого из этих случаев, что снижает риск забыть учесть флаг где-то в новом месте.

        public bool  CanPatrol          => _canPatrol && !_isArenaMode;
        public bool  AllowChase         => _allowChase && !_isArenaMode;
        public bool  AllowSearch        => _allowSearch && !_isArenaMode;
        public bool  AllowAlert         => _allowAlert && !_isArenaMode;
        public bool  UseHearing         => _useHearing && !_isArenaMode;

        /// <summary>Cover — независимая механика, Arena Mode её не выключает (ТЗ этого не требует).</summary>
        public bool  CanTakeCover       => _canTakeCover;
        public bool  AllowSuppress      => _allowSuppress;

        public bool  CanRun             => _canRun;
        public bool  IsArenaMode        => _isArenaMode;

        public bool  UseFOV             => _useFOV;
        public float FOVAngle           => _fovAngle;
        public bool  ParticipateInSquad => _participateInSquad;
    }
}