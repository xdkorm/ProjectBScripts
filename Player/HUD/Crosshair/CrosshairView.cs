using UnityEngine;
using UnityEngine.UI;
using ZigdarkS.ProjectB.Core;

namespace ZigdarkS.ProjectB.Player.HUD.Crosshair
{
    public class CrosshairView : MonoBehaviour
    {
        [Header("Straight Lines (Plus / T)")]
        [SerializeField] private RectTransform _leftLine;
        [SerializeField] private RectTransform _rightLine;
        [SerializeField] private RectTransform _topLine;
        [SerializeField] private RectTransform _bottomLine;

        [Header("Shotgun Arcs (O)")]
        [SerializeField] private RectTransform _leftArc;
        [SerializeField] private RectTransform _rightArc;
        [SerializeField] private RectTransform _topArc;
        [SerializeField] private RectTransform _bottomArc;

        [Header("Center Dot")]
        [SerializeField] private GameObject _centerDot;

        private Image[] _allImages;

        private void Awake()
        {
            // Собираем вообще все картинки в один массив, чтобы красить одной строчкой
            _allImages = new[]
            {
                _leftLine.GetComponent<Image>(), _rightLine.GetComponent<Image>(),
                _topLine.GetComponent<Image>(), _bottomLine.GetComponent<Image>(),
                _leftArc.GetComponent<Image>(), _rightArc.GetComponent<Image>(),
                _topArc.GetComponent<Image>(), _bottomArc.GetComponent<Image>(),
                _centerDot.GetComponent<Image>()
            };
        }

        public void ApplyVisuals(
            float thickness, 
            float length, 
            float gap, 
            Color color, 
            bool hasDot,
            CrosshairStyle style)
        {
            // 1. Управление точкой по центру
            if (_centerDot.activeSelf != hasDot) _centerDot.SetActive(hasDot);

            // 2. Синхронно меняем цвет всем элементам прицела
            foreach (var img in _allImages)
            {
                if (img != null) img.color = color;
            }

            // 3. Определяем, какие именно группы элементов должны быть включены
            bool useLines = style == CrosshairStyle.PlusShape || style == CrosshairStyle.TShape || style == CrosshairStyle.TOShape;
            bool useArcs = style == CrosshairStyle.OShape || style == CrosshairStyle.TOShape;

            // Специфическое выключение верхней линии для Т-стиля
            bool showTopLine = style == CrosshairStyle.PlusShape; 

            // Включаем/выключаем геймобжекты на основе выбранного стиля
            _leftLine.gameObject.SetActive(useLines);
            _rightLine.gameObject.SetActive(useLines);
            _bottomLine.gameObject.SetActive(useLines);
            _topLine.gameObject.SetActive(useLines && showTopLine);

            _leftArc.gameObject.SetActive(useArcs);
            _rightArc.gameObject.SetActive(useArcs);
            _topArc.gameObject.SetActive(useArcs);
            _bottomArc.gameObject.SetActive(useArcs);

            // 4. Логика для обычных прямых линий (Plus / T)
            if (useLines)
            {
                _leftLine.sizeDelta = new Vector2(length, thickness);
                _rightLine.sizeDelta = new Vector2(length, thickness);
                _topLine.sizeDelta = new Vector2(thickness, length);
                _bottomLine.sizeDelta = new Vector2(thickness, length);

                _leftLine.anchoredPosition = new Vector2(-gap, 0f);
                _rightLine.anchoredPosition = new Vector2(gap, 0f);
                _topLine.anchoredPosition = new Vector2(0f, gap);
                _bottomLine.anchoredPosition = new Vector2(0f, -gap);
            }

            // 5. Логика для дуг дробовика (O-Shape)
            // Они работают точно так же, как риски: разлетаются от центра на расстояние gap!
            if (useArcs)
            {
                _leftArc.anchoredPosition = new Vector2(-gap, 0f);
                _rightArc.anchoredPosition = new Vector2(gap, 0f);
                _topArc.anchoredPosition = new Vector2(0f, gap);
                _bottomArc.anchoredPosition = new Vector2(0f, -gap);
                
                // Если захочешь, чтобы толщина/размер дуг тоже настраивались из меню,
                // можно раскомментировать строки ниже (при условии, что это UI Images со Squre-пропорциями)
                // _leftArc.sizeDelta = new Vector2(length, length);
                // и так далее...
            }
        }

        public void SetVisibility(bool isVisible)
        {
            if (gameObject.activeSelf != isVisible)
            {
                gameObject.SetActive(isVisible);
            }
        }
    }
}