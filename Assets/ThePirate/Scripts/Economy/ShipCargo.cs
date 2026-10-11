using Framework.Inventory;
using GameWork.Framework.Ships.Movement;
using ThePirate.Combat;
using UnityEngine;

namespace ThePirate.Economy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ShipMovementController))]
    public sealed class ShipCargo : MonoBehaviour
    {
        public const string CatalogPath = "GameWork/ResourceCatalog";
        private ShipMovementController movement;
        private CargoInventory inventory;
        public ResourceCatalog Catalog { get; private set; }
        public string LastError { get; private set; }

        public static ShipCargo Ensure(CombatShip ship)
        {
            if (ship == null || ship.Team != 1 || ship.GetComponent<ShipMovementController>() == null) return null;
            var cargo = ship.GetComponent<ShipCargo>();
            return cargo != null ? cargo : ship.gameObject.AddComponent<ShipCargo>();
        }

        // Inicjalizujemy na żądanie, również w sceneLoaded przed Start komponentów.
        public bool TryGetInventory(out CargoInventory result, out string error)
        {
            if (inventory == null)
            {
                movement = GetComponent<ShipMovementController>();
                Catalog = UnityEngine.Resources.Load<ResourceCatalog>(CatalogPath);
                error = null;
                if (Catalog == null) error = "Brak katalogu zasobów. Tools > GameWork > Economy > Resource catalog.";
                else if (movement == null || !movement.HasCargoCapacity || float.IsNaN(movement.CargoCapacity) || float.IsInfinity(movement.CargoCapacity) || movement.CargoCapacity <= 0f) error = "Statek wymaga poprawnego ShipCargoStats.";
                else if (Catalog.TryBuildWeights(out var weights, out error))
                {
                    var created = new CargoInventory(movement.CargoCapacity, weights);
                    if (created.TryRestore(new ResourceStack[0], movement.CurrentCargoWeight, out error))
                    {
                        inventory = created;
                        inventory.Changed += SynchronizeWeight;
                        SynchronizeWeight();
                    }
                }
                LastError = error;
            }
            result = inventory; error = LastError; return inventory != null;
        }

        private void Start()
        {
            if (!TryGetInventory(out _, out string error)) Debug.LogWarning(error, this);
        }
        private void SynchronizeWeight() => movement.SetCargoWeight(inventory.TotalWeight);
        private void OnDestroy() { if (inventory != null) inventory.Changed -= SynchronizeWeight; }
    }
}
