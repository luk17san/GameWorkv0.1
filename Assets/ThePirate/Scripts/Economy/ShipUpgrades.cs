using GameWork.Framework.Ships.Movement;
using ThePirate.Combat;
using ThePirate.Ports;
using UnityEngine;

namespace ThePirate.Economy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatShip))]
    public sealed class ShipUpgrades : MonoBehaviour
    {
        private CombatShip ship;
        private UpgradeSettings settings;
        private ShipUpgradeState state = new ShipUpgradeState();
        private bool processing;
        public string LastError { get; private set; }

        public static ShipUpgrades Ensure(CombatShip target)
        {
            if (target == null || target.Team != 1) return null;
            return target.GetComponent<ShipUpgrades>() ?? target.gameObject.AddComponent<ShipUpgrades>();
        }
        private void Awake() => ship = GetComponent<CombatShip>();
        private bool Initialize(out string error)
        {
            error = null;
            if (settings != null) return true;
            var rules = Resources.Load<ShipUpgradeRules>("GameWork/ShipUpgradeRules");
            if (rules == null) error = "Brak ShipUpgradeRules. Tools > GameWork > Economy > Ship upgrade rules.";
            else rules.TrySnapshot(out settings, out error);
            LastError = error; return settings != null;
        }

        public bool TryCapture(out ShipUpgradeState captured, out string error)
        {
            captured = null;
            if (!Initialize(out error) || !CanRestore(state, out int maximum, out error)) return false;
            if (maximum != ship.Hull.MaxHealth) { error = "Maksymalne HP nie odpowiadają zapisanym poziomom ulepszeń."; return false; }
            captured = state.Copy(); captured.effectsSignature = settings.Signature; return true;
        }

        public bool CanRestore(ShipUpgradeState saved, out int expectedHullMaximum, out string error)
        {
            expectedHullMaximum = 0;
            if (!Initialize(out error)) return false;
            saved = saved ?? new ShipUpgradeState();
            if (!saved.Valid || (saved.Any && saved.effectsSignature != settings.Signature))
            { error = "Zmieniono definicję efektów ulepszeń lub poziomy zapisu są nieprawidłowe."; return false; }
            if (ship.Hull == null || ship.Hull.BaseMaxHealth < 1) { error = "Brak poprawnego kadłuba."; return false; }
            long maximum = (long)ship.Hull.BaseMaxHealth + (saved.hullLevel == 1 ? settings.HullBonus : 0);
            if (maximum > int.MaxValue) { error = "Maksymalne HP przekracza limit."; return false; }
            var movement = ship.GetComponent<ShipMovementController>();
            if (movement == null || !movement.HasMovementConfiguration) { error = "Brak konfiguracji napędu."; return false; }
            float speed = movement.BaseForwardSpeed * (saved.driveLevel == 1 ? settings.SpeedMultiplier : 1f);
            float acceleration = movement.BaseAcceleration * (saved.driveLevel == 1 ? settings.AccelerationMultiplier : 1f);
            if (float.IsNaN(speed) || float.IsInfinity(speed) || speed <= 0f || speed > 1000f
                || float.IsNaN(acceleration) || float.IsInfinity(acceleration) || acceleration <= 0f)
            { error = "Nieprawidłowe wynikowe parametry napędu."; return false; }
            if (saved.batteryLevel == 1)
            {
                var batteries = ship.GetComponentsInChildren<WeaponBattery>(true);
                if (batteries.Length == 0) { error = "Brak baterii dział."; return false; }
                foreach (var battery in batteries)
                    if ((long)battery.BaseDamage + settings.DamageBonus > int.MaxValue || battery.BaseDamage < 1)
                    { error = "Obrażenia baterii przekraczają limit."; return false; }
            }
            expectedHullMaximum = (int)maximum; error = null; return true;
        }

        public bool TryRestore(ShipUpgradeState saved, out string error)
        {
            if (!CanRestore(saved, out int maximum, out error)) return false;
            state = saved?.Copy() ?? new ShipUpgradeState();
            state.effectsSignature = settings.Signature;
            ship.GetComponent<ShipMovementController>().SetUpgradeMultipliers(state.driveLevel == 1 ? settings.SpeedMultiplier : 1f,
                state.driveLevel == 1 ? settings.AccelerationMultiplier : 1f);
            foreach (var battery in ship.GetComponentsInChildren<WeaponBattery>(true))
                battery.SetUpgrade(state.batteryLevel == 1 ? settings.DamageBonus : 0, state.batteryLevel == 1 ? settings.ReloadMultiplier : 1f);
            ship.Hull.SetMaximumPreservingDamage(maximum);
            return true;
        }

        public bool TryQuote(ShipUpgradeKind kind, out int price, out string preview, out bool purchased, out string error)
        {
            price = 0; preview = null; purchased = false;
            if (!Initialize(out error)) return false;
            var next = state.Copy();
            switch (kind)
            {
                case ShipUpgradeKind.Hull:
                    price = settings.HullPrice; purchased = state.hullLevel == 1; next.hullLevel = 1;
                    preview = $"Kadłub: {ship.Hull.BaseMaxHealth} → {(long)ship.Hull.BaseMaxHealth + settings.HullBonus} maks. HP"; break;
                case ShipUpgradeKind.Drive:
                    price = settings.DrivePrice; purchased = state.driveLevel == 1; next.driveLevel = 1;
                    preview = $"Napęd: prędkość ×{settings.SpeedMultiplier:0.##}, przyspieszenie ×{settings.AccelerationMultiplier:0.##}"; break;
                case ShipUpgradeKind.Batteries:
                    price = settings.BatteryPrice; purchased = state.batteryLevel == 1; next.batteryLevel = 1;
                    preview = $"Baterie: +{settings.DamageBonus} obrażeń na pocisk, czas następnych przeładowań ×{settings.ReloadMultiplier:0.##}"; break;
                default: error = "Nieprawidłowy rodzaj ulepszenia."; return false;
            }
            next.effectsSignature = settings.Signature;
            return CanRestore(next, out _, out error);
        }

        public bool TryBuy(ShipUpgradeKind kind, int displayedPrice, out string message)
        {
            message = null;
            if (processing) { message = "Ulepszenie jest już w toku."; return false; }
            if (!isActiveAndEnabled || global::Framework.Menu.PauseService.GameplayInputBlocked)
            { message = "Ulepszenie jest teraz niedostępne."; return false; }
            var port = PortRepairService.Ensure(ship);
            if (port == null || !port.Available(out message)) return false;
            if (!TryQuote(kind, out int price, out _, out bool purchased, out message)) return false;
            if (purchased) { message = "Ten poziom został już kupiony."; return false; }
            if (price != displayedPrice) { message = "Cena zmieniła się — sprawdź wycenę."; return false; }
            var gold = PlayerGold.Ensure(ship);
            if (gold == null || gold.Balance < price) { message = "Brakuje złota."; return false; }
            var next = state.Copy(); next.effectsSignature = settings.Signature;
            if (kind == ShipUpgradeKind.Hull) next.hullLevel = 1;
            else if (kind == ShipUpgradeKind.Drive) next.driveLevel = 1;
            else next.batteryLevel = 1;
            processing = true;
            try
            {
                if (!gold.Wallet.TrySpend(price)) { message = "Nie udało się opłacić ulepszenia."; return false; }
                // Stan i parametry były sprawdzone przed pobraniem złota.
                TryRestore(next, out _);
                message = $"Kupiono ulepszenie za {price} złota."; return true;
            }
            finally { processing = false; }
        }
    }
}
