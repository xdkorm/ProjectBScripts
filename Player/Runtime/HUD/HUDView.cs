using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ZigdarkS.ProjectB.Player.HUD
{
    public class HUDView : MonoBehaviour
    {
        [SerializeField] private Slider hpSlider;

        [Header("Ammo")]
        [SerializeField] private TMP_Text magAmmoText;
        [SerializeField] private TMP_Text reserveAmmoText;

        public void UpdateHealthBar(float hpPercent)
        {
            hpSlider.value = hpPercent;
        }

        public void UpdateAmmo(int inMagazine, int inReserve)
        {
            magAmmoText.text = $"{inMagazine}";
            reserveAmmoText.text = $"{inReserve}";
        }

        public void ClearAmmo()
        {
            magAmmoText.text = "--";
            reserveAmmoText.text = "--";
        }
    }
}