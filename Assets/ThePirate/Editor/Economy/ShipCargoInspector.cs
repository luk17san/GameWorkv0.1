using ThePirate.Economy;
using UnityEditor;
using UnityEngine;

namespace ThePirate.EditorTools
{
    [CustomEditor(typeof(ShipCargo))]
    public sealed class ShipCargoInspector : UnityEditor.Editor
    {
        private int selected, quantity = 1;
        private string message;
        public override bool RequiresConstantRepaint() => Application.isPlaying;

        public override void OnInspectorGUI()
        {
            if (!Application.isPlaying) { EditorGUILayout.HelpBox("Ładownia uruchamia się podczas gry. Pojemność pochodzi z ShipCargoStats kontrolera ruchu.", MessageType.Info); return; }
            var cargo = (ShipCargo)target;
            if (!cargo.TryGetInventory(out var inventory, out string error)) { EditorGUILayout.HelpBox(error, MessageType.Error); return; }
            EditorGUILayout.LabelField("Masa / pojemność", $"{inventory.TotalWeight:0.###} / {inventory.Capacity:0.###} kg");
            foreach (var stack in inventory.Snapshot())
            {
                var definition = cargo.Catalog.Find(stack.resourceId);
                EditorGUILayout.LabelField(definition != null ? definition.DisplayName : stack.resourceId, stack.quantity.ToString());
            }
            if (inventory.LegacyWeight > 0f) EditorGUILayout.LabelField("Stary ładunek bez zawartości", $"{inventory.LegacyWeight:0.###} kg");
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Narzędzia testowe w edytorze — nie są handlem ani nagrodami w grze.", MessageType.Info);
            var resources = cargo.Catalog.Resources;
            if (resources.Count > 0)
            {
                var names = new string[resources.Count];
                for (int i = 0; i < names.Length; i++) names[i] = resources[i] != null ? resources[i].DisplayName : "Brak zasobu";
                selected = Mathf.Clamp(EditorGUILayout.Popup("Zasób", selected, names), 0, names.Length - 1);
                quantity = EditorGUILayout.IntField("Ilość", quantity);
                var resource = resources[selected];
                using (new EditorGUI.DisabledScope(resource == null))
                {
                    if (GUILayout.Button("Test: załaduj")) message = inventory.TryAdd(resource.Id, quantity, out error) ? "Załadowano." : error;
                    if (GUILayout.Button("Test: wyładuj")) message = inventory.TryRemove(resource.Id, quantity, out error) ? "Wyładowano." : error;
                }
            }
            if (inventory.LegacyWeight > 0f && GUILayout.Button("Test: usuń stary anonimowy ładunek"))
                message = inventory.TryRestore(inventory.Snapshot(), 0f, out error) ? "Usunięto stary ładunek." : error;
            if (!string.IsNullOrEmpty(message)) EditorGUILayout.HelpBox(message, MessageType.Info);
        }
    }
}
