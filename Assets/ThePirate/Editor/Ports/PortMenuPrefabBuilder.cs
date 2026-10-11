using ThePirate.Ports;
using UnityEditor;
using UnityEngine;

namespace ThePirate.EditorTools
{
    public static class PortMenuPrefabBuilder
    {
        private const string Path = "Assets/ThePirate/Resources/GameWork/PortMenu.prefab";
        [InitializeOnLoadMethod]
        private static void Schedule() => EditorApplication.delayCall += Ensure;
        private static void Ensure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall += Ensure; return; }
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Path) == null) Create(); else AddUpgradePanel();
        }
        private static void AddUpgradePanel()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
            if (prefab == null || (prefab.GetComponent<PortMenuView>()?.HasUpgradePanel == true && prefab.GetComponent<PortMenuView>()?.HasShipyardPanel == true && prefab.GetComponentInChildren<PortShipyardPanel>(true)?.HasBuildButton == true)) return;
            var root = PrefabUtility.LoadPrefabContents(Path);
            try
            {
                bool changed = PortUpgradeLayout.Ensure(root);
                changed = PortShipyardLayout.Ensure(root) || changed;
                changed = PortShipBuildLayout.Ensure(root) || changed;
                if (changed) PrefabUtility.SaveAsPrefabAsset(root, Path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        [MenuItem("Tools/GameWork/Ports/Create port menu prefab")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
            if (existing != null) { AddUpgradePanel(); Selection.activeObject = existing; return; }
            const string folder = "Assets/ThePirate/Resources/GameWork";
            if (!AssetDatabase.IsValidFolder(folder)) { Debug.LogError("Brak folderu " + folder); return; }
            var root = PortMenuLayout.Create();
            try { Selection.activeObject = PrefabUtility.SaveAsPrefabAsset(root, Path); }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
