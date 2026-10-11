using Framework.Economy;
using ThePirate.Combat;
using ThePirate.Economy;
using UnityEngine;

namespace ThePirate.Ports
{
    public enum RepairTarget { Hull, Modules, All }
    public struct RepairQuote
    {
        public int Cost;
        public int HullMissing;
        public int ModuleMissing;
        public bool NeedsRepair => HullMissing > 0 || ModuleMissing > 0;
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatShip))]
    public sealed class PortRepairService : MonoBehaviour
    {
        private CombatShip ship;
        private ShipDocking docking;
        private bool processing;
        public CombatShip Ship => ship;
        public PortDock Port => docking != null && docking.IsDocked ? PortDock.Find(gameObject.scene, docking.PortId) : null;
        public int Gold => PlayerGold.Ensure(ship)?.Balance ?? 0;

        public static PortRepairService Ensure(CombatShip target)
        {
            if (target == null || target.Team != 1) return null;
            return target.GetComponent<PortRepairService>() ?? target.gameObject.AddComponent<PortRepairService>();
        }
        private void Awake() { ship = GetComponent<CombatShip>(); docking = GetComponent<ShipDocking>(); }

        public bool Available(out string error)
        {
            error = null;
            if (!isActiveAndEnabled || ship == null || ship.Team != 1 || !ship.Alive || docking == null || !docking.isActiveAndEnabled || !docking.IsDocked)
                error = "Usługi wymagają żywego, zacumowanego statku.";
            else if (ship.DockingBlocked)
            {
                docking.Undock();
                error = "Zagrożenie — usługi portu przerwane.";
            }
            else if (Port == null || !Port.AvailableFor(docking)) error = "Port jest niedostępny.";
            return error == null;
        }

        public bool TryQuote(RepairTarget target, out RepairQuote quote, out string error)
        {
            quote = default; error = null;
            if (!Available(out error)) return false;
            if (target != RepairTarget.Hull && target != RepairTarget.Modules && target != RepairTarget.All)
            { error = "Nieprawidłowy rodzaj naprawy."; return false; }
            PortDock port = Port;
            long cost = 0, moduleMissing = 0;
            if (target != RepairTarget.Modules)
            {
                if (!RepairCost.TryCalculate(ship.Hull.CurrentHealth, ship.Hull.MaxHealth, port.HullRepairGoldPerHealth, out int missing, out int hullCost))
                { error = "Nieprawidłowy koszt naprawy kadłuba."; return false; }
                quote.HullMissing = missing; cost += hullCost;
            }
            if (target != RepairTarget.Hull)
            {
                foreach (var module in ship.GetComponentsInChildren<ShipDamageModule>(true))
                {
                    if (!RepairCost.TryCalculate(module.CurrentHealth, module.MaxHealth, port.ModuleRepairGoldPerHealth, out int missing, out int moduleCost))
                    { error = "Nieprawidłowy koszt naprawy modułu."; return false; }
                    moduleMissing += missing; cost += moduleCost;
                }
            }
            if (cost > int.MaxValue || moduleMissing > int.MaxValue)
            { error = "Koszt naprawy przekracza limit."; return false; }
            quote.ModuleMissing = (int)moduleMissing; quote.Cost = (int)cost;
            return true;
        }

        public bool TryRepair(RepairTarget target, int displayedCost, out string message)
        {
            message = null;
            if (processing) { message = "Naprawa jest już w toku."; return false; }
            if (global::Framework.Menu.PauseService.GameplayInputBlocked)
            { message = "Naprawa jest niedostępna podczas pauzy lub wczytywania."; return false; }
            if (!TryQuote(target, out RepairQuote quote, out message)) return false;
            if (quote.Cost != displayedCost) { message = "Koszt zmienił się — sprawdź nową wycenę."; return false; }
            if (!quote.NeedsRepair) { message = "Wybrane elementy nie wymagają naprawy."; return false; }
            var gold = PlayerGold.Ensure(ship);
            if (gold == null || gold.Balance < quote.Cost) { message = "Brakuje złota na pełną naprawę."; return false; }
            processing = true;
            try
            {
                if (!gold.Wallet.TrySpend(quote.Cost)) { message = "Nie udało się opłacić naprawy."; return false; }
                if (target != RepairTarget.Modules) ship.Hull.RestoreCurrentHealth(ship.Hull.MaxHealth);
                if (target != RepairTarget.Hull)
                    foreach (var module in ship.GetComponentsInChildren<ShipDamageModule>(true)) module.RestoreCurrentHealth(module.MaxHealth);
                ThePirate.VFX.CombatVFXLink.Ensure(ship).Repair(quote.HullMissing / (float)ship.Hull.MaxHealth);
                message = $"Naprawiono. Koszt: {quote.Cost} złota.";
                return true;
            }
            finally { processing = false; }
        }
        public void Undock() => docking?.Undock();
    }
}
