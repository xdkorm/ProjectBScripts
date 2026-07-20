using System;
using UnityEngine;

namespace ZigdarkS.ProjectB.Weapon.Data
{
    public enum ActionCycleMode
    {
        None,       // обычное авто/полуавтоматическое оружие — цикла нет вообще
        Manual,     // болтовка/помпа — игрок передёргивает сам (отдельный инпут)
        Automatic   // цикл идёт по таймеру сам (для полуавтоматов с анимацией затвора, если нужна просто задержка)
    }

    [Serializable]
    public class ActionCycleSettings
    {
        public ActionCycleMode CycleMode = ActionCycleMode.None;
        [Min(0f)] public float CycleDuration = 0.35f;
    }
}