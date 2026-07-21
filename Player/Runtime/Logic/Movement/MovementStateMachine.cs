using System.Collections.Generic;
using UnityEngine;
using ZigdarkS.ProjectB.Player.Logic.Movement.States;
using ZigdarkS.ProjectB.Player.View;
using ZigdarkS.ProjectB.Player.Data;

namespace ZigdarkS.ProjectB.Player.Logic.Movement
{
    public class MovementStateMachine
    {
        private readonly Dictionary<MovementState, IMovementState> _states;
        private IMovementState _currentStateLogic;
        private MovementState _currentStateEnum;

        public IMovementState CurrentStateLogic => _currentStateLogic;
        public MovementState CurrentStateEnum => _currentStateEnum;

        public MovementStateMachine(Dictionary<MovementState, IMovementState> states)
        {
            _states = states;
        }

        public void Initialize(PlayerView playerView, PlayerConfig config)
        {
            _currentStateEnum = MovementState.Idle;
            _currentStateLogic = _states[_currentStateEnum];
            _currentStateLogic.Enter(playerView, config);
        }

        public void UpdateState(MovementSystem movementSystem, PlayerView playerView, PlayerConfig config)
        {
            MovementState nextStateEnum = _currentStateEnum;
            _currentStateLogic.Update(movementSystem, playerView, config, ref nextStateEnum);

            if (nextStateEnum != _currentStateEnum)
            {
                _currentStateLogic.Exit(playerView);
                _currentStateEnum = nextStateEnum;
                _currentStateLogic = _states[_currentStateEnum];
                _currentStateLogic.Enter(playerView, config);
            }
        }
    }
}