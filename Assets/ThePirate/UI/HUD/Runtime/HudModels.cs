using System;
using UnityEngine;

namespace ThePirate.UI.HUD
{
    public enum HudBatterySide { Left, Right, Bow, Stern, Special }
    public enum HudDriveMode { Unavailable, Stopped, Forward, Braking, Reverse }
    public enum HudMarkerKind { Player, Friendly, Enemy, Neutral }
    public enum HudMessageKind { Info, Warning, Error }

    // Snapshoty są wartościami: odbiorca nie może zmienić stanu magazynu.
    public readonly struct HudDurability
    {
        public readonly bool Available;
        public readonly int Current, Maximum;
        public float Fraction => Maximum > 0 ? (float)Current / Maximum : 0f;
        public HudDurability(int current, int maximum)
        {
            Available = maximum > 0; Maximum = Math.Max(0, maximum);
            Current = Math.Max(0, Math.Min(current, Maximum));
        }
    }

    public readonly struct HudMovement
    {
        public readonly bool Available;
        // Prędkość rzeczywista pozioma oraz zadana (ze znakiem), w jednostkach Unity/s.
        public readonly float Speed, RequestedSpeed;
        public readonly HudDriveMode Drive;
        public HudMovement(float speed, float requestedSpeed, HudDriveMode drive)
        { Available = true; Speed = speed; RequestedSpeed = requestedSpeed; Drive = drive; }
    }

    public readonly struct HudCargo
    {
        public readonly bool Available, HasCapacity;
        public readonly float Weight, Capacity;
        public HudCargo(float weight, float capacity, bool hasCapacity)
        { Available = true; Weight = Mathf.Max(0, weight); Capacity = Mathf.Max(0, capacity); HasCapacity = hasCapacity; }
    }

    public readonly struct HudWind
    {
        public readonly bool Available;
        // Wektor prędkości wiatru w świecie, NIE ExternalVelocity statku.
        public readonly Vector3 Velocity;
        public HudWind(Vector3 velocity) { Available = true; Velocity = velocity; }
    }

    public readonly struct HudBattery
    {
        public readonly ulong Id; public readonly int CannonCount;
        public readonly HudBatterySide Side;
        public readonly bool Ready, Selected, Destroyed;
        public readonly float ReloadRemaining, ReloadDuration;
        public float ReloadProgress => ReloadDuration > 0f
            ? Mathf.Clamp01(1f - ReloadRemaining / ReloadDuration) : 1f;
        public HudBattery(ulong id, HudBatterySide side, int cannonCount, bool ready,
            bool selected, bool destroyed, float remaining, float duration)
        {
            Id = id; Side = side; CannonCount = cannonCount; Ready = ready;
            Selected = selected; Destroyed = destroyed;
            ReloadRemaining = Mathf.Max(0, remaining); ReloadDuration = Mathf.Max(0, duration);
        }
    }

    public readonly struct HudAim
    {
        public readonly bool Available, InSelectedSector, CanFire;
        public readonly ulong SelectedBatteryId;
        public readonly Vector3 Point;
        public HudAim(Vector3 point, ulong selectedBatteryId, bool inSelectedSector, bool canFire)
        { Available = true; Point = point; SelectedBatteryId = selectedBatteryId; InSelectedSector = inSelectedSector; CanFire = canFire; }
    }

    public readonly struct HudTarget
    {
        public readonly bool Available, Enemy;
        public readonly ulong Id;
        public readonly string Name;
        public readonly float Distance;
        public readonly HudDurability Durability;
        public HudTarget(ulong id, string name, float distance, bool enemy, HudDurability durability)
        { Available = true; Id = id; Name = name ?? string.Empty; Distance = distance; Enemy = enemy; Durability = durability; }
    }

    public readonly struct HudMapMarker
    {
        // Id ważne tylko w bieżącej sesji, nie zapisujemy ich w save.
        public readonly ulong Id;
        public readonly string Name;
        public readonly Vector3 Position;
        public readonly float Heading;
        public readonly HudMarkerKind Kind;
        public HudMapMarker(ulong id, string name, Vector3 position, float heading, HudMarkerKind kind)
        { Id = id; Name = name ?? string.Empty; Position = position; Heading = heading; Kind = kind; }
    }

    public readonly struct HudObjective
    {
        public readonly bool Available, Completed;
        public readonly string Text;
        public HudObjective(string text, bool completed)
        { Available = !string.IsNullOrWhiteSpace(text); Text = text ?? string.Empty; Completed = completed; }
    }

    public readonly struct HudMessage
    {
        public readonly string Text;
        public readonly HudMessageKind Kind;
        public readonly float Duration;
        public HudMessage(string text, HudMessageKind kind = HudMessageKind.Info, float duration = 4f)
        { Text = text ?? string.Empty; Kind = kind; Duration = Mathf.Max(0, duration); }
    }
}
