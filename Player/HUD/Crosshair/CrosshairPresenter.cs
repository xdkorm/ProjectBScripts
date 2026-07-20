using System;
using VContainer.Unity;
using UnityEngine;
using ZigdarkS.ProjectB.Weapon.Inventory;

namespace ZigdarkS.ProjectB.Player.HUD.Crosshair
{
    public class CrosshairPresenter : IStartable, ITickable, IDisposable
    {
        private readonly CrosshairModel _model;
        private readonly CrosshairView _view;
        private readonly CrosshairConfig _config;
        private readonly IWeaponInventory _inventory;

        public CrosshairPresenter(
            CrosshairModel model,
            CrosshairView view,
            CrosshairConfig config,
            IWeaponInventory inventory)
        {
            _model = model;
            _view = view;
            _config = config;
            _inventory = inventory;
        }

        public void Start()
        {
            _model.OnSettingsChanged += Redraw;
            Redraw();
        }

        public void Tick()
        {
            var activeWeapon = _inventory.ActiveWeapon;
            if (activeWeapon == null)
            {
                _view.SetVisibility(false);
                return;
            }

            // ИСПРАВЛЕНИЕ 1: Передаем стиль прицела из конфига текущего оружия в модель.
            // Благодаря guard-clause внутри SetStyle, ивент перерисовки не будет спамиться каждый кадр.
            _model.SetStyle(activeWeapon.Config.CrosshairStyle);

            // Используем единый источник расчёта разброса из оружия.
            // Благодаря этому и стрельба, и прицел получают один и тот же ADS-переход.
            float totalAbsoluteSpread = activeWeapon.GetEffectiveSpread(activeWeapon.AdsProgress);

            // 4. КОСМЕТИКА И ОТРИСОВКА
            _view.SetVisibility(_model.IsVisible);
            if (!_model.IsVisible) return;

            // Альфа-затухание в ADS
            float alphaProgress = Mathf.InverseLerp(0.2f, 0.8f, _model.AdsProgress);
            Color fadedColor = _model.Color;
            fadedColor.a = 1f - alphaProgress;

            // ПЕРЕВОД В ПИКСЕЛИ
            float dynamicGap = totalAbsoluteSpread * _config.SpreadToGapMultiplier;

            // ИСПРАВЛЕНИЕ 3: Добавили 6-й параметр (_model.CurrentStyle) в ApplyVisuals
            _view.ApplyVisuals(_model.Thickness, _model.Length, dynamicGap, fadedColor, _model.HasDot, _model.CurrentStyle);
        }

        private void Redraw()
        {
            _view.SetVisibility(_model.IsVisible);
            if (!_model.IsVisible) return;
            
            // ИСПРАВЛЕНИЕ 4: Добавили в статичную перерисовку 6-й параметр стиля
            _view.ApplyVisuals(_model.Thickness, _model.Length, _model.CurrentGap, _model.Color, _model.HasDot, _model.CurrentStyle);
        }

        public void Dispose()
        {
            _model.OnSettingsChanged -= Redraw;
        }
    }
}