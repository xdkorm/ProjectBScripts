namespace ZigdarkS.ProjectB.Core.Combat
{
    public interface IDamageable
    {
         void TakeDamage(float damage);
    }

    public interface IDirectionalDamageable : IDamageable
    {
        void TakeDamage(float damage, UnityEngine.Vector3 hitDirection);
    }
}