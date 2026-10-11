using Framework.Inventory;
using ThePirate.Economy;
using UnityEditor;
using UnityEngine;

namespace ThePirate.EditorTools
{
    [CustomEditor(typeof(ShipLootTable))]
    public sealed class ShipLootTableInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (!((ShipLootTable)target).TrySnapshot(out var rewards, out string error)) { EditorGUILayout.HelpBox(error, MessageType.Error); return; }
            var catalog = Resources.Load<ResourceCatalog>("GameWork/ResourceCatalog");
            if (catalog == null) { EditorGUILayout.HelpBox("Brak katalogu zasobów.", MessageType.Warning); return; }
            if (!catalog.TryBuildWeights(out var weights, out error)) { EditorGUILayout.HelpBox(error, MessageType.Error); return; }
            double mass = 0;
            foreach (var item in rewards)
            {
                if (!weights.TryGetValue(item.resourceId, out float weight)) { EditorGUILayout.HelpBox("Nieznany zasób: " + item.resourceId, MessageType.Error); return; }
                mass += (double)weight * item.quantity;
            }
            EditorGUILayout.HelpBox(rewards.Length == 0 ? "Pusta tabela nie przyznaje materiałów." : $"Masa paczki: {mass:0.###} kg. Cały łup musi zmieścić się w ładowni.", MessageType.Info);
        }
    }
}
