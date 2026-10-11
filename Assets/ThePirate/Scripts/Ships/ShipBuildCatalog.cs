using UnityEngine;

namespace ThePirate.Ships
{
    [CreateAssetMenu(fileName = "ShipBuildCatalog", menuName = "GameWork/Ships/Build Catalog")]
    public sealed class ShipBuildCatalog : ScriptableObject
    {
        [SerializeField] private ShipBuildRecipe[] recipes = new ShipBuildRecipe[0];
        public static ShipBuildCatalog Load() => Resources.Load<ShipBuildCatalog>("GameWork/ShipBuildCatalog");
        public bool TryFind(string typeId, out ShipBuildQuote quote, out string error)
        {
            quote = null; error = null;
            if (recipes == null || recipes.Length > 32) { error = "Błędny katalog receptur (limit 32)."; return false; }
            var ids = new System.Collections.Generic.HashSet<string>();
            foreach (var recipe in recipes)
            {
                if (recipe == null) { error = "Pusta receptura w katalogu."; return false; }
                if (!recipe.TrySnapshot(out var candidate, out error)) return false;
                if (!ids.Add(candidate.ShipTypeId)) { error = "Powtórzona receptura statku: " + candidate.ShipTypeId; return false; }
                if (candidate.ShipTypeId == typeId) quote = candidate;
            }
            if (quote != null) return true;
            error = "Brak receptury dla tego statku."; return false;
        }
    }
}
