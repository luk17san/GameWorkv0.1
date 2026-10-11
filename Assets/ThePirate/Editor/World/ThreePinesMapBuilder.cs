#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

namespace ThePirate.Editor.World
{
    [InitializeOnLoad]
    public static class ThreePinesMapBuilder
    {
        const string Root = "Assets/ThePirate/World/ThreePines";
        const string ScenePath = "Assets/ThePirate/Scenes/WybrzezeTrzechSosen.unity";
        const string Request = "Temp/ThreePines.build";
        [Serializable] public class Outline { public string name; public Vector2[] points; }
        [Serializable] public class Map { public float width = 3000; public float depth = 2000; public Outline[] outlines; }
        static ThreePinesMapBuilder() { EditorApplication.update += PendingBuild; }
        static void PendingBuild()
        {
            if (!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            File.Delete(Request);
            try { Build(); }
            catch (Exception e) { File.WriteAllText("Temp/ThreePines.result", e.ToString()); Debug.LogException(e); }
        }
        [MenuItem("GameWork/Map/Wybrzeze Trzech Sosen/Generate new scene")]
        public static void Build()
        {
            // Always create new assets; regenerating preserves sculpted previous versions.
            var map = JsonUtility.FromJson<Map>(File.ReadAllText(Root + "/Coastlines.json"));
            if (map == null || map.outlines == null || map.width <= 0 || map.depth <= 0) throw new InvalidDataException("Invalid coastline map");
            foreach (var o in map.outlines) if (o.points == null || o.points.Length < 3) throw new InvalidDataException(o.name);
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                Directory.CreateDirectory(Root + "/Generated"); AssetDatabase.Refresh();
                var data = new TerrainData { name = "ThreePines_Terrain", heightmapResolution = 1025, size = new Vector3(map.width, 220, map.depth) };
                var polygons = new Vector2[map.outlines.Length][];
                for (int i = 0; i < polygons.Length; i++)
                {
                    polygons[i] = new Vector2[map.outlines[i].points.Length];
                    for (int j = 0; j < polygons[i].Length; j++)
                    {
                        var p = map.outlines[i].points[j];
                        polygons[i][j] = new Vector2((p.x - 40) / 1455f * map.width, (970 - p.y) / 930f * map.depth);
                    }
                }
                var heights = new float[1025, 1025];
                for (int z = 0; z < 1025; z++) for (int x = 0; x < 1025; x++)
                {
                    var p = new Vector2(x / 1024f * map.width, z / 1024f * map.depth);
                    float distance = -100000;
                    foreach (var polygon in polygons) distance = Mathf.Max(distance, SignedDistance(p, polygon));
                    float h;
                    if (distance < 0) h = Mathf.Max(-30, distance * .55f);
                    else
                    {
                        float core = Mathf.SmoothStep(0, 1, Mathf.Clamp01((distance - 12) / 135));
                        float noise = Mathf.PerlinNoise(p.x * .005f + 17, p.y * .005f + 31);
                        float ridge = 1 - Mathf.Abs(2 * Mathf.PerlinNoise(p.x * .009f, p.y * .009f + 73) - 1);
                        h = Mathf.Min(distance * .16f, 9) + core * (22 + 78 * noise + 30 * ridge);
                    }
                    heights[z, x] = (h + 35) / 220;
                }
                data.SetHeights(0, 0, heights);
                string dataPath = AssetDatabase.GenerateUniqueAssetPath(Root + "/Generated/ThreePines_Terrain.asset");
                AssetDatabase.CreateAsset(data, dataPath);
                var land = Terrain.CreateTerrainGameObject(data); land.name = "Lad_i_wyspy";
                land.transform.position = new Vector3(-map.width / 2, -35, -map.depth / 2);
                var terrain = land.GetComponent<Terrain>(); terrain.drawInstanced = true; terrain.heightmapPixelError = 3; terrain.basemapDistance = 5000;
                terrain.materialTemplate = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Terrain.mat");
                if (!terrain.materialTemplate)
                {
                    var shader = Shader.Find("Universal Render Pipeline/Terrain/Lit");
                    if (!shader) throw new InvalidOperationException("URP Terrain shader missing");
                    var material = new Material(shader);
                    AssetDatabase.CreateAsset(material, Root + "/Terrain.mat"); terrain.materialTemplate = material;
                }
                Paint(data);
                var ocean = GameObject.CreatePrimitive(PrimitiveType.Plane); ocean.name = "Ocean_Y0";
                UnityEngine.Object.DestroyImmediate(ocean.GetComponent<Collider>());
                ocean.transform.localScale = new Vector3((map.width + 400) / 10, 1, (map.depth + 400) / 10);
                ocean.GetComponent<Renderer>().sharedMaterial = OceanMaterial();
                var light = new GameObject("Slonce").AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.3f; light.transform.rotation = Quaternion.Euler(50, -30, 0);
                RenderSettings.ambientLight = new Color(.55f, .6f, .65f);
                var camera = new GameObject("Main Camera").AddComponent<Camera>(); camera.tag = "MainCamera";
                camera.transform.position = new Vector3(0, 2400, 0); camera.transform.rotation = Quaternion.Euler(90, 0, 0);
                camera.orthographic = true; camera.orthographicSize = 1100; camera.farClipPlane = 5000;
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.025f,.16f,.22f);
                camera.gameObject.AddComponent<UniversalAdditionalCameraData>().requiresDepthTexture = true;
                camera.gameObject.AddComponent<AudioListener>();
                var marker = new GameObject("Punkt_startowy_statku"); marker.transform.position = new Vector3(-350, 1, 0);
                string path = AssetDatabase.GenerateUniqueAssetPath(ScenePath);
                if (!EditorSceneManager.SaveScene(scene, path)) throw new IOException("Could not save scene");
                AssetDatabase.SaveAssets();
                // Collider raycasts check actual generated geometry in Editor, not Play Mode.
                Physics.SyncTransforms();
                int passed = 0;
                foreach (var p in new[] {new Vector2(345,380),new Vector2(1100,430),new Vector2(1340,820)})
                {
                    Vector3 origin = new Vector3((p.x-40)/1455*map.width-map.width/2,500,(970-p.y)/930*map.depth-map.depth/2);
                    if (land.GetComponent<TerrainCollider>().Raycast(new Ray(origin,Vector3.down),out var hit,1000) && hit.point.y > 0) passed++;
                }
                File.WriteAllText("Temp/ThreePines.result", "PASS scene=" + path + " terrain=" + dataPath + " colliders="+passed+"/3; Editor generation only; Play Mode pending");
                Debug.Log("[ThreePines] Scene generated: " + path);
            }
            finally { EditorSceneManager.CloseScene(scene, true); if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous); }
        }
        static float SignedDistance(Vector2 p, Vector2[] polygon)
        {
            bool inside = false; float d2 = float.MaxValue;
            for (int i=0,j=polygon.Length-1;i<polygon.Length;j=i++)
            {
                var a=polygon[j]; var b=polygon[i]; var edge=b-a;
                var delta=p-(a+edge*Mathf.Clamp01(Vector2.Dot(p-a,edge)/Mathf.Max(edge.sqrMagnitude,.0001f)));
                d2=Mathf.Min(d2,delta.sqrMagnitude);
                if ((a.y>p.y)!=(b.y>p.y) && p.x<(b.x-a.x)*(p.y-a.y)/(b.y-a.y)+a.x) inside=!inside;
            }
            return Mathf.Sqrt(d2)*(inside?1:-1);
        }
        static Material OceanMaterial()
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Ocean.mat");
            if (!m)
            {
                var shader = Shader.Find("ThePirate/ThreePines/Ocean");
                if (!shader) throw new InvalidOperationException("ThreePines ocean shader missing");
                m = new Material(shader); AssetDatabase.CreateAsset(m, Root + "/Ocean.mat");
            }
            m.SetFloat("_Opacity",1); m.SetFloat("_DepthRange",28); m.SetFloat("_WaveHeight",.18f);
            m.SetFloat("_WaveScale",.4f); m.SetFloat("_SunSpecular",.35f); m.SetFloat("_FoamStrength",.65f);
            m.SetColor("_DeepColor",new Color(.025f,.17f,.25f));
            m.SetColor("_ShallowColor",new Color(.13f,.48f,.48f));
            m.SetColor("_FoamColor",new Color(.82f,.93f,.9f)); EditorUtility.SetDirty(m);
            return m;
        }
        [MenuItem("GameWork/Map/Wybrzeze Trzech Sosen/Refresh ocean material")]
        public static void RefreshOcean()
        {
            var scene=SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded) throw new InvalidOperationException("Open WybrzezeTrzechSosen scene first");
            foreach(var root in scene.GetRootGameObjects()) if(root.name=="Ocean_Y0") root.GetComponent<Renderer>().sharedMaterial=OceanMaterial();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        }
        static void Paint(TerrainData data)
        {
            string folder="Assets/Models/ArchipelagoKit/Generated/Terrain/Layers/";
            var sand=AssetDatabase.LoadAssetAtPath<TerrainLayer>(folder+"Beach_Sand.terrainlayer");
            var grass=AssetDatabase.LoadAssetAtPath<TerrainLayer>(folder+"Island_Grass.terrainlayer");
            var rock=AssetDatabase.LoadAssetAtPath<TerrainLayer>(folder+"Island_Rock.terrainlayer");
            if (!sand || !grass || !rock) throw new InvalidOperationException("Missing terrain layers");
            data.terrainLayers=new[]{sand,grass,rock}; data.alphamapResolution=512;
            var alpha=new float[512,512,3];
            for(int z=0;z<512;z++) for(int x=0;x<512;x++)
            {
                float nx=x/511f,nz=z/511f,h=data.GetInterpolatedHeight(nx,nz)-35;
                float s=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2,12,h));
                float r=Mathf.Max(Mathf.InverseLerp(24,48,data.GetSteepness(nx,nz)),Mathf.InverseLerp(55,105,h))*(1-s);
                alpha[z,x,0]=s;alpha[z,x,1]=1-s-r;alpha[z,x,2]=r;
            }
            data.SetAlphamaps(0,0,alpha);
        }
    }
}
#endif
