// Data/ReloadMode.cs
using System;
using UnityEngine;

namespace ZigdarkS.ProjectB.Weapon.Data
{
    [Serializable]
    public abstract class ReloadMode
    {
        [SerializeField] private string _name = "Reload";
        public string Name => _name;

        /// <summary>Длительность до первого шага (для магазинной — вся перезарядка одним шагом).</summary>
        public abstract float GetStepDuration(AmmoSettings settings, bool isFirstStep);

        public virtual float GetExitDuration() => 0f;

        /// <summary>
        /// Применяет один шаг зарядки. Возвращает true, если нужен ещё один шаг (магазин не полон и есть резерв).
        /// </summary>
        public abstract bool ApplyStep(Logic.AmmoState state, AmmoSettings settings);
    }

    [Serializable]
    public class FullMagazineReloadMode : ReloadMode
    {
        [Min(0f)] [SerializeField] private float _duration = 2f;

        public override float GetStepDuration(AmmoSettings settings, bool isFirstStep) => _duration;

        public override bool ApplyStep(Logic.AmmoState state, AmmoSettings settings)
        {
            state.FillFromReserve(settings.MagazineSize, int.MaxValue);
            return false;
        }
    }

    [Serializable]
    public class SequentialReloadMode : ReloadMode
    {
        [Min(1)] [SerializeField] private int _shellsPerStep = 1;
        [Min(0f)] [SerializeField] private float _firstStepDelay = 0.5f;
        [Min(0f)] [SerializeField] private float _stepInterval = 0.4f;
        [Min(0f)] [SerializeField] private float _exitDelay = 0.25f;

        public override float GetStepDuration(AmmoSettings settings, bool isFirstStep)
            => isFirstStep ? _firstStepDelay : _stepInterval;

        public override float GetExitDuration() => _exitDelay;

        public override bool ApplyStep(Logic.AmmoState state, AmmoSettings settings)
        {
            // Сценарий А: всё пусто — первый патрон идёт прямо в патронник
            if (settings.UsesChamberSlot
                && settings.ChamberFillPolicy == ChamberFillPolicy.ChamberFirst
                && state.InChamber == 0 && state.InTube == 0 && state.InReserve > 0)
            {
                state.LoadDirectlyToChamber();
                return state.InTube < settings.MagazineSize && state.InReserve > 0;
            }

            // Обычное заполнение трубы (работает для обоих сценариев)
            state.FillFromReserve(settings.MagazineSize, _shellsPerStep);
            return state.InTube < settings.MagazineSize && state.InReserve > 0;
        }
    }
}