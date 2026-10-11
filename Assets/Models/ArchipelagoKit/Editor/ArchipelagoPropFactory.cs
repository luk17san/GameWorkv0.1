#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameWork.ArchipelagoKit.Editor
{
    internal static class ArchipelagoPropFactory
    {
        private const string Root = "Assets/GameWork/ArchipelagoKit/Generated";
        private static Material wood, darkWood, leaf, red, sand, stone, plaster, roof, glass;

        [MenuItem("GameWork/Archipelago/Build Prop Prefabs")]
        internal static void BuildAll()
        {
            EnsureFolders();
            wood = Mat("Wood_Warm", new Color(.38f, .20f, .10f));
            darkWood = Mat("Wood_Shadow", new Color(.19f, .13f, .10f));
            leaf = Mat("Leaf_Teal", new Color(.14f, .46f, .31f));
            red = Mat("Roof_Terracotta", new Color(.66f, .20f, .15f));
            sand = Mat("Sand", new Color(.84f, .68f, .40f));
            stone = Mat("Stone_Cool", new Color(.44f, .51f, .51f));
            plaster = Mat("Plaster_Cream", new Color(.83f, .73f, .51f));
            roof = Mat("Roof_DarkWood", new Color(.31f, .18f, .15f));
            glass = Mat("Lantern_Gold", new Color(1f, .73f, .26f));

            SavePrefab("Palm_Tropical", MakePalm());
            SavePrefab("Rock_Mossy", MakeRock());
            SavePrefab("Cottage_SlavicHarbor", MakeCottage());
            SavePrefab("Pier_Small", MakePier());
            SavePrefab("Lighthouse_Small", MakeLighthouse());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Archipelago Kit", "Utworzono 5 prefabrykatów w:\n" + Root, "OK");
        }

        private static GameObject MakePalm()
        {
            var root = NewRoot("Palm_Tropical");
            Part(root, PrimitiveType.Cylinder, "Trunk", new Vector3(0, 1.45f, 0), new Vector3(.22f, 1.5f, .22f), wood);
            var crown = new Vector3(0, 3f, 0);
            for (int i = 0; i < 7; i++)
            {
                float a = i * Mathf.PI * 2f / 7f;
                var frond = Part(root, PrimitiveType.Sphere, "Frond_" + i, crown + new Vector3(Mathf.Cos(a) * .72f, -.18f, Mathf.Sin(a) * .72f), new Vector3(.88f, .10f, .28f), leaf);
                frond.transform.rotation = Quaternion.Euler(0, -a * Mathf.Rad2Deg, 0);
            }
            Part(root, PrimitiveType.Sphere, "Coconut", crown, new Vector3(.32f, .24f, .32f), sand);
            return root;
        }

        private static GameObject MakeRock()
        {
            var root = NewRoot("Rock_Mossy");
            var body = Part(root, PrimitiveType.Sphere, "Stone", new Vector3(0, .42f, 0), new Vector3(1.2f, .72f, .92f), stone);
            body.transform.rotation = Quaternion.Euler(8, 24, -6);
            Part(root, PrimitiveType.Sphere, "Moss", new Vector3(.12f, .79f, .04f), new Vector3(.74f, .15f, .56f), leaf);
            return root;
        }

        private static GameObject MakeCottage()
        {
            var root = NewRoot("Cottage_SlavicHarbor");
            Part(root, PrimitiveType.Cube, "Walls", new Vector3(0, .72f, 0), new Vector3(2.5f, 1.45f, 2.1f), plaster);
            var roofA = Part(root, PrimitiveType.Cube, "Roof_Left", new Vector3(-.48f, 1.82f, 0), new Vector3(1.48f, .16f, 2.42f), roof);
            roofA.transform.rotation = Quaternion.Euler(0, 0, -35);
            var roofB = Part(root, PrimitiveType.Cube, "Roof_Right", new Vector3(.48f, 1.82f, 0), new Vector3(1.48f, .16f, 2.42f), red);
            roofB.transform.rotation = Quaternion.Euler(0, 0, 35);
            Part(root, PrimitiveType.Cube, "Door", new Vector3(0, .52f, -1.06f), new Vector3(.42f, 1.02f, .08f), darkWood);
            Part(root, PrimitiveType.Cube, "Beam", new Vector3(0, 1.42f, -1.11f), new Vector3(2.58f, .12f, .13f), wood);
            Part(root, PrimitiveType.Cube, "Red_Emblem", new Vector3(0, 1.18f, -1.13f), new Vector3(.2f, .2f, .04f), red);
            return root;
        }

        private static GameObject MakePier()
        {
            var root = NewRoot("Pier_Small");
            for (int i = 0; i < 5; i++)
            {
                float z = i * .62f;
                Part(root, PrimitiveType.Cube, "Deck_" + i, new Vector3(0, .34f, z), new Vector3(2.1f, .15f, .57f), i % 2 == 0 ? wood : darkWood);
                foreach (float x in new[] { -0.87f, 0.87f })
                    Part(root, PrimitiveType.Cylinder, "Post_" + i + "_" + x, new Vector3(x, .03f, z), new Vector3(.12f, .48f, .12f), darkWood);
            }
            return root;
        }

        private static GameObject MakeLighthouse()
        {
            var root = NewRoot("Lighthouse_Small");
            Part(root, PrimitiveType.Cylinder, "Tower", new Vector3(0, 2.0f, 0), new Vector3(.72f, 2f, .72f), plaster);
            Part(root, PrimitiveType.Cylinder, "Red_Band", new Vector3(0, 2.2f, 0), new Vector3(.75f, .18f, .75f), red);
            Part(root, PrimitiveType.Cube, "Lantern_Room", new Vector3(0, 4.12f, 0), new Vector3(.98f, .65f, .98f), glass);
            Part(root, PrimitiveType.Cylinder, "Roof", new Vector3(0, 4.62f, 0), new Vector3(1.18f, .28f, 1.18f), red);
            Part(root, PrimitiveType.Cylinder, "Top", new Vector3(0, 4.91f, 0), new Vector3(.18f, .18f, .18f), darkWood);
            return root;
        }

        private static GameObject NewRoot(string name) => new GameObject(name);

        private static GameObject Part(GameObject parent, PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent.transform, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            part.isStatic = true;
            return part;
        }

        private static Material Mat(string name, Color color)
        {
            string path = Root + "/Materials/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m)
            {
                Shader s = Shader.Find("Universal Render Pipeline/Lit");
                if (!s) s = Shader.Find("Standard");
                m = new Material(s) { name = name, color = color };
                AssetDatabase.CreateAsset(m, path);
            }
            else m.color = color;
            EditorUtility.SetDirty(m);
            return m;
        }

        private static void SavePrefab(string name, GameObject root)
        {
            string path = Root + "/Prefabs/" + name + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void EnsureFolders()
        {
            foreach (string p in new[] { "Assets/GameWork", "Assets/GameWork/ArchipelagoKit", Root, Root + "/Materials", Root + "/Prefabs" })
                if (!AssetDatabase.IsValidFolder(p))
                {
                    string parent = Path.GetDirectoryName(p).Replace('\\', '/');
                    string leaf = Path.GetFileName(p);
                    AssetDatabase.CreateFolder(parent, leaf);
                }
        }
    }
}
#endif
