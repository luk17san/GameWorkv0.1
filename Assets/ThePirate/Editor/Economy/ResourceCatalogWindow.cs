using System;
using Framework.Inventory;
using ThePirate.Economy;
using UnityEditor;
using UnityEngine;

namespace ThePirate.EditorTools
{
    public sealed class ResourceCatalogWindow : EditorWindow
    {
        public const string CatalogAssetPath = "Assets/ThePirate/Resources/GameWork/ResourceCatalog.asset";
        private ResourceCatalog catalog;
        private ResourceDefinition selected;
        private UnityEditor.Editor catalogEditor, resourceEditor;
        private Vector2 scroll;
        private string newName = "Nowy zasób";
        private string message;

        [MenuItem("Tools/GameWork/Economy/Resource catalog")]
        public static void Open()
        {
            EnsureCatalog();
            GetWindow<ResourceCatalogWindow>("Katalog zasobów").Show();
        }

        [InitializeOnLoadMethod]
        private static void Schedule() => EditorApplication.delayCall += EnsureCatalog;

        private static void EnsureCatalog()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            { EditorApplication.delayCall += EnsureCatalog; return; }
            if (AssetDatabase.LoadAssetAtPath<ResourceCatalog>(CatalogAssetPath) != null) return;
            EnsureFolder("Assets/ThePirate/Resources/GameWork/ResourceDefinitions");
            var created = CreateInstance<ResourceCatalog>();
            AssetDatabase.CreateAsset(created, CatalogAssetPath);
            var defaults = new[] {
                CreateResource("wood", "Drewno", 2f, 5, ResourceCategory.Material),
                CreateResource("iron", "Żelazo", 5f, 12, ResourceCategory.Material),
                CreateResource("gunpowder", "Proch", 1f, 20, ResourceCategory.Ammunition)
            };
            var serialized = new SerializedObject(created);
            var list = serialized.FindProperty("resources"); list.arraySize = defaults.Length;
            for (int i = 0; i < defaults.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = defaults[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }

        private static ResourceDefinition CreateResource(string id, string label, float weight, int price, ResourceCategory category)
        {
            string path = "Assets/ThePirate/Resources/GameWork/ResourceDefinitions/" + id + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<ResourceDefinition>(path);
            if (existing != null) return existing;
            var resource = CreateInstance<ResourceDefinition>();
            var serialized = new SerializedObject(resource);
            serialized.FindProperty("resourceId").stringValue = id;
            serialized.FindProperty("displayName").stringValue = label;
            serialized.FindProperty("unitWeight").floatValue = weight;
            serialized.FindProperty("basePrice").intValue = price;
            serialized.FindProperty("category").enumValueIndex = (int)category;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(resource, path);
            return resource;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/'); string parent = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = parent + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(parent, parts[i]);
                parent = next;
            }
        }

        private void OnGUI()
        {
            if (catalog == null) catalog = AssetDatabase.LoadAssetAtPath<ResourceCatalog>(CatalogAssetPath);
            if (catalog == null) { EditorGUILayout.HelpBox("Katalog nie jest jeszcze gotowy. Otwórz okno ponownie po imporcie.", MessageType.Warning); return; }
            EditorGUILayout.HelpBox("ID jest kluczem zapisu. Zmieniaj nazwę, ikonę i opis; zachowaj ID już używanego zasobu. Zmiany katalogu testuj w nowej sesji Play Mode.", MessageType.Info);
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                newName = EditorGUILayout.TextField("Nazwa nowego zasobu", newName);
                if (GUILayout.Button("Dodaj zasób do katalogu") && !string.IsNullOrWhiteSpace(newName))
                {
                    string id = "resource-" + Guid.NewGuid().ToString("N");
                    selected = CreateResource(id, newName.Trim(), 1f, 0, ResourceCategory.Material);
                    var serialized = new SerializedObject(catalog);
                    var list = serialized.FindProperty("resources"); int index = list.arraySize;
                    list.arraySize++; list.GetArrayElementAtIndex(index).objectReferenceValue = selected;
                    serialized.ApplyModifiedProperties(); AssetDatabase.SaveAssets();
                    message = "Dodano " + selected.DisplayName + ".";
                }
                UnityEditor.Editor.CreateCachedEditor(catalog, null, ref catalogEditor);
                scroll = EditorGUILayout.BeginScrollView(scroll);
                catalogEditor.OnInspectorGUI();
                selected = (ResourceDefinition)EditorGUILayout.ObjectField("Edytowany zasób", selected, typeof(ResourceDefinition), false);
                foreach (var resource in catalog.Resources)
                    if (resource != null && GUILayout.Button(resource.DisplayName + " [" + resource.Id + "]")) selected = resource;
                if (selected != null)
                {
                    UnityEditor.Editor.CreateCachedEditor(selected, null, ref resourceEditor);
                    resourceEditor.OnInspectorGUI();
                }
                EditorGUILayout.EndScrollView();
            }
            if (GUILayout.Button("Sprawdź katalog")) message = catalog.TryBuildWeights(out _, out string error) ? "Katalog poprawny." : error;
            if (!string.IsNullOrEmpty(message)) EditorGUILayout.HelpBox(message, MessageType.Info);
        }
        private void OnDisable()
        {
            if (catalogEditor != null) DestroyImmediate(catalogEditor);
            if (resourceEditor != null) DestroyImmediate(resourceEditor);
        }
    }
}
