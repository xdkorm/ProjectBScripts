using ZigdarkS.ProjectB.Player.View;
using ZigdarkS.ProjectB.Player.Data;

namespace ZigdarkS.ProjectB.Player.Logic.Movement.States
{
    public interface IMovementState
    {
        void Enter(PlayerView playerView, PlayerConfig playerConfig);
        void Update(MovementSystem playerMovementSystem, PlayerView playerView, PlayerConfig playerConfig, ref MovementState currentEnum);
        void ProcessMovement(MovementSystem playerMovementSystem, PlayerView playerView, PlayerConfig playerConfig);
        void Exit(PlayerView playerView);
    }
}