using UnityEngine;
using ZigdarkS.ProjectB.Player.View;
using ZigdarkS.ProjectB.Player.Data;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Weapon.Inventory;

namespace ZigdarkS.ProjectB.Player.Logic.Movement.States
{
    public class IdleState : GroundedState
    {
        public IdleState(IInputService input, IWeaponInventory weaponInventory, VaultDetector vaultDetector) 
            : base(input, weaponInventory, vaultDetector)   
        {
        }

        protected override float GetMaxSpeed(PlayerConfig config) => config.Movement.WalkSpeed;

        protected override void CheckTransitions(MovementSystem system, PlayerView view, PlayerConfig config, ref MovementState currentEnum)
        {
            if (_input.IsCrouching())
            {
                currentEnum = MovementState.Crouching;
                return;
            }

            if (_input.GetMovement() != Vector2.zero)
            {
                currentEnum = IsSprintingAllowed(_input, system) ? MovementState.Sprinting : MovementState.Walking;
            }
        }
    }
}