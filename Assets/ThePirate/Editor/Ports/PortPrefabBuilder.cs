using System;
using ThePirate.Ports;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ThePirate.EditorTools
{
    [InitializeOnLoad]
    public static class PortPrefabBuilder
    {
        private const string Path = "Assets/ThePirate/Resources/GameWork/PortDock.prefab";
        static PortPrefabBuilder() => EditorApplication.delayCall += CreateMissing;
        private static void CreateMissing()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Path) == null) CreatePrefab();
        }
        [MenuItem("Tools/GameWork/Ports/Create or select port prefab")]
        public static void CreatePrefab()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
            if (existing != null) { Selection.activeObject = existing; return; }
            if (!AssetDatabase.IsValidFolder("Assets/ThePirate/Resources")) AssetDatabase.CreateFolder("Assets/ThePirate", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/ThePirate/Resources/GameWork")) AssetDatabase.CreateFolder("Assets/ThePirate/Resources", "GameWork");
            var root = new GameObject("PortDock");
            try
            {
                var dock = root.AddComponent<PortDock>();
                var point = new GameObject("Berth").transform; point.SetParent(root.transform, false); point.localPosition = new Vector3(0, 0.5f, 0);
                dock.Configure("sandbox-port", point);
                MakePart(root.transform, "Pier", new Vector3(-9, 0.7f, 0), new Vector3(5, 0.6f, 22));
                MakePart(root.transform, "Land", new Vector3(-22, 0, 0), new Vector3(20, 2, 30));
                for (int i = -1; i <= 1; i++) MakePart(root.transform, "MooringPost" + i, new Vector3(-6.5f, 1.2f, i * 8), new Vector3(0.5f, 1.5f, 0.5f));
                var marker = new GameObject("ApproachZone").transform; marker.SetParent(root.transform, false);
                var line = marker.gameObject.AddComponent<LineRenderer>(); line.useWorldSpace = false; line.loop = true; line.positionCount = 64; line.startWidth = line.endWidth = 0.12f;
                line.startColor = line.endColor = Color.cyan;
                var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    const string materialPath = "Assets/ThePirate/Resources/GameWork/PortZone.mat";
                    var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                    if (material == null) { material = new Material(shader); if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.cyan); material.color = Color.cyan; AssetDatabase.CreateAsset(material, materialPath); }
                    line.sharedMaterial = material;
                }
                for (int i = 0; i < 64; i++) { float angle = i * Mathf.PI * 2f / 64f; line.SetPosition(i, new Vector3(Mathf.Sin(angle) * 12, 0.15f, Mathf.Cos(angle) * 12)); }
                Selection.activeObject = PrefabUtility.SaveAsPrefabAsset(root, Path);
                AssetDatabase.SaveAssets();
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        private static void MakePart(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube); part.name = name; part.transform.SetParent(parent, false); part.transform.localPosition = position; part.transform.localScale = scale;
        }
        [MenuItem("Tools/GameWork/Ports/Place port in current scene")]
        public static void PlacePort()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            CreatePrefab();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path); if (prefab == null) return;
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(instance, "Place port");
            instance.transform.position = new Vector3(-60, 0, -60);
            var dock = instance.GetComponent<PortDock>(); dock.Configure(Guid.NewGuid().ToString("N"), dock.Berth);
            EditorUtility.SetDirty(dock); PrefabUtility.RecordPrefabInstancePropertyModifications(dock);
            EditorSceneManager.MarkSceneDirty(instance.scene); Selection.activeGameObject = instance;
        }
    }
}
