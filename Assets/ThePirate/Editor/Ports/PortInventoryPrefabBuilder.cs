using System.IO;
using ThePirate.Ports;
using UnityEditor;
using UnityEngine;

namespace ThePirate.EditorTools
{
    public static class PortInventoryPrefabBuilder
    {
        private const string Path = "Assets/ThePirate/Resources/GameWork/PortMenu.prefab";
        [InitializeOnLoadMethod]
        private static void Schedule() => EditorApplication.delayCall += Ensure;
        private static void Ensure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall += Ensure; return; }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
            if (prefab != null && prefab.GetComponentInChildren<PortInventoryPanel>(true) == null) Build();
        }
        [MenuItem("Tools/GameWork/Ports/Validate inventory panel")]
        public static void Validate()
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
            var panel = root != null ? root.GetComponentInChildren<PortInventoryPanel>(true) : null;
            if (panel == null) throw new System.InvalidOperationException("Brak ekwipunku w prefabie.");
            var data = new SerializedObject(panel);
            foreach (string field in new[] { "weight", "details", "empty", "fill", "portrait", "content", "itemTemplate", "discardOne", "discardAll", "confirm", "cancel" })
                if (data.FindProperty(field).objectReferenceValue == null) throw new System.InvalidOperationException("Brak referencji: " + field);
            var filters = data.FindProperty("filters");
            if (filters.arraySize != 4) throw new System.InvalidOperationException("Nieprawidłowe filtry.");
            for (int i = 0; i < 4; i++) if (filters.GetArrayElementAtIndex(i).objectReferenceValue == null) throw new System.InvalidOperationException("Brak filtra.");
            foreach (var graphic in panel.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
                if (graphic.GetComponent<CanvasRenderer>() == null) throw new System.InvalidOperationException("Brak CanvasRenderer: " + graphic.name);
            var view = root.GetComponent<PortMenuView>();
            if (view == null || !view.HasVisual || !view.HasUpgradePanel || !view.HasShipyardPanel) throw new System.InvalidOperationException("Utracono usługi portu.");
            File.WriteAllText("Temp/PortInventoryValidation.txt", "PASS: 11 references, 4 filters, CanvasRenderers, original port services. Edit Mode only; Play Mode pending.");
            Debug.Log("Ekwipunek: walidacja prefabu PASS. Play Mode pozostaje do sprawdzenia.");
        }
        [MenuItem("Tools/GameWork/Ports/Create inventory panel")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var root = PrefabUtility.LoadPrefabContents(Path);
            try
            {
                if (root.GetComponentInChildren<PortInventoryPanel>(true) != null) return;
                Directory.CreateDirectory("Temp/PortInventoryBackup");
                File.Copy(Path, "Temp/PortInventoryBackup/PortMenu-" + System.DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".prefab", false);
                if (PortInventoryLayout.Ensure(root) == null) throw new System.InvalidOperationException("Brak CargoPanel.");
                PrefabUtility.SaveAsPrefabAsset(root, Path);
                Debug.Log("Ekwipunek portu: utworzono edytowalną siatkę, kategorie i szczegóły zasobów.");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
