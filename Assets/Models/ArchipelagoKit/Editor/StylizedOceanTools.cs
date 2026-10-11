#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using GameWork.ArchipelagoKit.Water;

namespace GameWork.ArchipelagoKit.Editor
{
    public static class StylizedOceanTools
    {
        private const string ShaderName = "GameWork/World/StylizedOcean";

        [MenuItem("GameWork/Archipelago/Water/Upgrade Selected Ocean (Keep Colours)")]
        private static void UpgradeSelectedOcean()
        {
            var go = Selection.activeGameObject;
            var renderer = go ? go.GetComponent<MeshRenderer>() : null;
            var mat = renderer ? renderer.sharedMaterial : null;
            if (!mat || mat.shader.name != ShaderName)
            {
                EditorUtility.DisplayDialog("Ocean", "Zaznacz ocean używający materiału StylizedOcean.", "OK");
                return;
            }
            Undo.RecordObject(mat, "Upgrade wave crests and foam");
            mat.SetFloat("_WaveHeading", 35f);
            mat.SetFloat("_DirectionSpread", 48f);
            mat.SetFloat("_DirectionPeriod", 120f);
            mat.SetFloat("_DirectionVariation", .65f);
            mat.SetFloat("_CrestSharpness", 2.8f);
            mat.SetFloat("_CrestStrength", 1.8f);
            mat.SetFloat("_FoamBreakup", .55f);
            mat.SetFloat("_FoamPersistence", .65f);
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            SceneView.RepaintAll();
        }

        [MenuItem("GameWork/Archipelago/Water/Add Wake To Selected Ship")]
        private static void AddWakeToShip()
        {
            var ship = Selection.activeGameObject;
            if (!ship || EditorUtility.IsPersistent(ship))
            {
                EditorUtility.DisplayDialog("Kilwater", "Zaznacz poruszający się obiekt statku w Hierarchy.", "OK");
                return;
            }
            var shader = Shader.Find("GameWork/World/ShipWake");
            if (!shader) { Debug.LogError("Brak ShipWake.shader. Zaimportuj cały folder Runtime/Water."); return; }
            string shaderPath = AssetDatabase.GetAssetPath(shader).Replace('\\', '/');
            const string suffix = "/Runtime/Water/ShipWake.shader";
            if (!shaderPath.EndsWith(suffix, StringComparison.Ordinal)) return;
            string root = shaderPath.Substring(0, shaderPath.Length - suffix.Length);
            var foam = LoadDataMap(root + "/Runtime/Water/Textures/FoamPattern.png");
            EnsureFolder(root + "/Generated/Materials");
            string path = root + "/Generated/Materials/ShipWake.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!material)
            {
                material = new Material(shader) { name = "ShipWake" };
                material.SetTexture("_FoamMap", foam);
                AssetDatabase.CreateAsset(material, path);
            }
            var wake = ship.GetComponent<ShipWakeEmitter>();
            if (!wake) wake = Undo.AddComponent<ShipWakeEmitter>(ship);
            Undo.RecordObject(wake, "Assign ship wake material");
            wake.wakeMaterial = material;
            EditorUtility.SetDirty(wake);
            PrefabUtility.RecordPrefabInstancePropertyModifications(wake);
            if (ship.scene.IsValid()) EditorSceneManager.MarkSceneDirty(ship.scene);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Kilwater", "Dodano ShipWakeEmitter. Ustaw Ship Width i Local Stern Offset, a następnie przypisz Ocean. Efekt działa w Play Mode podczas ruchu statku.", "OK");
        }

        [MenuItem("GameWork/Archipelago/Water/Apply Ocean Look To Selected")]
        private static void ApplyOceanLook() => ApplySelected(false);

        [MenuItem("GameWork/Archipelago/Water/Apply Mobile Look To Selected")]
        private static void ApplyMobileLook() => ApplySelected(true);

        private static void ApplySelected(bool mobile)
        {
            var go = Selection.activeGameObject;
            var ocean = go ? go.GetComponent<StylizedOceanSurface>() : null;
            if (!ocean)
            {
                EditorUtility.DisplayDialog("Ocean", "Zaznacz w Hierarchy obiekt oceanu z komponentem StylizedOceanSurface.", "OK");
                return;
            }
            try
            {
                var material = CreateMaterial(mobile);
                Undo.RecordObjects(new UnityEngine.Object[] { ocean, ocean.GetComponent<MeshRenderer>() }, "Apply ocean look");
                ocean.AssignMaterial(material);
                EditorUtility.SetDirty(ocean);
                PrefabUtility.RecordPrefabInstancePropertyModifications(ocean);
                PrefabUtility.RecordPrefabInstancePropertyModifications(ocean.GetComponent<MeshRenderer>());
                if (ocean.gameObject.scene.IsValid()) EditorSceneManager.MarkSceneDirty(ocean.gameObject.scene);
                SceneView.RepaintAll();
                EditorUtility.DisplayDialog("Ocean", "Zastosowano materiał i tekstury. Zapisz scenę. Dostosuj materiał w Inspectorze do skali statku i oświetlenia; opis parametrów jest w WATER_UPGRADE_PL.md.", "OK");
            }
            catch (Exception ex) { Debug.LogException(ex); }
        }

        // Public so the existing terrain window can create the same configured ocean.
        public static Material CreateMaterial(bool mobile = false)
        {
            var shader = Shader.Find(ShaderName);
            if (!shader) throw new InvalidOperationException("Brak shaderu " + ShaderName + ". Zaimportuj folder Runtime/Water.");
            string shaderPath = AssetDatabase.GetAssetPath(shader).Replace('\\', '/');
            const string suffix = "/Runtime/Water/StylizedOcean.shader";
            if (!shaderPath.EndsWith(suffix, StringComparison.Ordinal))
                throw new InvalidOperationException("Zachowaj układ ArchipelagoKit/Runtime/Water/StylizedOcean.shader.");
            // Works under Assets/Models/ArchipelagoKit as well as Assets/GameWork/ArchipelagoKit.
            string root = shaderPath.Substring(0, shaderPath.Length - suffix.Length);
            string textureFolder = root + "/Runtime/Water/Textures/";
            var wave = LoadDataMap(textureFolder + "WaveData.png");
            var foam = LoadDataMap(textureFolder + "FoamPattern.png");
            var variation = LoadDataMap(textureFolder + "MacroVariation.png");
            bool depth = EnableActiveDepth(!mobile);
            var mat = new Material(shader) { name = mobile ? "Ocean_Anime_Mobile" : "Ocean_Anime_TopDown" };
            mat.SetTexture("_WaveData", wave);
            mat.SetTexture("_FoamMap", foam);
            mat.SetTexture("_VariationMap", variation);
            mat.SetFloat("_UseDepth", depth ? 1f : 0f);
            mat.SetFloat("_UseSceneColor", depth && !mobile ? 1f : 0f);
            mat.SetFloat("_Mobile", mobile ? 1f : 0f);
            if (mobile) mat.EnableKeyword("_WATER_MOBILE");
            else mat.DisableKeyword("_WATER_MOBILE");
            EnsureFolder(root + "/Generated/Materials");
            string path = AssetDatabase.GenerateUniqueAssetPath(root + "/Generated/Materials/" + mat.name + ".mat");
            AssetDatabase.CreateAsset(mat, path);
            AssetDatabase.SaveAssets();
            if (!depth) Debug.LogWarning("Nie znaleziono aktywnego URP Asset. Ocean ma wyłączone efekty głębokości; skonfiguruj URP przed testem wybrzeża.");
            return mat;
        }

        private static Texture2D LoadDataMap(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!importer) throw new InvalidOperationException("Brak tekstury: " + path);
            // Packed slopes are NOT Unity normal-map textures: preserve the RGB data.
            bool change = importer.sRGBTexture || importer.textureType != TextureImporterType.Default
                       || !importer.mipmapEnabled || importer.wrapMode != TextureWrapMode.Repeat
                       || importer.filterMode != FilterMode.Trilinear
                       || importer.textureCompression != TextureImporterCompression.Uncompressed;
            if (change)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = false;
                importer.mipmapEnabled = true;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Trilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.anisoLevel = 2;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static bool EnableActiveDepth(bool enableOpaqueTexture)
        {
            var pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (!pipeline) return false;
            if (!pipeline.supportsCameraDepthTexture)
            {
                Undo.RecordObject(pipeline, "Enable ocean depth texture");
                pipeline.supportsCameraDepthTexture = true;
                EditorUtility.SetDirty(pipeline);
            }
            if (enableOpaqueTexture && !pipeline.supportsCameraOpaqueTexture)
            {
                Undo.RecordObject(pipeline, "Enable ocean opaque texture");
                pipeline.supportsCameraOpaqueTexture = true;
                EditorUtility.SetDirty(pipeline);
            }
            var camera = Camera.main;
            if (camera)
            {
                var extra = camera.GetComponent<UniversalAdditionalCameraData>();
                if (!extra) extra = Undo.AddComponent<UniversalAdditionalCameraData>(camera.gameObject);
                Undo.RecordObject(extra, "Enable ocean camera depth");
                extra.requiresDepthTexture = true;
                if (enableOpaqueTexture) extra.requiresColorTexture = true;
                EditorUtility.SetDirty(extra);
                PrefabUtility.RecordPrefabInstancePropertyModifications(extra);
                if (camera.gameObject.scene.IsValid()) EditorSceneManager.MarkSceneDirty(camera.gameObject.scene);
            }
            return true;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
#endif
