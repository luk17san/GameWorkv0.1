#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GameWork.TerrainEditor
{
    public sealed class IslandTerrainEditor : EditorWindow
    {
        private enum Landform { Island, Archipelago, Continent }
        private enum BrushMode { Raise, Lower, Smooth, Flatten }

        private Landform landform = Landform.Island;
        private BrushMode brushMode = BrushMode.Raise;
        private Terrain terrain;
        private int resolution = 513;
        private Vector3 terrainSize = new Vector3(4000f, 700f, 4000f);
        private int seed = 1247;
        private float roughness = 0.12f;
        private float coastWidth = 0.12f;
        private int islandCount = 7;
        private float brushRadius = 120f;
        private float brushStrength = 0.35f;
        private float flattenHeight = 0.08f;
        private bool sculptingEnabled;
        private bool hasBrushHit;
        private Vector3 brushHit;

        [MenuItem("Window/GameWork/Island & Continent Editor")]
        private static void Open()
        {
            GetWindow<IslandTerrainEditor>("Island Editor");
        }

        private void OnEnable() { SceneView.duringSceneGui += OnSceneGUI; }
        private void OnDisable() { SceneView.duringSceneGui -= OnSceneGUI; }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("GameWork • Edytor wysp i lądów", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Generuj ukształtowanie terenu na standardowym Unity Terrain. Poziom 0 terenu jest poziomem oceanu — ustaw ocean w scenie na tej samej wysokości.", MessageType.Info);

            terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true);
            if (GUILayout.Button("Utwórz nowy Terrain")) CreateTerrain();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Generowanie lądu", EditorStyles.boldLabel);
            landform = (Landform)EditorGUILayout.EnumPopup("Typ lądu", landform);
            terrainSize = EditorGUILayout.Vector3Field("Rozmiar (X, wysokość, Z)", terrainSize);
            resolution = EditorGUILayout.IntPopup("Rozdzielczość", resolution,
                new[] { "129", "257", "513", "1025" }, new[] { 129, 257, 513, 1025 });
            seed = EditorGUILayout.IntField("Ziarno losowości", seed);
            roughness = EditorGUILayout.Slider("Nieregularność brzegu", roughness, 0f, 0.35f);
            coastWidth = EditorGUILayout.Slider("Szerokość łagodnego brzegu", coastWidth, 0.03f, 0.35f);
            if (landform == Landform.Archipelago)
                islandCount = EditorGUILayout.IntSlider("Liczba wysp", islandCount, 2, 20);

            using (new EditorGUI.DisabledScope(terrain == null))
            {
                if (GUILayout.Button("Generuj ląd", GUILayout.Height(30))) GenerateLand();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Rzeźbienie w widoku sceny", EditorStyles.boldLabel);
            sculptingEnabled = EditorGUILayout.Toggle("Włącz pędzel", sculptingEnabled);
            brushMode = (BrushMode)EditorGUILayout.EnumPopup("Tryb", brushMode);
            brushRadius = EditorGUILayout.Slider("Promień", brushRadius, 5f, 500f);
            brushStrength = EditorGUILayout.Slider("Siła", brushStrength, 0.01f, 1f);
            if (brushMode == BrushMode.Flatten)
                flattenHeight = EditorGUILayout.Slider("Wysokość docelowa", flattenHeight, 0f, 1f);

            EditorGUILayout.HelpBox("Włącz pędzel, najedź kursorem na Terrain i przeciągnij lewym przyciskiem. Wygładzanie i spłaszczanie działają w obrębie pędzla. Zmiany można cofnąć przez Ctrl+Z.", MessageType.None);
        }

        private void CreateTerrain()
        {
            string assetPath = EditorUtility.SaveFilePanelInProject(
                "Zapisz dane Terrain", "GameWork_IslandTerrainData", "asset",
                "Wskaż miejsce zapisu danych wysokości terenu.");
            if (string.IsNullOrEmpty(assetPath)) return;

            var data = new TerrainData { heightmapResolution = resolution, size = terrainSize };
            AssetDatabase.CreateAsset(data, assetPath);
            AssetDatabase.SaveAssets();
            var go = Terrain.CreateTerrainGameObject(data);
            go.name = "GameWork_Terrain";
            Undo.RegisterCreatedObjectUndo(go, "Create GameWork Terrain");
            terrain = go.GetComponent<Terrain>();
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
        }

        private void GenerateLand()
        {
            if (terrain == null) return;
            var data = terrain.terrainData;
            Undo.RegisterCompleteObjectUndo(data, "Generate island terrain");
            data.heightmapResolution = resolution;
            data.size = terrainSize;

            int n = data.heightmapResolution;
            var heights = new float[n, n];
            var random = new System.Random(seed);
            Vector2[] centers;
            Vector2[] radii;
            int count = landform == Landform.Archipelago ? islandCount : 1;
            centers = new Vector2[count];
            radii = new Vector2[count];

            if (landform == Landform.Continent)
            {
                centers[0] = new Vector2(0.5f, 0.5f);
                radii[0] = new Vector2(0.47f, 0.42f);
            }
            else if (landform == Landform.Island)
            {
                centers[0] = new Vector2(0.5f, 0.5f);
                radii[0] = new Vector2(0.34f, 0.30f);
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    float angle = (Mathf.PI * 2f * i / count) + (float)random.NextDouble() * 0.55f;
                    float ring = 0.08f + (float)random.NextDouble() * 0.11f;
                    centers[i] = new Vector2(0.5f + Mathf.Cos(angle) * ring, 0.5f + Mathf.Sin(angle) * ring);
                    float size = 0.10f + (float)random.NextDouble() * 0.075f;
                    radii[i] = new Vector2(size * (0.8f + (float)random.NextDouble() * 0.5f), size);
                }
            }

            float coast = Mathf.Max(0.001f, coastWidth);
            for (int z = 0; z < n; z++)
            {
                float v = z / (float)(n - 1);
                for (int x = 0; x < n; x++)
                {
                    float u = x / (float)(n - 1);
                    float best = 0f;
                    for (int i = 0; i < count; i++)
                    {
                        float dx = (u - centers[i].x) / radii[i].x;
                        float dz = (v - centers[i].y) / radii[i].y;
                        float distance = Mathf.Sqrt(dx * dx + dz * dz);
                        float n1 = Mathf.PerlinNoise(u * 18f + seed * 0.013f, v * 18f + seed * 0.021f) - 0.5f;
                        float n2 = Mathf.PerlinNoise(u * 47f + seed * 0.031f, v * 47f + seed * 0.017f) - 0.5f;
                        float distorted = distance - n1 * roughness * 1.7f - n2 * roughness * 0.55f;
                        float land = 1f - Mathf.SmoothStep(1f - coast, 1f + coast, distorted);
                        float interior = Mathf.Clamp01(1f - Mathf.Pow(Mathf.Clamp01(distorted), 1.6f));
                        float height = land * (0.10f + interior * 0.58f);
                        best = Mathf.Max(best, height);
                    }
                    // Broad low-frequency variation creates ridges and valleys away from the coast.
                    if (best > 0f)
                    {
                        float relief = (Mathf.PerlinNoise(u * 8f + seed, v * 8f - seed) - 0.5f) * 0.18f;
                        best = Mathf.Clamp01(best + relief * Mathf.Clamp01(best * 3f));
                    }
                    heights[z, x] = best;
                }
            }
            data.SetHeights(0, 0, heights);
            terrain.Flush();
            EditorUtility.SetDirty(data);
            SceneView.RepaintAll();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!sculptingEnabled || terrain == null) return;
            Event e = Event.current;
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            var collider = terrain.GetComponent<TerrainCollider>();
            RaycastHit hit = default;
            hasBrushHit = collider != null && collider.Raycast(ray, out hit, 100000f);

            if (hasBrushHit)
            {
                brushHit = hit.point;
                Handles.color = new Color(0.15f, 0.9f, 0.75f, 0.9f);
                Handles.DrawWireDisc(brushHit, Vector3.up, brushRadius);
                if (e.type == EventType.Layout && e.button == 0) HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                if (e.type == EventType.MouseDown && e.button == 0) { Sculpt(e, 1f); e.Use(); }
                else if (e.type == EventType.MouseDrag && e.button == 0) { Sculpt(e, Mathf.Clamp(e.delta.magnitude / 12f, 0.15f, 1f)); e.Use(); }
                else if (e.type == EventType.MouseUp && e.button == 0) { terrain.terrainData.SyncHeightmap(); e.Use(); }
            }
            if (e.type == EventType.Repaint) sceneView.Repaint();
        }

        private void Sculpt(Event e, float dragFactor)
        {
            var data = terrain.terrainData;
            int resolutionNow = data.heightmapResolution;
            Vector3 local = brushHit - terrain.transform.position;
            int cx = Mathf.RoundToInt(local.x / data.size.x * (resolutionNow - 1));
            int cz = Mathf.RoundToInt(local.z / data.size.z * (resolutionNow - 1));
            int radiusX = Mathf.Max(1, Mathf.CeilToInt(brushRadius / data.size.x * (resolutionNow - 1)));
            int radiusZ = Mathf.Max(1, Mathf.CeilToInt(brushRadius / data.size.z * (resolutionNow - 1)));
            int x0 = Mathf.Clamp(cx - radiusX, 0, resolutionNow - 1);
            int z0 = Mathf.Clamp(cz - radiusZ, 0, resolutionNow - 1);
            int x1 = Mathf.Clamp(cx + radiusX, 0, resolutionNow - 1);
            int z1 = Mathf.Clamp(cz + radiusZ, 0, resolutionNow - 1);
            int width = x1 - x0 + 1, height = z1 - z0 + 1;
            float[,] patch = data.GetHeights(x0, z0, width, height);
            float[,] source = (float[,])patch.Clone();
            Undo.RegisterCompleteObjectUndo(data, "Sculpt terrain");
            float amount = brushStrength * dragFactor * 0.012f;

            for (int z = 0; z < height; z++)
            for (int x = 0; x < width; x++)
            {
                int gx = x0 + x, gz = z0 + z;
                float dx = (gx - cx) / (float)radiusX;
                float dz = (gz - cz) / (float)radiusZ;
                float distance = Mathf.Sqrt(dx * dx + dz * dz);
                if (distance > 1f) continue;
                float falloff = 1f - Mathf.SmoothStep(0f, 1f, distance);
                if (brushMode == BrushMode.Raise) patch[z, x] = Mathf.Clamp01(source[z, x] + amount * falloff);
                else if (brushMode == BrushMode.Lower) patch[z, x] = Mathf.Clamp01(source[z, x] - amount * falloff);
                else if (brushMode == BrushMode.Flatten) patch[z, x] = Mathf.Lerp(source[z, x], flattenHeight, brushStrength * dragFactor * falloff * 0.25f);
                else
                {
                    float sum = 0f; int samples = 0;
                    for (int oz = -1; oz <= 1; oz++)
                    for (int ox = -1; ox <= 1; ox++)
                    {
                        int sx = Mathf.Clamp(x + ox, 0, width - 1), sz = Mathf.Clamp(z + oz, 0, height - 1);
                        sum += source[sz, sx]; samples++;
                    }
                    patch[z, x] = Mathf.Lerp(source[z, x], sum / samples, brushStrength * dragFactor * falloff);
                }
            }
            data.SetHeightsDelayLOD(x0, z0, patch);
            EditorUtility.SetDirty(data);
            e.Use();
            SceneView.RepaintAll();
        }
    }
}
#endif
