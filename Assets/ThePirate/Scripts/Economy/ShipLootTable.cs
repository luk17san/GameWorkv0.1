using Framework.Inventory;
using UnityEngine;

namespace ThePirate.Economy
{
    [CreateAssetMenu(fileName = "ShipLootTable", menuName = "GameWork/Economy/Ship Loot Table")]
    public sealed class ShipLootTable : ScriptableObject
    {
        [SerializeField] private ResourceStack[] rewards = new ResourceStack[0];
        public bool TrySnapshot(out ResourceStack[] result, out string error)
        {
            result = null; error = null;
            if (rewards == null || rewards.Length > 1024) { error = "Błędna tabela łupu."; return false; }
            var ids = new System.Collections.Generic.HashSet<string>();
            var copy = new ResourceStack[rewards.Length];
            for (int i = 0; i < rewards.Length; i++)
            {
                var item = rewards[i];
                if (item == null || !ResourceDefinition.ValidId(item.resourceId) || item.quantity <= 0 || !ids.Add(item.resourceId))
                { error = "Tabela łupu wymaga poprawnych, unikalnych ID i dodatnich ilości."; return false; }
                copy[i] = new ResourceStack { resourceId = item.resourceId, quantity = item.quantity };
            }
            result = copy; return true;
        }
    }
}
