using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;
using ZigdarkS.ProjectB.Core;

namespace ZigdarkS.ProjectB.Service.Input
{
    public class InputService : IInputService, IInitializable, IDisposable
    {
        private readonly GameInput _controls;
        public GameInput Controls => _controls;

        // Переменные для состояний (уже были у тебя)
        private bool _isCrouchToggled;
        private bool _sprintHasPriority;
        
        // --- НОВЫЕ ПЕРЕМЕННЫЕ ДЛЯ КЭШИРОВАНИЯ ТРИГГЕРОВ ---
        private int _cachedSelectedSlotIndex = -1;
        private bool _cachedJumpPressed;
        private bool _cachedFireModeSwitchPressed;

        public InputService()
        {
            _controls = new GameInput();
            LoadRebinds();
        }

        /// <summary>
        /// Этот метод вызывается ОДИН раз в кадр (например, из центрального Game Loop / Tick).
        /// Здесь мы собираем СЛПШОТ ВСЕГО ИНПУТА на текущий кадр.
        /// </summary>
        public void UpdateInput()
        {
            // 1. Сбор разовых триггеров (Ивентов)
            UpdateTriggers();

            // 2. Твоя логика стейтов приседа/спринта
            if (_controls.Player.CrouchToggle.WasPressedThisFrame())
            {
                _isCrouchToggled = !_isCrouchToggled;
                _sprintHasPriority = false;
            }

            if (_controls.Player.Crouch.WasPressedThisFrame())
            {
                _isCrouchToggled = false;
                _sprintHasPriority = false;
            }

            if (_controls.Player.Sprint.WasPressedThisFrame())
            {
                _isCrouchToggled = false;
                _sprintHasPriority = true;
            }

            // Используем уже закэшированный прыжок, чтобы не опрашивать экшен дважды
            if (_cachedJumpPressed)
            {
                _isCrouchToggled = false;
                _sprintHasPriority = true;
            }
        }

        private void UpdateTriggers()
        {
            // Кэшируем прыжок и смену режима
            _cachedJumpPressed = _controls.Player.Jump.WasPressedThisFrame();
            _cachedFireModeSwitchPressed = _controls.Player.WeaponFireModeLoop.WasPressedThisFrame();

            // Кэшируем выбор слота оружия
            _cachedSelectedSlotIndex = -1;
            if (_controls.Player.WeaponSlot1.WasPressedThisFrame()) _cachedSelectedSlotIndex = 0;
            else if (_controls.Player.WeaponSlot2.WasPressedThisFrame()) _cachedSelectedSlotIndex = 1;
            else if (_controls.Player.WeaponSlot3.WasPressedThisFrame()) _cachedSelectedSlotIndex = 2;
            else if (_controls.Player.WeaponSlot4.WasPressedThisFrame()) _cachedSelectedSlotIndex = 3;
            else if (_controls.Player.WeaponSlot5.WasPressedThisFrame()) _cachedSelectedSlotIndex = 4;
        }

        // --- ГЕТТЕРЫ ТЕПЕРЬ ПРОСТО ОТДАЮТ ГОТОВЫЕ ЗНАЧЕНИЯ ---

        public int GetSelectedSlotIndex() => _cachedSelectedSlotIndex;
        public bool IsJumping() => _cachedJumpPressed;
        public bool WasFireModeSwitchPressed() => _cachedFireModeSwitchPressed;
        public bool IsCyclingAction() => _controls.Player.WeaponCycleAction.WasPressedThisFrame();

        // Непрерывные значения (опросы удерживания кнопок) остаются без изменений, 
        // так как их можно читать в любой момент кадра без потери нажатия.
        public Vector2 GetMovement() => _controls.Player.Move.ReadValue<Vector2>();
        public Vector2 GetMouseLook() => _controls.Player.Look.ReadValue<Vector2>();
        public bool IsAttacking() => _controls.Player.Attack.IsPressed();
        public bool IsAiming() => _controls.Player.ADS.IsPressed();
        public bool IsSliding() => _controls.Player.Slide.IsPressed();
        public bool IsSwitchingFireModes() => _controls.Player.WeaponFireModeLoop.IsPressed();
        public bool IsSafetyPressed() => _controls.Player.WeaponSwitchFireModeToSafety.IsPressed();
        public bool IsVaulting() => _controls.Player.Vault.IsPressed();

        public bool IsSprinting()
        {
            if (!_controls.Player.Sprint.IsPressed()) return false;
            if (!_sprintHasPriority && IsCrouching()) return false;
            return true;
        }

        public bool IsCrouching()
        {
            bool crouchActive = _controls.Player.Crouch.IsPressed() || _isCrouchToggled;
            if (!crouchActive) return false;
            if (_sprintHasPriority && _controls.Player.Sprint.IsPressed()) return false;
            return true;
        }

        public bool IsReloading() => _controls.Player.Reload.IsPressed();

        public void Initialize() => _controls.Enable();
        public void Dispose()
        {
            _controls.Disable();
            _controls.Dispose();
        }

        public void SaveRebinds()
        {
            string json = _controls.asset.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("InputRebinds", json);
            PlayerPrefs.Save();
        }

        private void LoadRebinds()
        {
            if (PlayerPrefs.HasKey("InputRebinds"))
            {
                string json = PlayerPrefs.GetString("InputRebinds");
                _controls.asset.LoadBindingOverridesFromJson(json);
            }
        }
    }
}