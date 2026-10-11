using ThePirate.Economy;
using UnityEditor;
using UnityEngine;

namespace ThePirate.EditorTools
{
    public static class ShipLootTableBuilder
    {
        private const string Path = "Assets/ThePirate/Resources/GameWork/ShipLootTable.asset";
        [InitializeOnLoadMethod] private static void Schedule() => EditorApplication.delayCall += Ensure;
        private static void Ensure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall += Ensure; return; }
            if (AssetDatabase.LoadAssetAtPath<ShipLootTable>(Path) == null) Create();
        }
        [MenuItem("Tools/GameWork/Economy/Ship loot table")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var existing = AssetDatabase.LoadAssetAtPath<ShipLootTable>(Path);
            if (existing != null) { Selection.activeObject = existing; return; }
            if (!AssetDatabase.IsValidFolder("Assets/ThePirate/Resources/GameWork")) { Debug.LogError("Brak folderu GameWork w Resources."); return; }
            var table = ScriptableObject.CreateInstance<ShipLootTable>(); var serialized = new SerializedObject(table);
            var rewards = serialized.FindProperty("rewards"); rewards.arraySize = 3;
            Set(rewards.GetArrayElementAtIndex(0), "wood", 30); Set(rewards.GetArrayElementAtIndex(1), "iron", 10); Set(rewards.GetArrayElementAtIndex(2), "gunpowder", 5);
            serialized.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.CreateAsset(table, Path); AssetDatabase.SaveAssets(); Selection.activeObject = table;
        }
        private static void Set(SerializedProperty item, string id, int amount)
        { item.FindPropertyRelative("resourceId").stringValue = id; item.FindPropertyRelative("quantity").intValue = amount; }
    }
}
