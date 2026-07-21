using System;
using UnityEngine;
using UnityEngine.AI;
using ZigdarkS.ProjectB.Enemy.Data;

namespace ZigdarkS.ProjectB.Enemy.View
{
    /// <summary>
    /// Добавь на префаб врага. Обновляет Animator параметры каждый тик.
    /// Вызывай Tick() из EnemyBrain или напрямую из EnemyView.
    ///
    /// Требует в Animator Controller:
    /// Base Layer: Speed (float), VelocityX/VelocityZ (float, СЫРЫЕ м/с — не нормализуй в клипах,
    ///   позиции motion'ов в blend tree должны совпадать с реальной скоростью анимации),
    ///   IsDead (bool), DeathDirection (int).
    /// Combat Layer (Avatar Mask: Upper Body): IsInCombat (bool), Shoot/Reload (trigger).
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAnimator : MonoBehaviour
    {
        private static readonly int SpeedHash          = Animator.StringToHash("Speed");
        private static readonly int VelocityXHash      = Animator.StringToHash("VelocityX");
        private static readonly int VelocityZHash      = Animator.StringToHash("VelocityZ");
        private static readonly int IsInCombatHash     = Animator.StringToHash("IsInCombat");
        private static readonly int IsDeadHash         = Animator.StringToHash("IsDead");
        private static readonly int IsAlertHash        = Animator.StringToHash("IsAlert");
        private static readonly int ShootHash          = Animator.StringToHash("Shoot");
        private static readonly int ReloadHash         = Animator.StringToHash("Reload");
        private static readonly int DeathDirectionHash = Animator.StringToHash("DeathDirection");

        [Tooltip("Индекс Combat Layer в Animator Controller (см. вкладку Layers).")]
        [SerializeField] private int _combatLayerIndex = 1;

        [Tooltip("Скорость (м/с) изменения веса Combat Layer при входе/выходе из боя.")]
        [SerializeField] private float _combatLayerWeightSpeed = 4f;

        [Tooltip("Сглаживание изменения Speed/Velocity параметров")]
        [SerializeField] [Range(0f, 0.5f)] private float _speedDampTime = 0.1f;

        private Animator     _animator;
        private NavMeshAgent _agent;
        private float        _currentCombatLayerWeight;

        /// <summary>
        /// Дёргается через Animation Event из клипа смерти на кадре, где тело должно
        /// провиснуть в рэгдолл (НЕ в начале клипа — иначе анимация не успеет проиграться).
        /// Метод-приёмник события (Anim_ActivateRagdoll) обязан висеть на этом же
        /// компоненте, т.к. он на одном GameObject с Animator.
        /// </summary>
        public event Action OnRagdollActivateRequested;

        private void Awake()
        {
            _animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
            _agent    = GetComponentInParent<NavMeshAgent>() ?? GetComponent<NavMeshAgent>();
        }

        public void Tick(EnemyAIState currentState, bool isDead)
        {
            if (_animator == null) return;

            Vector3 localVelocity = Vector3.zero;
            float speed = 0f;

            if (_agent != null && _agent.enabled && !_agent.isStopped)
            {
                localVelocity = transform.InverseTransformDirection(_agent.velocity);
                speed = _agent.velocity.magnitude;
            }

            bool isCombat = currentState == EnemyAIState.Combat
                         || currentState == EnemyAIState.ReloadCover
                         || currentState == EnemyAIState.Suppress;

            bool isAlert = currentState == EnemyAIState.Alert;

            _animator.SetFloat(SpeedHash, speed, _speedDampTime, Time.deltaTime);
            _animator.SetFloat(VelocityXHash, localVelocity.x, _speedDampTime, Time.deltaTime);
            _animator.SetFloat(VelocityZHash, localVelocity.z, _speedDampTime, Time.deltaTime);

            _animator.SetBool(IsInCombatHash, isCombat);
            _animator.SetBool(IsAlertHash, isAlert);
            _animator.SetBool(IsDeadHash, isDead);

            UpdateCombatLayerWeight(isCombat);
        }

        private void UpdateCombatLayerWeight(bool isCombat)
        {
            if (_animator.layerCount <= _combatLayerIndex) return;

            float target = isCombat ? 1f : 0f;
            _currentCombatLayerWeight = Mathf.MoveTowards(
                _currentCombatLayerWeight, target, _combatLayerWeightSpeed * Time.deltaTime);

            _animator.SetLayerWeight(_combatLayerIndex, _currentCombatLayerWeight);
        }

        public void TriggerShoot()
        {
            _animator?.SetTrigger(ShootHash);
        }

        public void TriggerReload()
        {
            _animator?.SetTrigger(ReloadHash);
        }

        public void SetDeathDirection(DeathDirection direction)
        {
            _animator?.SetInteger(DeathDirectionHash, (int)direction);
        }

        public void Anim_ActivateRagdoll()
        {
            OnRagdollActivateRequested?.Invoke();
        }
    }
}