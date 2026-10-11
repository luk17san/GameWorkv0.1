using ThePirate.Economy;
using UnityEditor;
using UnityEngine;

namespace ThePirate.Editor.Economy
{
    public static class FloatingLootBuilder
    {
        private const string Folder = "Assets/ThePirate/Resources/GameWork/FloatingLoot";
        [InitializeOnLoadMethod]
        private static void Schedule() => EditorApplication.delayCall += CreateMissing;
        [MenuItem("Tools/GameWork/Economy/Floating loot prefabs")]
        public static void CreateMissing()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/ThePirate/Resources/GameWork", "FloatingLoot");
            Create("Planks", 0, 5); Create("Wreckage", 1, 10);
            AssetDatabase.SaveAssets();
        }
        private static void Create(string name, int appearance, int quantity)
        {
            string path = Folder + "/" + name + ".prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
            var go = FloatingLoot.CreateVisual(appearance);
            try
            {
                var loot = go.AddComponent<FloatingLoot>();
                var serialized = new SerializedObject(loot);
                serialized.FindProperty("appearance").intValue = appearance;
                serialized.FindProperty("resources").GetArrayElementAtIndex(0).FindPropertyRelative("quantity").intValue = quantity;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (shader != null)
                {
                    string materialPath = Folder + "/Driftwood.mat";
                    var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                    if (material == null)
                    {
                        material = new Material(shader) { color = new Color(0.32f, 0.18f, 0.08f) };
                        AssetDatabase.CreateAsset(material, materialPath);
                    }
                    foreach (var renderer in go.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = material;
                }
                PrefabUtility.SaveAsPrefabAsset(go, path);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
