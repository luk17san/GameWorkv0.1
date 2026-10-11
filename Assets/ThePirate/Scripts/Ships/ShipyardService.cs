using ThePirate.Combat;
using ThePirate.Economy;
using ThePirate.Ports;
using UnityEngine;
using Framework.Inventory;

namespace ThePirate.Ships
{
    [DisallowMultipleComponent]
    public sealed class ShipyardService : MonoBehaviour
    {
        private CombatShip ship;
        private bool processing;
        public static ShipyardService Ensure(CombatShip target)
        {
            if (target == null || target.Team != 1) return null;
            return target.GetComponent<ShipyardService>() ?? target.gameObject.AddComponent<ShipyardService>();
        }
        private void Awake() => ship = GetComponent<CombatShip>();

        public bool TryQuote(string typeId, out ShipDefinition definition, out string error)
        {
            if (!TryOffer(typeId, out definition, out error)) return false;
            if (!ShipCargo.Ensure(ship).TryGetInventory(out var inventory, out error)) return false;
            return inventory.TryPlanConsumption(new ResourceStack[0], definition.Capacity, out _, out error);
        }

        private bool TryOffer(string typeId, out ShipDefinition definition, out string error)
        {
            definition = null;
            var catalog = ShipCatalog.Load();
            if (catalog == null) { error = "Brak katalogu statków."; return false; }
            if (!catalog.TryFind(typeId, out definition, out error)) return false;
            var port = PortRepairService.Ensure(ship);
            if (port == null || !port.Available(out error)) return false;
            if (ShipIdentity.Ensure(ship).TypeId == typeId) { error = "Ten typ statku jest już aktywny."; return false; }
            return true;
        }

        public bool TryQuoteBuild(string typeId, out ShipBuildQuote quote, out string error)
        {
            quote = null;
            if (!TryOffer(typeId, out var definition, out error)) return false;
            var recipes = ShipBuildCatalog.Load();
            if (recipes == null) { error = "Brak katalogu receptur."; return false; }
            if (!recipes.TryFind(typeId, out quote, out error)) return false;
            if (!ShipCargo.Ensure(ship).TryGetInventory(out var inventory, out error)) return false;
            return inventory.TryPlanConsumption(quote.Materials(), definition.Capacity, out _, out error);
        }

        public bool TryBuild(string typeId, string displayedSignature, out string message)
        {
            message = null;
            if (processing || !isActiveAndEnabled || Framework.Menu.PauseService.GameplayInputBlocked)
            { message = "Budowa jest teraz niedostępna."; return false; }
            if (!TryQuoteBuild(typeId, out var quote, out message)) return false;
            if (quote.Signature != displayedSignature) { message = "Receptura zmieniła się — potwierdź nowy koszt."; return false; }
            if (!TryOffer(typeId, out var definition, out message)) return false;
            return TryExchange(definition, quote.GoldCost, quote.Materials(), true, out message);
        }

        public bool TryBuy(string typeId, int displayedPrice, out string message)
        {
            message = null;
            if (processing || !isActiveAndEnabled || Framework.Menu.PauseService.GameplayInputBlocked)
            { message = "Wymiana jest teraz niedostępna."; return false; }
            if (!TryQuote(typeId, out var definition, out message)) return false;
            if (definition.Price != displayedPrice) { message = "Cena zmieniła się — sprawdź nową ofertę."; return false; }
            return TryExchange(definition, definition.Price, new ResourceStack[0], false, out message);
        }

        private bool TryExchange(ShipDefinition definition, int goldCost, ResourceStack[] materials, bool building, out string message)
        {
            message = null;
            var wallet = PlayerGold.Ensure(ship);
            if (wallet.Balance < goldCost) { message = "Brakuje złota."; return false; }
            if (!ShipCargo.Ensure(ship).TryGetInventory(out var previousCargo, out message)
                || !previousCargo.TryPlanConsumption(materials, definition.Capacity, out var remaining, out message)) return false;
            var port = PortRepairService.Ensure(ship).Port;
            CombatShip prepared = null;
            processing = true;
            try
            {
                if (!ShipReplacement.TryPrepare(definition, gameObject.scene, ship.transform.position, ship.transform.rotation, out prepared, out message)) return false;
                var cargo = ShipCargo.Ensure(prepared);
                if (!cargo.TryGetInventory(out var inventory, out message) || !inventory.TryRestore(remaining, previousCargo.LegacyWeight, out message)) return false;
                if (!ShipUpgrades.Ensure(prepared).TryRestore(null, out message)) return false;
                if (!prepared.GetComponent<ShipDocking>().ClearBerthForReplacement(port, ship.transform))
                { message = "Nowy statek nie mieści się przy tym pomoście."; return false; }
                // Saldo poprzedniego statku zostaje nienaruszone aż do skutecznej wymiany.
                PlayerGold.Ensure(prepared).Wallet.TryRestore(wallet.Balance - goldCost);
                if (!ShipReplacement.TryCommit(ship, prepared, port, out message)) return false;
                prepared = null;
                message = building ? "Zbudowano statek. Materiały zużyto, pozostały ładunek i złoto przeniesiono." : "Kupiono nowy statek. Ładownia i pozostałe złoto zostały przeniesione."; return true;
            }
            finally { if (prepared != null) ShipReplacement.Discard(prepared); processing = false; }
        }
    }
}
