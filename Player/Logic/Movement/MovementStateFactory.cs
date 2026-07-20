using System;
using VContainer;
using ZigdarkS.ProjectB.Player.Logic.Movement.States;
using ZigdarkS.ProjectB.Player.Data;

namespace ZigdarkS.ProjectB.Player.Logic.Movement
{
    public class MovementStateFactory
    {
        private readonly IObjectResolver _container;

        public MovementStateFactory(IObjectResolver container)
        {
            _container = container;
        }

        public IMovementState Create(MovementState stateType)
        {
            return stateType switch
            {
                MovementState.Idle => _container.Resolve<IdleState>(),
                MovementState.Walking => _container.Resolve<WalkingState>(),
                MovementState.Sprinting => _container.Resolve<SprintingState>(),
                MovementState.Airborne => _container.Resolve<AirborneState>(),
                MovementState.Crouching => _container.Resolve<CrouchingState>(),
                MovementState.Sliding => _container.Resolve<SlidingState>(),
                MovementState.AirborneCrouch => _container.Resolve<AirborneCrouchState>(),
                MovementState.Vaulting => _container.Resolve<VaultingState>(),
                MovementState.Mantling => _container.Resolve<MantlingState>(),
                _ => throw new ArgumentOutOfRangeException(nameof(stateType), stateType, null)
            };
        }
    }
}