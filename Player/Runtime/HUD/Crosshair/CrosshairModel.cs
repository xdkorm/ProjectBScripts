using System;
using UnityEngine;
using ZigdarkS.ProjectB.Core.UI;

namespace ZigdarkS.ProjectB.Player.HUD.Crosshair
{
    public class CrosshairModel
    {
        public bool IsEnabledInSettings { get; private set; } = true;
        public float Thickness { get; set; }
        public float Length { get; set; }
        public float BaseGap { get; set; }
        public Color Color { get; set; }
        public bool HasDot { get; set; }
        public bool IsDynamic { get; set; }

        public CrosshairStyle CurrentStyle { get; private set; } = CrosshairStyle.PlusShape;

        public bool IsVisible => IsEnabledInSettings && AdsProgress < 0.8f;
        public float AdsProgress { get; private set; } = 0f;
        public float DynamicOffset { get; set; } = 0f;
        public float CurrentGap => BaseGap + (IsDynamic ? DynamicOffset : 0f);

        public event Action OnSettingsChanged;
        
        public CrosshairModel(CrosshairConfig config)
        {
            Thickness = config.Thickness;
            Length = config.Length;
            BaseGap = config.BaseGap;
            Color = config.Color;
            HasDot = config.HasDot;
            IsDynamic = config.IsDynamic;
        }

        public void SetStyle(CrosshairStyle newStyle)
        {
            if (CurrentStyle == newStyle) return;
            CurrentStyle = newStyle;
            OnSettingsChanged?.Invoke();
        }

        public void UpdateSettings(float thickness, float length, float baseGap, Color color, bool hasDot)
        {
            Thickness = thickness;
            Length = length;
            BaseGap = baseGap;
            Color = color;
            HasDot = hasDot;
            
            OnSettingsChanged?.Invoke();
        }

        public void SetEnabledFromSettings(bool isEnabled)
        {
            if (IsEnabledInSettings == isEnabled) return;
            IsEnabledInSettings = isEnabled;
            OnSettingsChanged?.Invoke();
        }

        public void UpdateAdsProgress(float progress)
        {
            if (Mathf.Approximately(AdsProgress, progress)) return;
            AdsProgress = progress;
        }
    }
}