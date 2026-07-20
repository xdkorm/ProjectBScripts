using UnityEngine;
using ZigdarkS.ProjectB.Player.Data;
namespace ZigdarkS.ProjectB.Player.Data
{
    public class PlayerSettings
    {
        private const string FovKey = "Player_FOV";
        private const string SensitivityKey = "Player_Sensitivity";
        private const string AdsSensitivityMultiplierKey = "Player_AdsSensitivityMultiplier";

        private float _fov;
        private float _mouseSensitivity;
        private float _adsSensitivityMultiplier;

        public PlayerSettings(PlayerConfig config)
        {
            _fov = PlayerPrefs.GetFloat(FovKey, config.DefaulPrefs.PlayerFov);
            _mouseSensitivity = PlayerPrefs.GetFloat(SensitivityKey, config.DefaulPrefs.Sensitivity);
            _adsSensitivityMultiplier = PlayerPrefs.GetFloat(AdsSensitivityMultiplierKey, 1f);
        }

        public float Fov
        {
            get => _fov;
            set { _fov = value; PlayerPrefs.SetFloat(FovKey, value); }
        }

        public float MouseSensitivity
        {
            get => _mouseSensitivity;
            set { _mouseSensitivity = value; PlayerPrefs.SetFloat(SensitivityKey, value); }
        }

        public float AdsSensitivityMultiplier
        {
            get => _adsSensitivityMultiplier;
            set { _adsSensitivityMultiplier = value; PlayerPrefs.SetFloat(AdsSensitivityMultiplierKey, value); }
        }
    }
}