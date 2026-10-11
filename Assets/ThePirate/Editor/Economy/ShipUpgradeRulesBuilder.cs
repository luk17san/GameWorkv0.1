using ThePirate.Economy;
using UnityEditor;
using UnityEngine;

namespace ThePirate.EditorTools
{
    public static class ShipUpgradeRulesBuilder
    {
        private const string Path = "Assets/ThePirate/Resources/GameWork/ShipUpgradeRules.asset";
        [InitializeOnLoadMethod]
        private static void Schedule() => EditorApplication.delayCall += Ensure;
        private static void Ensure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall += Ensure; return; }
            if (AssetDatabase.LoadAssetAtPath<ShipUpgradeRules>(Path) == null) Create();
        }
        [MenuItem("Tools/GameWork/Economy/Ship upgrade rules")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var existing = AssetDatabase.LoadAssetAtPath<ShipUpgradeRules>(Path);
            if (existing != null) { Selection.activeObject = existing; return; }
            var rules = ScriptableObject.CreateInstance<ShipUpgradeRules>();
            AssetDatabase.CreateAsset(rules, Path); AssetDatabase.SaveAssets(); Selection.activeObject = rules;
        }
    }
}
