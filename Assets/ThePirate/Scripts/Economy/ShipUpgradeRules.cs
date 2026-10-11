using System;
using System.Globalization;
using UnityEngine;

namespace ThePirate.Economy
{
    public enum ShipUpgradeKind { Hull, Drive, Batteries }

    [Serializable]
    public sealed class ShipUpgradeState
    {
        public int hullLevel, driveLevel, batteryLevel;
        public string effectsSignature;
        public bool Any => hullLevel != 0 || driveLevel != 0 || batteryLevel != 0;
        public bool Valid => hullLevel >= 0 && hullLevel <= 1 && driveLevel >= 0 && driveLevel <= 1 && batteryLevel >= 0 && batteryLevel <= 1
            && (!Any || (!string.IsNullOrEmpty(effectsSignature) && effectsSignature.Length <= 256));
        public ShipUpgradeState Copy() => new ShipUpgradeState { hullLevel = hullLevel, driveLevel = driveLevel, batteryLevel = batteryLevel, effectsSignature = effectsSignature };
    }

    public sealed class UpgradeSettings
    {
        public readonly int HullPrice, DrivePrice, BatteryPrice, HullBonus, DamageBonus;
        public readonly float SpeedMultiplier, AccelerationMultiplier, ReloadMultiplier;
        public UpgradeSettings(int hullPrice, int drivePrice, int batteryPrice, int hullBonus, int damageBonus, float speed, float acceleration, float reload)
        { HullPrice = hullPrice; DrivePrice = drivePrice; BatteryPrice = batteryPrice; HullBonus = hullBonus; DamageBonus = damageBonus; SpeedMultiplier = speed; AccelerationMultiplier = acceleration; ReloadMultiplier = reload; }
        public string Signature => HullBonus + ";" + DamageBonus + ";" + SpeedMultiplier.ToString("R", CultureInfo.InvariantCulture) + ";" + AccelerationMultiplier.ToString("R", CultureInfo.InvariantCulture) + ";" + ReloadMultiplier.ToString("R", CultureInfo.InvariantCulture);
    }

    [CreateAssetMenu(fileName = "ShipUpgradeRules", menuName = "GameWork/Economy/Ship Upgrade Rules")]
    public sealed class ShipUpgradeRules : ScriptableObject
    {
        [SerializeField, Min(1)] private int hullPrice = 100, drivePrice = 150, batteryPrice = 200;
        [SerializeField, Min(1)] private int hullBonus = 100, damageBonus = 5;
        [SerializeField, Range(1.01f, 2f)] private float speedMultiplier = 1.1f, accelerationMultiplier = 1.15f;
        [SerializeField, Range(0.1f, 0.99f)] private float reloadMultiplier = 0.9f;

        public bool TrySnapshot(out UpgradeSettings settings, out string error)
        {
            settings = null; error = null;
            if (hullPrice < 1 || drivePrice < 1 || batteryPrice < 1 || hullBonus < 1 || damageBonus < 1
                || !Between(speedMultiplier, 1.01f, 2f) || !Between(accelerationMultiplier, 1.01f, 2f) || !Between(reloadMultiplier, 0.1f, 0.99f))
            { error = "Nieprawidłowe ceny lub parametry ShipUpgradeRules."; return false; }
            settings = new UpgradeSettings(hullPrice, drivePrice, batteryPrice, hullBonus, damageBonus, speedMultiplier, accelerationMultiplier, reloadMultiplier);
            return true;
        }
        private static bool Between(float value, float min, float max) => !float.IsNaN(value) && !float.IsInfinity(value) && value >= min && value <= max;
    }
}
