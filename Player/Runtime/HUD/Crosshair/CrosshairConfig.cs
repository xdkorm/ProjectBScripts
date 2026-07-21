using UnityEngine;

namespace ZigdarkS.ProjectB.Player.HUD.Crosshair
{
    [CreateAssetMenu(fileName = "CrosshairConfig", menuName = "ProjectB/HUD/Crosshair Config")]
    public class CrosshairConfig : ScriptableObject
    {
        [Header("Visual Settings")]
        [SerializeField] private float _thickness = 3f;
        [SerializeField] private float _length = 8f;
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private bool _hasDot = false;
        [SerializeField] private bool _isDynamic = true;

        private float _baseGap = 0f;

        [Header("UI Scaling")]
        [Tooltip("Множитель для перевода разброса оружия в пиксели зазора UI")]
        [SerializeField] private float _spreadToGapMultiplier = 500f;

        public float Thickness => _thickness;
        public float Length => _length;
        public float BaseGap => _baseGap;
        public Color Color => _color;
        public bool HasDot => _hasDot;
        public bool IsDynamic => _isDynamic;
        public float SpreadToGapMultiplier => _spreadToGapMultiplier;
    }
}