#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using GameWork.ArchipelagoKit.Water;

namespace GameWork.ArchipelagoKit.Editor
{
    public sealed class ArchipelagoSetupWindow : EditorWindow
    {
        private enum Formation { SingleIsland, Archipelago, LargeIsland }
        private enum SculptMode { Raise, Lower, Smooth, Flatten }
        private Formation formation = Formation.Archipelago;
        private SculptMode sculptMode = SculptMode.Raise;
        private float seaLevel;
        private float islandSize = 420f;
        private float terrainHeight = 110f;
        private int heightmapResolution = 513;
        private int seed = 17;
        private float brushRadius = 36f;
        private float brushStrength = 0.35f;
        private int scatterCount = 90;
        private float maxSlope = 24f;
        private float minDryHeight = 1.5f;
        private GameObject[] scatterPrefabs = new GameObject[0];
        private Vector2 scroll;
        private static ArchipelagoSetupWindow activeWindow;
        private static float sceneBrushRadius = 36f;
        private static float sceneBrushStrength = .35f;
        private static int sceneMode;
        private static float seaLevelForScatter;
        private static float flattenTarget;
        private static bool flattenCaptured;
        private static bool undoRegistered;

        [MenuItem("GameWork/Archipelago/Setup & Terrain Editor")]
        private static void Open() => GetWindow<ArchipelagoSetupWindow>("Archipelago");

        private void OnEnable() { activeWindow = this; }
        private void OnDisable() { if (activeWindow == this) activeWindow = null; }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("GAMEWORK • ARCHIPELAGO KIT", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Stylizowane morze i edytor wysp dla Unity 6 URP. Poziom morza jest wspólną wysokością odniesienia dla generatora i dekoracji.", MessageType.Info);
            seaLevel = EditorGUILayout.FloatField("Sea Level (Y)", seaLevel);
            EditorGUILayout.Space(6);
            DrawOceanSection();
            EditorGUILayout.Space(10);
            DrawTerrainSection();
            EditorGUILayout.Space(10);
            DrawSculptSection();
            EditorGUILayout.Space(10);
            DrawScatterSection();
            EditorGUILayout.EndScrollView();
        }

        private void DrawOceanSection()
        {
            EditorGUILayout.LabelField("Ocean", EditorStyles.boldLabel);
            if (GUILayout.Button("Create Ocean", GUILayout.Height(28))) CreateOcean();
            EditorGUILayout.HelpBox("W URP Asset włącz Depth Texture, aby piana brzegowa była widoczna.", MessageType.None);
        }

        private void DrawTerrainSection()
        {
            EditorGUILayout.LabelField("Island Terrain", EditorStyles.boldLabel);
            formation = (Formation)EditorGUILayout.EnumPopup("Formation", formation);
            islandSize = EditorGUILayout.FloatField("Island Width", islandSize);
            terrainHeight = EditorGUILayout.FloatField("Terrain Height", terrainHeight);
            heightmapResolution = EditorGUILayout.IntPopup("Heightmap", heightmapResolution, new[] { "257 (Mobile)", "513 (Default)", "1025 (High Detail)" }, new[] { 257, 513, 1025 });
            seed = EditorGUILayout.IntField("Seed", seed);
            if (GUILayout.Button(formation == Formation.Archipelago ? "Create Archipelago" : "Create Island Terrain", GUILayout.Height(30))) CreateFormation();
        }

        private void DrawSculptSection()
        {
            EditorGUILayout.LabelField("Sculpt Brush", EditorStyles.boldLabel);
            sculptMode = (SculptMode)EditorGUILayout.EnumPopup("Mode", sculptMode);
            brushRadius = EditorGUILayout.Slider("Radius", brushRadius, 3f, 160f);
            brushStrength = EditorGUILayout.Slider("Strength", brushStrength, .02f, 1.5f);
            sceneBrushRadius = brushRadius; sceneBrushStrength = brushStrength; sceneMode = (int)sculptMode;
            if (Selection.activeGameObject && Selection.activeGameObject.GetComponent<Terrain>())
                EditorGUILayout.HelpBox("Przeciągnij LPM po wyspie w Scene View. Shift odwraca Raise/Lower.", MessageType.None);
            else
                EditorGUILayout.HelpBox("Zaznacz obiekt Terrain w Hierarchy, aby rzeźbić go w Scene View.", MessageType.Warning);
        }

        private void DrawScatterSection()
        {
            EditorGUILayout.LabelField("Paint & Scatter Objects", EditorStyles.boldLabel);
            scatterPrefabs = DrawPrefabList(scatterPrefabs);
            scatterCount = EditorGUILayout.IntSlider("Object Count", scatterCount, 1, 500);
            maxSlope = EditorGUILayout.Slider("Maximum Slope", maxSlope, 1f, 50f);
            minDryHeight = EditorGUILayout.FloatField("Minimum Height Above Sea", minDryHeight);
            seaLevelForScatter = seaLevel;
            using (new EditorGUI.DisabledScope(scatterPrefabs == null || scatterPrefabs.Length == 0))
                if (GUILayout.Button("Scatter On Selected Terrain")) Scatter();
            if (GUILayout.Button("Build Prop Prefabs")) ArchipelagoPropFactory.BuildAll();
        }

        private static GameObject[] DrawPrefabList(GameObject[] values)
        {
            int count = Mathf.Max(0, EditorGUILayout.IntField("Prefab Slots", values == null ? 0 : values.Length));
            var list = new GameObject[count];
            for (int i = 0; i < count; i++)
            {
                if (values != null && i < values.Length) list[i] = values[i];
                list[i] = (GameObject)EditorGUILayout.ObjectField("Prefab " + (i + 1), list[i], typeof(GameObject), false);
            }
            return list;
        }

        private void CreateOcean()
        {
            EnsureFolders();
            var mat = StylizedOceanTools.CreateMaterial();
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.name = "Ocean_Stylized";
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.position = new Vector3(0, seaLevel, 0);
            var surface = go.AddComponent<StylizedOceanSurface>();
            surface.Configure(mat, Mathf.Max(1000f, islandSize * 10f), seaLevel);
            Undo.RegisterCreatedObjectUndo(go, "Create stylized ocean");
            Selection.activeGameObject = go;
            EditorUtility.DisplayDialog("Archipelago Kit", "Utworzono powierzchnię oceanu na Y=" + seaLevel + ".", "OK");
        }

        private void CreateFormation()
        {
            EnsureFolders();
            if (formation == Formation.Archipelago)
            {
                Vector2[] offsets = { Vector2.zero, new Vector2(islandSize * 1.18f, islandSize * .2f), new Vector2(-islandSize * .8f, islandSize * 1.05f), new Vector2(islandSize * .45f, -islandSize * 1.18f) };
                for (int i = 0; i < offsets.Length; i++) CreateTerrain("Island_" + (i + 1), offsets[i], islandSize * (i == 0 ? 1f : .62f), seed + i * 29);
            }
            else
            {
                float factor = formation == Formation.LargeIsland ? 1.75f : 1f;
                CreateTerrain(formation == Formation.LargeIsland ? "LargeIsland" : "Island", Vector2.zero, islandSize * factor, seed);
            }
        }

        private void CreateTerrain(string objectName, Vector2 offset, float width, int terrainSeed)
        {
            int res = heightmapResolution;
            var data = new TerrainData { heightmapResolution = res, size = new Vector3(width, terrainHeight, width), name = objectName + "_Data" };
            data.SetHeights(0, 0, MakeIslandHeights(res, terrainHeight, terrainSeed));
            string dataPath = AssetDatabase.GenerateUniqueAssetPath("Assets/GameWork/ArchipelagoKit/Generated/Terrain/" + objectName + "_Data.asset");
            AssetDatabase.CreateAsset(data, dataPath);
            var go = Terrain.CreateTerrainGameObject(data);
            go.name = objectName;
            go.transform.position = new Vector3(offset.x - width * .5f, seaLevel - 40f, offset.y - width * .5f);
            var terrain = go.GetComponent<Terrain>();
            terrain.drawInstanced = true;
            terrain.heightmapPixelError = 6;
            terrain.basemapDistance = 1200;
            ConfigureTerrainLayers(data, terrain, seaLevel);
            Undo.RegisterCreatedObjectUndo(go, "Create island terrain");
            Selection.activeGameObject = go;
        }

        private static float[,] MakeIslandHeights(int res, float height, int terrainSeed)
        {
            Random.InitState(terrainSeed);
            float ox = Random.value * 1000f, oz = Random.value * 1000f;
            var h = new float[res, res];
            for (int z = 0; z < res; z++) for (int x = 0; x < res; x++)
            {
                float nx = (x / (float)(res - 1) - .5f) * 2f;
                float nz = (z / (float)(res - 1) - .5f) * 2f;
                float dist = Mathf.Sqrt(nx * nx + nz * nz);
                float profile = Mathf.SmoothStep(0, 1, Mathf.Clamp01((1.20f - dist) * 5.5f));
                float broad = Mathf.PerlinNoise(ox + x / (float)res * 3.1f, oz + z / (float)res * 3.1f);
                float detail = Mathf.PerlinNoise(ox + x / (float)res * 12f, oz + z / (float)res * 12f);
                float elevation = -34f + profile * (38f + broad * 54f) + (detail - .5f) * 5f * profile;
                // Broad, low beach ring transitions into the grassy center.
                float beach = Mathf.Exp(-Mathf.Pow((profile - .40f) * 8f, 2f)) * profile;
                elevation += beach * 2.1f;
                h[z, x] = Mathf.Clamp01((elevation + 40f) / height);
            }
            return h;
        }

        private static void ConfigureTerrainLayers(TerrainData data, Terrain terrain, float seaLevel)
        {
            string folder = "Assets/GameWork/ArchipelagoKit/Generated/Terrain/Layers";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/GameWork/ArchipelagoKit/Generated/Terrain", "Layers");
            var layers = new[]
            {
                MakeTerrainLayer(folder, "Beach_Sand", new Color(.82f, .68f, .43f), 16f),
                MakeTerrainLayer(folder, "Island_Grass", new Color(.32f, .55f, .30f), 20f),
                MakeTerrainLayer(folder, "Island_Rock", new Color(.48f, .50f, .45f), 14f)
            };
            data.terrainLayers = layers;
            int ares = 256;
            var alpha = new float[ares, ares, layers.Length];
            for (int z = 0; z < ares; z++) for (int x = 0; x < ares; x++)
            {
                float nx = x / (float)(ares - 1), nz = z / (float)(ares - 1);
                float y = terrain.transform.position.y + data.GetInterpolatedHeight(nx, nz);
                float relative = y - seaLevel;
                float slope = data.GetSteepness(nx, nz);
                float sandWeight = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-1.2f, 5.0f, relative));
                float rockWeight = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(25f, 52f, slope))
                                 * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(8f, 27f, relative));
                float grassWeight = (1f - sandWeight) * (1f - rockWeight);
                float total = Mathf.Max(.0001f, sandWeight + grassWeight + rockWeight);
                alpha[z, x, 0] = sandWeight / total;
                alpha[z, x, 1] = grassWeight / total;
                alpha[z, x, 2] = rockWeight / total;
            }
            data.alphamapResolution = ares;
            data.SetAlphamaps(0, 0, alpha);
            EditorUtility.SetDirty(data);
        }

        private static TerrainLayer MakeTerrainLayer(string folder, string name, Color baseColor, float tile)
        {
            string texturePath = folder + "/" + name + ".png";
            if (!File.Exists(texturePath))
            {
                const int resolution = 96;
                var tex = new Texture2D(resolution, resolution, TextureFormat.RGB24, true, false);
                tex.name = name;
                var pixels = new Color[resolution * resolution];
                for (int y = 0; y < resolution; y++) for (int x = 0; x < resolution; x++)
                {
                    float n = Mathf.PerlinNoise(x * .09f, y * .09f) * .55f + Mathf.PerlinNoise(x * .23f + 12f, y * .23f) * .25f;
                    pixels[y * resolution + x] = baseColor * Mathf.Lerp(.82f, 1.18f, n);
                }
                tex.SetPixels(pixels); tex.Apply(true, false);
                File.WriteAllBytes(texturePath, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(texturePath);
            }
            string layerPath = folder + "/" + name + ".terrainlayer";
            var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(layerPath);
            if (!layer)
            {
                layer = new TerrainLayer { name = name };
                AssetDatabase.CreateAsset(layer, layerPath);
            }
            layer.diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            layer.tileSize = new Vector2(tile, tile);
            EditorUtility.SetDirty(layer);
            return layer;
        }

        private void Scatter()
        {
            var terrain = Selection.activeGameObject ? Selection.activeGameObject.GetComponent<Terrain>() : null;
            if (!terrain) { EditorUtility.DisplayDialog("Archipelago Kit", "Zaznacz Terrain przed rozmieszczaniem obiektów.", "OK"); return; }
            if (scatterPrefabs == null || scatterPrefabs.Length == 0) return;
            var parent = new GameObject(terrain.name + "_Decorations");
            parent.transform.position = Vector3.zero;
            Undo.RegisterCreatedObjectUndo(parent, "Scatter island objects");
            var data = terrain.terrainData; var size = data.size;
            var rng = new System.Random(seed);
            int created = 0, tries = 0;
            while (created < scatterCount && tries < scatterCount * 15)
            {
                tries++;
                float nx = (float)rng.NextDouble(), nz = (float)rng.NextDouble();
                float steep = data.GetSteepness(nx, nz);
                if (steep > maxSlope) continue;
                var world = terrain.transform.position + new Vector3(nx * size.x, data.GetInterpolatedHeight(nx, nz), nz * size.z);
                if (world.y < seaLevel + minDryHeight) continue;
                var prefab = scatterPrefabs[rng.Next(scatterPrefabs.Length)];
                if (!prefab) continue;
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                Undo.RegisterCreatedObjectUndo(instance, "Place island prop");
                instance.transform.SetParent(parent.transform, true);
                instance.transform.position = world;
                instance.transform.rotation = Quaternion.Euler(0, (float)rng.NextDouble() * 360f, 0);
                float scale = Mathf.Lerp(.78f, 1.22f, (float)rng.NextDouble());
                instance.transform.localScale = Vector3.one * scale;
                created++;
            }
            EditorUtility.DisplayDialog("Archipelago Kit", "Rozmieszczono " + created + " obiektów na suchej powierzchni.", "OK");
        }

        private static void EnsureFolders()
        {
            foreach (string p in new[] { "Assets/GameWork", "Assets/GameWork/ArchipelagoKit", "Assets/GameWork/ArchipelagoKit/Generated", "Assets/GameWork/ArchipelagoKit/Generated/Materials", "Assets/GameWork/ArchipelagoKit/Generated/Terrain" })
                if (!AssetDatabase.IsValidFolder(p))
                {
                    string parent = Path.GetDirectoryName(p).Replace('\\', '/');
                    AssetDatabase.CreateFolder(parent, Path.GetFileName(p));
                }
        }

        [InitializeOnLoadMethod]
        private static void RegisterSceneBrush() => SceneView.duringSceneGui += OnSceneGUI;

        private static void OnSceneGUI(SceneView view)
        {
            var window = activeWindow;
            var terrain = Selection.activeGameObject ? Selection.activeGameObject.GetComponent<Terrain>() : null;
            if (window == null || !terrain || Event.current.alt) return;
            var e = Event.current;
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (!Physics.Raycast(ray, out var hit, 100000f) || hit.collider.gameObject != terrain.gameObject) return;
            Handles.color = new Color(.25f, .9f, .74f, .85f);
            Handles.DrawWireDisc(hit.point, Vector3.up, sceneBrushRadius);
            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
            {
                if (e.type == EventType.MouseDown)
                {
                    undoRegistered = false;
                    flattenCaptured = false;
                }
                Sculpt(terrain, hit.point, e.shift);
                e.Use();
            }
            if (e.type == EventType.MouseUp && e.button == 0)
            {
                terrain.terrainData.SyncHeightmap();
                EditorUtility.SetDirty(terrain.terrainData);
                undoRegistered = false;
            }
        }

        private static void Sculpt(Terrain terrain, Vector3 point, bool invert)
        {
            var data = terrain.terrainData;
            int res = data.heightmapResolution;
            Vector3 local = point - terrain.transform.position;
            int cx = Mathf.RoundToInt(local.x / data.size.x * (res - 1));
            int cz = Mathf.RoundToInt(local.z / data.size.z * (res - 1));
            int radius = Mathf.Max(1, Mathf.RoundToInt(sceneBrushRadius / data.size.x * (res - 1)));
            int x0 = Mathf.Clamp(cx - radius, 0, res - 1), x1 = Mathf.Clamp(cx + radius, 0, res - 1);
            int z0 = Mathf.Clamp(cz - radius, 0, res - 1), z1 = Mathf.Clamp(cz + radius, 0, res - 1);
            int width = x1 - x0 + 1, depth = z1 - z0 + 1;
            var heights = data.GetHeights(x0, z0, width, depth);
            int sampleX0 = Mathf.Max(0, x0 - 1), sampleZ0 = Mathf.Max(0, z0 - 1);
            int sampleX1 = Mathf.Min(res - 1, x1 + 1), sampleZ1 = Mathf.Min(res - 1, z1 + 1);
            var smoothSource = data.GetHeights(sampleX0, sampleZ0, sampleX1 - sampleX0 + 1, sampleZ1 - sampleZ0 + 1);
            if (!undoRegistered) { Undo.RegisterCompleteObjectUndo(data, "Sculpt island terrain"); undoRegistered = true; }
            float flatten = flattenTarget;
            if (sceneMode == (int)SculptMode.Flatten && !flattenCaptured)
            {
                flatten = (point.y - terrain.transform.position.y) / data.size.y;
                flattenTarget = flatten; flattenCaptured = true;
            }
            for (int z = 0; z < depth; z++) for (int x = 0; x < width; x++)
            {
                int gx = x0 + x, gz = z0 + z;
                float wx = gx / (float)(res - 1) * data.size.x, wz = gz / (float)(res - 1) * data.size.z;
                float distance = Vector2.Distance(new Vector2(wx, wz), new Vector2(local.x, local.z));
                if (distance > sceneBrushRadius) continue;
                float falloff = 1f - distance / sceneBrushRadius;
                falloff = falloff * falloff * (3f - 2f * falloff);
                float old = heights[z, x];
                switch ((SculptMode)sceneMode)
                {
                    case SculptMode.Raise: heights[z, x] = Mathf.Clamp01(old + (invert ? -1f : 1f) * sceneBrushStrength * .0018f * falloff); break;
                    case SculptMode.Lower: heights[z, x] = Mathf.Clamp01(old - (invert ? -1f : 1f) * sceneBrushStrength * .0018f * falloff); break;
                    case SculptMode.Flatten: heights[z, x] = Mathf.Lerp(old, flatten, sceneBrushStrength * .12f * falloff); break;
                    case SculptMode.Smooth:
                        float sum = 0; int n = 0;
                        for (int dz = -1; dz <= 1; dz++) for (int dx = -1; dx <= 1; dx++)
                        {
                            int sx = Mathf.Clamp(gx + dx, 0, res - 1), sz = Mathf.Clamp(gz + dz, 0, res - 1);
                            sum += smoothSource[sz - sampleZ0, sx - sampleX0]; n++;
                        }
                        heights[z, x] = Mathf.Lerp(old, sum / n, sceneBrushStrength * .12f * falloff); break;
                }
            }
            data.SetHeightsDelayLOD(x0, z0, heights);
            terrain.Flush();
            SceneView.RepaintAll();
        }
    }
}
#endif
