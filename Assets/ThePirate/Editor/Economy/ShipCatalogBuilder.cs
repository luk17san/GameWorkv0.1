using ThePirate.Ships;
using ThePirate.Combat;
using GameWork.Framework.Ships.Movement;
using UnityEditor;
using UnityEngine;

namespace ThePirate.EditorTools
{
    public static class ShipCatalogBuilder
    {
        private const string Folder = "Assets/ThePirate/Resources/GameWork/Ships";
        private const string CatalogPath = "Assets/ThePirate/Resources/GameWork/ShipCatalog.asset";
        private const string Source = "Assets/ThePirate/Prefab/PlayerShip.prefab";
        [InitializeOnLoadMethod]
        private static void Schedule() => EditorApplication.delayCall += Ensure;
        private static void Ensure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall += Ensure; return; }
            if (AssetDatabase.LoadAssetAtPath<ShipCatalog>(CatalogPath) == null) Create();
        }
        [MenuItem("Tools/GameWork/Economy/Ship catalog")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var existing = AssetDatabase.LoadAssetAtPath<ShipCatalog>(CatalogPath);
            if (existing != null) { Selection.activeObject = existing; return; }
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Source) == null) { Debug.LogError("Brak " + Source); return; }
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/ThePirate/Resources/GameWork", "Ships");
            var starter = Make("starter", "Statek startowy", 200, false);
            var brig = Make("brig", "Ciężki bryg", 400, true);
            var catalog = ScriptableObject.CreateInstance<ShipCatalog>();
            var serialized = new SerializedObject(catalog); var list = serialized.FindProperty("ships"); list.arraySize = 2;
            list.GetArrayElementAtIndex(0).objectReferenceValue = starter; list.GetArrayElementAtIndex(1).objectReferenceValue = brig;
            serialized.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.CreateAsset(catalog, CatalogPath); AssetDatabase.SaveAssets(); Selection.activeObject = catalog;
        }
        private static ShipDefinition Make(string id, string label, int price, bool heavy)
        {
            string definitionPath = Folder + "/" + id + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<ShipDefinition>(definitionPath); if (existing != null) return existing;
            string prefabPath = Folder + "/" + id + ".prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                var root = PrefabUtility.LoadPrefabContents(Source);
                try
                {
                    root.name = heavy ? "BrigShip" : "StarterShip";
                    var identity = root.GetComponent<ShipIdentity>() ?? root.AddComponent<ShipIdentity>(); identity.Configure(id);
                    var movement = root.GetComponent<ShipMovementController>(); var movementSerialized = new SerializedObject(movement);
                    var stats = (ShipMovementStats)movementSerialized.FindProperty("movementStats").objectReferenceValue;
                    var cargo = (ShipCargoStats)movementSerialized.FindProperty("cargoStats").objectReferenceValue;
                    if (stats == null || cargo == null) throw new System.InvalidOperationException("Prefab źródłowy wymaga konfiguracji ruchu i ładowni.");
                    var copiedStats = CopyAsset(stats, Folder + "/" + id + "-movement.asset");
                    var copiedCargo = CopyAsset(cargo, Folder + "/" + id + "-cargo.asset");
                    if (heavy)
                    {
                        var health = new SerializedObject(root.GetComponent<Framework.Health.Health>()); health.FindProperty("maxHealth").intValue = 750; health.ApplyModifiedPropertiesWithoutUndo();
                        var move = new SerializedObject(copiedStats); move.FindProperty("maxForwardSpeed").floatValue = 12f; move.FindProperty("acceleration").floatValue = 3f; move.ApplyModifiedPropertiesWithoutUndo();
                        var hold = new SerializedObject(copiedCargo); hold.FindProperty("cargoCapacity").floatValue = 1800f; hold.ApplyModifiedPropertiesWithoutUndo();
                        root.transform.localScale = Vector3.Scale(root.transform.localScale, new Vector3(1.15f, 1f, 1.15f));
                    }
                    movementSerialized.FindProperty("movementStats").objectReferenceValue = copiedStats;
                    movementSerialized.FindProperty("cargoStats").objectReferenceValue = copiedCargo;
                    movementSerialized.ApplyModifiedPropertiesWithoutUndo(); root.SetActive(true);
                    prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            var definition = ScriptableObject.CreateInstance<ShipDefinition>(); var data = new SerializedObject(definition);
            data.FindProperty("typeId").stringValue = id; data.FindProperty("displayName").stringValue = label;
            data.FindProperty("description").stringValue = heavy ? "Więcej wytrzymałości i miejsca na towary, niższa prędkość." : "Lekki statek do szybkiej żeglugi.";
            data.FindProperty("price").intValue = price; data.FindProperty("prefab").objectReferenceValue = prefab;
            data.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.CreateAsset(definition, definitionPath); return definition;
        }
        private static T CopyAsset<T>(T source, string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path); if (existing != null) return existing;
            var copy = Object.Instantiate(source); AssetDatabase.CreateAsset(copy, path); return copy;
        }
    }
}
