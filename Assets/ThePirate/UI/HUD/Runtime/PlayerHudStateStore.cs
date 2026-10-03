using System;
using System.Collections.Generic;

namespace ThePirate.UI.HUD
{
    // Osobny magazyn dla każdego gracza. Bez statycznych zdarzeń i zależności od Canvas.
    public sealed class PlayerHudStateStore
    {
        private HudDurability durability;
        private HudMovement movement;
        private HudCargo cargo;
        private HudWind wind;
        private HudAim aim;
        private HudTarget target;
        private HudObjective objective;
        private IReadOnlyList<HudBattery> batteries = Array.AsReadOnly(Array.Empty<HudBattery>());
        private IReadOnlyList<HudMapMarker> markers = Array.AsReadOnly(Array.Empty<HudMapMarker>());

        public HudDurability Durability => durability;
        public HudMovement Movement => movement;
        public HudCargo Cargo => cargo;
        public HudWind Wind => wind;
        public HudAim Aim => aim;
        public HudTarget Target => target;
        public HudObjective Objective => objective;
        public IReadOnlyList<HudBattery> Batteries => batteries;
        public IReadOnlyList<HudMapMarker> Markers => markers;

        public event Action<HudDurability> DurabilityChanged;
        public event Action<HudMovement> MovementChanged;
        public event Action<HudCargo> CargoChanged;
        public event Action<HudWind> WindChanged;
        public event Action<HudAim> AimChanged;
        public event Action<HudTarget> TargetChanged;
        public event Action<HudObjective> ObjectiveChanged;
        public event Action<IReadOnlyList<HudBattery>> BatteriesChanged;
        public event Action<IReadOnlyList<HudMapMarker>> MarkersChanged;
        // Komunikat jest zdarzeniem jednorazowym, nie stanem do ponownego odtwarzania.
        public event Action<HudMessage> MessageRaised;

        public void SetDurability(HudDurability value) => Set(ref durability, value, DurabilityChanged);
        public void SetMovement(HudMovement value) => Set(ref movement, value, MovementChanged);
        public void SetCargo(HudCargo value) => Set(ref cargo, value, CargoChanged);
        public void SetWind(HudWind value) => Set(ref wind, value, WindChanged);
        public void SetAim(HudAim value) => Set(ref aim, value, AimChanged);
        public void SetTarget(HudTarget value) => Set(ref target, value, TargetChanged);
        public void SetObjective(HudObjective value) => Set(ref objective, value, ObjectiveChanged);
        public void SetBatteries(IReadOnlyList<HudBattery> value) => SetList(ref batteries, value, BatteriesChanged);
        public void SetMarkers(IReadOnlyList<HudMapMarker> value) => SetList(ref markers, value, MarkersChanged);
        public void Publish(HudMessage message)
        { if (!string.IsNullOrWhiteSpace(message.Text)) MessageRaised?.Invoke(message); }

        private static void Set<T>(ref T field, T value, Action<T> changed) where T : struct
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            changed?.Invoke(value);
        }

        private static void SetList<T>(ref IReadOnlyList<T> field, IReadOnlyList<T> value,
            Action<IReadOnlyList<T>> changed) where T : struct
        {
            int count = value?.Count ?? 0;
            bool equal = count == field.Count;
            for (int i = 0; equal && i < count; i++)
                equal = EqualityComparer<T>.Default.Equals(field[i], value[i]);
            if (equal) return;
            var copy = new T[count];
            for (int i = 0; i < count; i++) copy[i] = value[i];
            field = Array.AsReadOnly(copy);
            changed?.Invoke(field);
        }
    }
}
