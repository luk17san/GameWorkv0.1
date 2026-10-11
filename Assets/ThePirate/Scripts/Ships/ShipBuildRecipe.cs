using System;
using System.Collections.Generic;
using System.Globalization;
using Framework.Inventory;
using UnityEngine;

namespace ThePirate.Ships
{
    [CreateAssetMenu(fileName = "ShipBuildRecipe", menuName = "GameWork/Ships/Build Recipe")]
    public sealed class ShipBuildRecipe : ScriptableObject
    {
        [SerializeField] private string shipTypeId;
        [SerializeField, Min(0)] private int goldCost;
        [SerializeField] private ResourceStack[] materials = new ResourceStack[0];
        public string ShipTypeId => shipTypeId;
        public bool TrySnapshot(out ShipBuildQuote quote, out string error)
        {
            quote = null; error = null;
            if (!ResourceDefinition.ValidId(shipTypeId) || goldCost < 0 || materials == null || materials.Length < 1 || materials.Length > 1024)
            { error = "Receptura wymaga ID statku, nieujemnego kosztu złota i materiałów."; return false; }
            var copy = new ResourceStack[materials.Length]; var ids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < materials.Length; i++)
            {
                var item = materials[i];
                if (item == null || !ResourceDefinition.ValidId(item.resourceId) || item.quantity <= 0 || !ids.Add(item.resourceId))
                { error = "Receptura zawiera błędny lub powtórzony materiał."; return false; }
                copy[i] = new ResourceStack { resourceId = item.resourceId, quantity = item.quantity };
            }
            Array.Sort(copy, (a, b) => StringComparer.Ordinal.Compare(a.resourceId, b.resourceId));
            quote = new ShipBuildQuote(shipTypeId, goldCost, copy); return true;
        }
    }

    // Kopia oferty: zmiana assetu po pokazaniu ceny wymaga nowego potwierdzenia.
    public sealed class ShipBuildQuote
    {
        private readonly ResourceStack[] materials;
        public string ShipTypeId { get; }
        public int GoldCost { get; }
        public string Signature { get; }
        internal ShipBuildQuote(string typeId, int gold, ResourceStack[] costs)
        {
            ShipTypeId = typeId; GoldCost = gold; materials = costs;
            var signature = new System.Text.StringBuilder(typeId).Append(':').Append(gold.ToString(CultureInfo.InvariantCulture));
            foreach (var cost in materials) signature.Append('|').Append(cost.resourceId).Append(':').Append(cost.quantity.ToString(CultureInfo.InvariantCulture));
            Signature = signature.ToString();
        }
        public ResourceStack[] Materials()
        {
            var copy = new ResourceStack[materials.Length];
            for (int i = 0; i < copy.Length; i++) copy[i] = new ResourceStack { resourceId = materials[i].resourceId, quantity = materials[i].quantity };
            return copy;
        }
    }
}
