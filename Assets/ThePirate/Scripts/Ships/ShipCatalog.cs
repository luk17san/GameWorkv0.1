using System.Collections.Generic;
using Framework.Inventory;
using UnityEngine;

namespace ThePirate.Ships
{
    [CreateAssetMenu(fileName = "ShipCatalog", menuName = "GameWork/Ships/Ship Catalog")]
    public sealed class ShipCatalog : ScriptableObject
    {
        [SerializeField] private ShipDefinition[] ships = new ShipDefinition[0];
        public IReadOnlyList<ShipDefinition> Ships => System.Array.AsReadOnly(ships ?? new ShipDefinition[0]);
        public static ShipCatalog Load() => Resources.Load<ShipCatalog>("GameWork/ShipCatalog");
        public bool Validate(out string error)
        {
            error = null;
            if (ships == null || ships.Length < 1 || ships.Length > 32) { error = "Katalog wymaga 1–32 projektów."; return false; }
            var ids = new HashSet<string>();
            foreach (var definition in ships)
            {
                if (definition == null) { error = "Pusty projekt w katalogu."; return false; }
                if (!definition.Validate(out error)) return false;
                if (!ids.Add(definition.Id)) { error = "Powtórzone ID statku: " + definition.Id; return false; }
            }
            return true;
        }
        public bool TryFind(string id, out ShipDefinition definition, out string error)
        {
            definition = null;
            if (!Validate(out error)) return false;
            foreach (var candidate in ships) if (candidate.Id == id) { definition = candidate; return true; }
            error = "Nieznany projekt statku: " + id; return false;
        }
    }
}
