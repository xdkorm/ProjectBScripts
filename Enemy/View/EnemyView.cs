using System;
using UnityEngine;
using UnityEngine.AI;
using ZigdarkS.ProjectB.Core.Combat;
using ZigdarkS.ProjectB.Enemy.Data;

namespace ZigdarkS.ProjectB.Enemy.View
{
    public class EnemyView : MonoBehaviour, IDirectionalDamageable
    {
        [SerializeField] private NavMeshAgent  _agent;
        [SerializeField] private LineRenderer  _laserLine;

        [Header("Ragdoll & Animation")]
        [SerializeField] private Animator  _animator;
        [SerializeField] private Collider  _mainCollider;

        [Header("Ragdoll Impulse")]
        [Tooltip("Сила импульса, применяемая к костям рэгдолла при активации. Позже сюда добавим зависимость от оружия.")]
        [SerializeField] private float _ragdollImpulseForce = 8f;
        [Tooltip("Небольшой подброс вверх, чтобы тело не проваливалось плашмя в пол.")]
        [SerializeField] private float _ragdollUpKick = 0.15f;

        private EnemyAnimator    _enemyAnimator;  // наш компонент-обёртка
        private Rigidbody[]      _boneRigidbodies;
        private Collider[]       _boneColliders;
        private bool             _isRagdollActive;
        private Vector3          _lastHitDirection;

        public bool     IsRagdollActive => _isRagdollActive;
        public Vector3  Position        => transform.position;
        public Vector3  EyePosition     => transform.position + Vector3.up * 0.5f;
        public Vector3  Forward         => transform.forward;
        public Vector3  Right           => transform.right;

        public event Action<float> OnDamaged;

        // ── Инициализация ─────────────────────────────────────────────────────

        public void Initialize()
        {
            _boneRigidbodies = GetComponentsInChildren<Rigidbody>();
            _boneColliders   = GetComponentsInChildren<Collider>();
            _enemyAnimator   = GetComponentInChildren<EnemyAnimator>();

            if (_enemyAnimator != null)
                _enemyAnimator.OnRagdollActivateRequested += HandleRagdollActivateRequested;

            EnableRagdoll(false);
        }

        private void OnDestroy()
        {
            if (_enemyAnimator != null)
                _enemyAnimator.OnRagdollActivateRequested -= HandleRagdollActivateRequested;
        }

        // ── Анимации ──────────────────────────────────────────────────────────

        /// <summary>Вызывай каждый тик из EnemyBrain.</summary>
        public void TickAnimator(EnemyAIState currentState, bool isDead)
        {
            _enemyAnimator?.Tick(currentState, isDead);
        }

        /// <summary>Вызывай при каждом выстреле.</summary>
        public void OnShootAnimation()
        {
            _enemyAnimator?.TriggerShoot();
        }

        /// <summary>Вызывай при начале перезарядки.</summary>
        public void OnReloadAnimation()
        {
            _enemyAnimator?.TriggerReload();
        }

        /// <summary>
        /// Вызови из DeadState.Enter ДО того как в этом же кадре EnemyBrain выставит IsDead = true —
        /// тогда Any State → Death в аниматоре сразу попадёт в нужный под-стейт направления.
        /// </summary>
        public void SetDeathDirection(DeathDirection direction)
            => _enemyAnimator?.SetDeathDirection(direction);

        public void SetAnimFloat(string param, float value, float dampTime = 0.1f)
            => _animator?.SetFloat(param, value, dampTime, Time.deltaTime);

        public void SetAnimBool(string param, bool value)
            => _animator?.SetBool(param, value);

        public void SetAnimTrigger(string param)
            => _animator?.SetTrigger(param);

        // ── Урон ──────────────────────────────────────────────────────────────

        public void TakeDamage(float damage) => TakeDamage(damage, Vector3.zero);

        public void TakeDamage(float damage, Vector3 hitDirection)
        {
            OnDamaged?.Invoke(damage);
        }

        // ── Движение ──────────────────────────────────────────────────────────

        public void SetDestination(Vector3 target)
        {
            if (_agent != null && _agent.enabled) _agent.SetDestination(target);
        }

        public void SetSpeed(float speed)
        {
            if (_agent != null && _agent.enabled) _agent.speed = speed;
        }

        public void SetStopped(bool isStopped)
        {
            if (_agent != null && _agent.enabled) _agent.isStopped = isStopped;
        }

        // ── Поворот ───────────────────────────────────────────────────────────

        /// <summary>Мгновенный поворот к цели (без учёта Y).</summary>
        public void RotateTowards(Vector3 targetPosition)
        {
            Vector3 dir = targetPosition - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }

        /// <summary>Плавный поворот к цели. Используй в CombatState.</summary>
        public void RotateTowardsSmoothly(Vector3 targetPosition, float degreesPerSecond)
        {
            Vector3 dir = targetPosition - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) return;

            Quaternion target = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, target, degreesPerSecond * Time.deltaTime);
        }

        // ── Лазер ─────────────────────────────────────────────────────────────

        public void ShowLaser(Vector3 start, Vector3 end)
        {
            if (_laserLine == null) return;
            _laserLine.SetPosition(0, start);
            _laserLine.SetPosition(1, end);
            _laserLine.enabled = true;
        }

        public void HideLaser()
        {
            if (_laserLine != null) _laserLine.enabled = false;
        }

        // ── Рагдолл ───────────────────────────────────────────────────────────

        /// <summary>
        /// Вызывается через Animation Event (EnemyAnimator.Anim_ActivateRagdoll) на нужном кадре
        /// клипа смерти — НЕ сразу при входе в DeadState.
        /// </summary>
        private void HandleRagdollActivateRequested()
        {
            EnableRagdoll(true);
            ApplyRagdollImpulse(_lastHitDirection);
        }

        public void EnableRagdoll(bool enable)
        {
            _isRagdollActive = enable;

            if (_animator    != null) _animator.enabled     = !enable;
            if (_mainCollider != null) _mainCollider.enabled = !enable;
            if (_agent       != null) _agent.enabled        = !enable;

            if (_boneRigidbodies != null)
                foreach (var rb in _boneRigidbodies)
                {
                    if (rb.gameObject == gameObject) continue;
                    rb.isKinematic = !enable;
                }

            if (_boneColliders != null)
                foreach (var col in _boneColliders)
                {
                    if (col == _mainCollider) continue;
                    col.enabled = true;
                }
        }

        /// <summary>
        /// Импульс на кости рэгдолла от направления удара. Сила пока одна на всех —
        /// когда понадобится, замени _ragdollImpulseForce на выборку по типу оружия.
        /// </summary>
        private void ApplyRagdollImpulse(Vector3 hitDirection)
        {
            if (_boneRigidbodies == null || _boneRigidbodies.Length == 0) return;

            Vector3 direction = hitDirection.sqrMagnitude > 0.0001f ? hitDirection.normalized : Vector3.up;
            direction.y += _ragdollUpKick;
            direction.Normalize();

            foreach (var rb in _boneRigidbodies)
            {
                if (rb.gameObject == gameObject) continue;
                rb.AddForce(direction * _ragdollImpulseForce, ForceMode.Impulse);
            }
        }

        // ── Утилиты ───────────────────────────────────────────────────────────

        private void Reset()
        {
            _agent        = GetComponent<NavMeshAgent>();
            _laserLine    = GetComponent<LineRenderer>();
            _animator     = GetComponentInChildren<Animator>();
            _mainCollider = GetComponent<Collider>();
        }
    }
}