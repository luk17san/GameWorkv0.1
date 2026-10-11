using Framework.Inventory;
using ThePirate.Ships;
using UnityEditor;
using UnityEngine;

namespace ThePirate.EditorTools
{
    public static class ShipBuildCatalogBuilder
    {
        private const string Folder = "Assets/ThePirate/Resources/GameWork";
        private const string CatalogPath = Folder + "/ShipBuildCatalog.asset";
        [InitializeOnLoadMethod] private static void Schedule() => EditorApplication.delayCall += Ensure;
        private static void Ensure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall += Ensure; return; }
            if (AssetDatabase.LoadAssetAtPath<ShipBuildCatalog>(CatalogPath) == null) Create();
        }
        [MenuItem("Tools/GameWork/Economy/Ship build recipes")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var existing = AssetDatabase.LoadAssetAtPath<ShipBuildCatalog>(CatalogPath);
            if (existing != null) { Selection.activeObject = existing; return; }
            if (!AssetDatabase.IsValidFolder(Folder)) { Debug.LogError("Brak folderu " + Folder); return; }
            // Nie wymagamy kolejności delayCall generatorów statków/zasobów. ID są sprawdzane przy budowie.
            var starter = Make("starter", 75, 20, 5);
            var brig = Make("brig", 150, 60, 20);
            var catalog = ScriptableObject.CreateInstance<ShipBuildCatalog>(); var data = new SerializedObject(catalog);
            var recipes = data.FindProperty("recipes"); recipes.arraySize = 2;
            recipes.GetArrayElementAtIndex(0).objectReferenceValue = starter; recipes.GetArrayElementAtIndex(1).objectReferenceValue = brig;
            data.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.CreateAsset(catalog, CatalogPath); AssetDatabase.SaveAssets(); Selection.activeObject = catalog;
        }
        private static ShipBuildRecipe Make(string type, int gold, int wood, int iron)
        {
            string path = Folder + "/" + type + "-build.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ShipBuildRecipe>(path); if (existing != null) return existing;
            var recipe = ScriptableObject.CreateInstance<ShipBuildRecipe>(); var data = new SerializedObject(recipe);
            data.FindProperty("shipTypeId").stringValue = type; data.FindProperty("goldCost").intValue = gold;
            var list = data.FindProperty("materials"); list.arraySize = 2;
            SetCost(list.GetArrayElementAtIndex(0), "wood", wood); SetCost(list.GetArrayElementAtIndex(1), "iron", iron);
            data.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.CreateAsset(recipe, path); return recipe;
        }
        private static void SetCost(SerializedProperty item, string id, int amount)
        { item.FindPropertyRelative("resourceId").stringValue = id; item.FindPropertyRelative("quantity").intValue = amount; }
    }
}
