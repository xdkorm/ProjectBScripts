using UnityEngine;
using System;

namespace ZigdarkS.ProjectB.Enemy.View
{
    public interface IEnemyView
    {
        bool IsRagdollActive { get; }
        Vector3 Position { get; }
        Vector3 EyePosition { get; }
        Vector3 Forward { get; }
        Vector3 Right { get; }
        
        public event Action<float, Vector3> OnDamaged;

        void Initialize();
        void EnableRagdoll(bool enable);
        void SetDestination(Vector3 target);
        void SetSpeed(float speed);
        void SetStopped(bool isStopped);
        void RotateTowards(Vector3 targetPosition);
        void ShowLaser(Vector3 start, Vector3 end);
        void HideLaser();

        // ��������� ������ ��� ��������� (��� ��� _animator � ���� private)
        void PlayBoolAnimation(string paramName, bool value);
        void PlayTriggerAnimation(string paramName);
    }
}