using UnityEngine;
using ZigdarkS.ProjectB.Player.View;
using ZigdarkS.ProjectB.Player.Data;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Weapon.Inventory;

namespace ZigdarkS.ProjectB.Player.Logic.Movement.States
{
    public class WalkingState : GroundedState
    {
        public WalkingState(IInputService input, IWeaponInventory weaponInventory, VaultDetector vaultDetector) 
            : base(input, weaponInventory, vaultDetector)   
        {
        }
        
        protected override float GetMaxSpeed(PlayerConfig config) => config.Movement.WalkSpeed;

        protected override float GetWalkPenaltyModifier(MovementSystem system)
        {
            if (Time.time >= system.WalkPenaltyUntil) return 1f;

            float total = system.WalkPenaltyUntil - system.WalkPenaltyStartTime;
            float remaining = system.WalkPenaltyUntil - Time.time;
            // t: 1 в начале штрафа → 0 в конце → множитель плавно идёт к 1f
            float t = remaining / total;
            return Mathf.Lerp(1f, system.WalkPenaltyMultiplier, t);
        }

        protected override void CheckTransitions(MovementSystem system, PlayerView view, PlayerConfig config, ref MovementState currentEnum)
        {
            if (_input.IsCrouching())
            {
                currentEnum = MovementState.Crouching;
                return;
            }
            if (_input.GetMovement() == Vector2.zero)
            {
                currentEnum = MovementState.Idle;
                return;
            }
            if (IsSprintingAllowed(_input, system))
            {
                currentEnum = MovementState.Sprinting;
            }
        }
    }
}