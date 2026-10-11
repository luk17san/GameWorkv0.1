using Framework.Inventory;
using ThePirate.Ships;
using UnityEditor;
using UnityEngine;

namespace ThePirate.EditorTools
{
    [CustomEditor(typeof(ShipBuildRecipe))]
    public sealed class ShipBuildRecipeInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var recipe = (ShipBuildRecipe)target;
            if (!recipe.TrySnapshot(out var quote, out string error)) { EditorGUILayout.HelpBox(error, MessageType.Error); return; }
            var ships = ShipCatalog.Load();
            if (ships == null) { EditorGUILayout.HelpBox("Utwórz katalog statków, aby zweryfikować ID projektu.", MessageType.Warning); return; }
            if (!ships.TryFind(quote.ShipTypeId, out _, out error)) { EditorGUILayout.HelpBox(error, MessageType.Error); return; }
            var resources = Resources.Load<ResourceCatalog>("GameWork/ResourceCatalog");
            if (resources == null) { EditorGUILayout.HelpBox("Brak katalogu zasobów.", MessageType.Warning); return; }
            if (!resources.TryBuildWeights(out var weights, out error)) { EditorGUILayout.HelpBox(error, MessageType.Error); return; }
            foreach (var cost in quote.Materials())
                if (!weights.ContainsKey(cost.resourceId)) { EditorGUILayout.HelpBox("Nieznany zasób: " + cost.resourceId, MessageType.Error); return; }
            EditorGUILayout.HelpBox("Receptura jest poprawna. Ilości oznaczają sztuki zasobów, nie kilogramy. Koszt i pojemność pozostałego ładunku są sprawdzane ponownie przed budową.", MessageType.Info);
        }
    }
}
