using ThePirate.UI.HUD;
using UnityEditor;
using UnityEngine;

namespace ThePirate.EditorTools
{
    public static class EnemyHudPrefabBuilder
    {
        private const string Path = "Assets/ThePirate/Resources/GameWork/EnemyHealthHud.prefab";
        [InitializeOnLoadMethod]
        private static void ScheduleCreation() => EditorApplication.delayCall += EnsurePrefab;
        private static void EnsurePrefab()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += EnsurePrefab;
                return;
            }
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Path) == null) Build(false);
        }
        [MenuItem("Tools/GameWork/AI/Create enemy HUD prefab")]
        public static void Create() => Build(true);
        private static void Build(bool select)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
            if (existing != null) { if (select) Selection.activeObject = existing; return; }
            var root = EnemyHealthHudLayout.Create();
            try
            {
                root.GetComponent<CanvasGroup>().alpha = 1;
                var prefab = PrefabUtility.SaveAsPrefabAsset(root, Path);
                if (select) Selection.activeObject = prefab;
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
