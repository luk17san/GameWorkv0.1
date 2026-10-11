#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using ThePirate.Combat;
using ThePirate.UI.HUD;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ThePirate.Editor.World
{
    [InitializeOnLoad]
    public static class ParchmentMapBuilder
    {
        const string ScenePath = "Assets/ThePirate/Scenes/WybrzezeTrzechSosen.unity";
        const string TexturePath = "Assets/ThePirate/World/ThreePines/ParchmentMap.png";
        const string PrefabPath = "Assets/ThePirate/Resources/GameWork/ParchmentMap.prefab";
        const string Request = "Temp/ParchmentMap.install";
        static ParchmentMapBuilder() { EditorApplication.update += Pending; }
        static void Pending()
        {
            if (!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            File.Delete(Request);
            try { Install(); }
            catch (Exception e) { File.WriteAllText("Temp/ParchmentMap.result", "FAIL: " + e); Debug.LogException(e); }
        }
        [MenuItem("GameWork/Map/Wybrzeze Trzech Sosen/Install parchment map")]
        public static void Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(ScenePath);
            if (scene.IsValid() && scene.isLoaded && scene.isDirty) throw new InvalidOperationException("Zapisz scenę przed instalacją mapy, potem uruchom polecenie ponownie.");
            bool opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                if (scene.GetRootGameObjects().Any(r => r.GetComponentInChildren<ParchmentMapView>(true) != null))
                    throw new InvalidOperationException("Mapa już istnieje; zachowano jej konfigurację.");
                var player = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<CombatShip>()).Single(s => s.Team == 1);
                var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
                importer.textureType = TextureImporterType.Default; importer.maxTextureSize = 2048;
                importer.mipmapEnabled = false; importer.npotScale = TextureImporterNPOTScale.None;
                importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
                if (!texture || texture.width != 1536 || texture.height != 1024) throw new InvalidOperationException("Expected reference image 1536 x 1024.");
                Directory.CreateDirectory("Documentation/ThreePines");
                File.Copy(ScenePath, "Documentation/ThreePines/ParchmentMap.before-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".unity.bak");
                if (File.Exists(PrefabPath)) throw new InvalidOperationException("Prefab mapy już istnieje; zachowano poprzednią wersję.");
                var root = new GameObject("ParchmentMap", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, scene);
                Undo.RegisterCreatedObjectUndo(root, "Install parchment map");
                var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 200;
                var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1600, 1000); scaler.matchWidthOrHeight = .5f;
                var view = root.AddComponent<ParchmentMapView>();
                var panel = Rect("MapPanel", root.transform); Stretch(panel, Vector2.zero, Vector2.one);
                var shade = panel.gameObject.AddComponent<Image>(); shade.color = new Color(0,0,0,.8f); shade.raycastTarget = false;
                var viewport = Rect("MapViewport", panel); Stretch(viewport, new Vector2(.04f,.08f), new Vector2(.96f,.96f));
                var paper = Rect("Parchment", viewport); Stretch(paper, Vector2.zero, Vector2.one);
                var image = paper.gameObject.AddComponent<RawImage>(); image.texture = texture; image.raycastTarget = false;
                var fitter = paper.gameObject.AddComponent<AspectRatioFitter>(); fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent; fitter.aspectRatio = 1.5f;
                var halo = Rect("ShipMarker", paper); halo.sizeDelta = new Vector2(32,32);
                var ring = halo.gameObject.AddComponent<HudShapeGraphic>(); ring.shape = HudShapeGraphic.ShapeKind.Disc; ring.color = new Color(1,.93f,.72f); ring.raycastTarget = false;
                var arrow = Rect("Heading", halo); arrow.sizeDelta = new Vector2(22,26);
                var graphic = arrow.gameObject.AddComponent<HudShapeGraphic>(); graphic.shape = HudShapeGraphic.ShapeKind.Polygon;
                graphic.points = new[] { new Vector2(.5f,1), new Vector2(0,0), new Vector2(1,0) }; graphic.color = new Color(.65f,.08f,.025f); graphic.raycastTarget = false;
                var label = Rect("Status", panel); Stretch(label,new Vector2(.1f,0),new Vector2(.9f,.07f));
                var text = label.gameObject.AddComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.fontSize = 24; text.alignment = TextAnchor.MiddleCenter; text.color = new Color(1,.93f,.75f); text.raycastTarget = false;
                view.panel = panel.gameObject; view.parchment = paper; view.marker = halo; view.status = text;
                panel.gameObject.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                UnityEngine.Object.DestroyImmediate(root);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), scene);
                var installed = instance.GetComponent<ParchmentMapView>(); installed.player = player.transform;
                PrefabUtility.RecordPrefabInstancePropertyModifications(installed);
                var uv = installed.WorldToImage(player.transform.position);
                if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) throw new InvalidOperationException("Player mapping outside image.");
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
                File.WriteAllText("Temp/ParchmentMap.result", "PASS Editor: prefab + scene binding; player UV=" + uv + "; Play Mode pending.");
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }
        static RectTransform Rect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f,.5f); return rect;
        }
        static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
        { rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero; }
    }
}
#endif
