using UnityEngine;

namespace ZigdarkS.ProjectB.Player.View
{
    public class PlayerContainer : MonoBehaviour
    {
        [SerializeField] private PlayerView _playerView;
        public PlayerView PlayerView => _playerView;
    }
}