using UnityEngine;

namespace ZigdarkS.ProjectB.Core
{
    public interface IInputService
    {
        void UpdateInput();
        Vector2 GetMovement();
        Vector2 GetMouseLook();
        bool IsAttacking();
        bool IsAiming();
        bool IsJumping();
        bool IsSprinting();
        bool IsCrouching();
        bool IsSliding();
        bool IsSwitchingFireModes();
        bool IsSafetyPressed();
        int GetSelectedSlotIndex();
        bool IsReloading();
        bool IsCyclingAction();
        bool IsVaulting();
    }
}