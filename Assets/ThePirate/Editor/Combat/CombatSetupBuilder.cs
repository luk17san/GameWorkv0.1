using System;
using System.IO;
using Framework.Health;
using ThePirate.Combat;
using UnityEditor;
using UnityEngine;

namespace ThePirate.Editor.Combat
{
    public static class CombatSetupBuilder
    {
        private const string PlayerPath = "Assets/ThePirate/Prefab/PlayerShip.prefab";
        private const string EnemyPath = "Assets/ThePirate/Prefab/CombatTestEnemy.prefab";
        private const string Request = "Temp/GameWorkCombatSetup.request";
        private const string Result = "Temp/GameWorkCombatSetup.result";
        [InitializeOnLoadMethod]
        private static void CheckRequest() { if (File.Exists(Request)) EditorApplication.delayCall += RunRequested; }
        private static void RunRequested()
        {
            if (!File.Exists(Request)) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) { EditorApplication.delayCall += RunRequested; return; }
            File.Delete(Request);
            try { Build(); File.WriteAllText(Result, "OK: PlayerShip batteries=4 cannons=10 mortar=1; CombatTestEnemy; asset validation and 40 edit-mode checks passed. Play Mode not tested."); }
            catch (Exception exception) { File.WriteAllText(Result, "ERROR: " + exception); Debug.LogException(exception); }
        }
        [MenuItem("GameWork/Combat/Configure combat prefabs")]
        public static void Build()
        {
            var root = PrefabUtility.LoadPrefabContents(PlayerPath);
            try
            {
                if (root.transform.Find("CombatRig") == null)
                {
                    var projectile = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThePirate/Prefab/Cannonball.prefab").GetComponent<CannonProjectile>();
                    if (projectile == null) throw new InvalidOperationException("Missing Cannonball projectile.");
                    foreach (var input in root.GetComponentsInChildren<CannonPlayerInput>(true)) UnityEngine.Object.DestroyImmediate(input);
                    var oldCannon = root.transform.Find("TestCannon");
                    if (oldCannon != null) oldCannon.gameObject.SetActive(false);
                    var hull = Add<Health>(root); SetInt(hull, "maxHealth", 500);
                    Add<CombatShip>(root).Configure(1);
                    Add<ShipCombatController>(root); Add<CannonPlayerInput>(root);
                    Add<ShipCollisionDamage>(root); Add<ShipAutoFire>(root);
                    Add<CrewAutoFireSystem>(root); Add<ShipConditionController>(root);
                    Add<FireRangeVisualizer>(root);
                    var special = Add<SpecialWeaponController>(root);
                    var rigidbody = root.GetComponent<Rigidbody>(); rigidbody.mass = 1000;
                    rigidbody.constraints |= RigidbodyConstraints.FreezePositionY;
                    // Rozmiary rzeczywistych rendererów w lokalnym układzie prefabu.
                    Bounds bounds = new Bounds(Vector3.zero, Vector3.zero); bool found = false;
                    foreach (var renderer in root.GetComponentsInChildren<Renderer>())
                    {
                        Bounds local = renderer.localBounds;
                        for (int i = 0; i < 8; i++)
                        {
                            Vector3 corner = local.center + Vector3.Scale(local.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                            Vector3 point = root.transform.InverseTransformPoint(renderer.transform.TransformPoint(corner));
                            if (!found) { bounds = new Bounds(point, Vector3.zero); found = true; } else bounds.Encapsulate(point);
                        }
                    }
                    if (!found) bounds = new Bounds(Vector3.zero, new Vector3(4, 3, 12));
                    float halfWidth = Mathf.Max(0.5f, bounds.extents.x * 0.65f);
                    float halfLength = Mathf.Max(1f, bounds.extents.z * 0.85f);
                    var box = root.GetComponent<BoxCollider>();
                    box.center = new Vector3(bounds.center.x, 0.3f, bounds.center.z);
                    box.size = new Vector3(halfWidth * 2, 1.4f, halfLength * 2);
                    Transform rig = Child(root.transform, "CombatRig", Vector3.zero);
                    Battery(rig, "LeftBroadside", new Vector3(box.center.x - halfWidth - 0.45f, 0.8f, box.center.z), -90, 4, halfLength, root.transform, projectile);
                    Battery(rig, "RightBroadside", new Vector3(box.center.x + halfWidth + 0.45f, 0.8f, box.center.z), 90, 4, halfLength, root.transform, projectile);
                    Battery(rig, "BowBattery", new Vector3(box.center.x, 0.8f, box.center.z + halfLength + 0.45f), 0, 1, halfLength, root.transform, projectile);
                    Battery(rig, "SternBattery", new Vector3(box.center.x, 0.8f, box.center.z - halfLength - 0.45f), 180, 1, halfLength, root.transform, projectile);
                    var mortar = Child(rig, "MortarMount", new Vector3(box.center.x, 2.5f, box.center.z));
                    var module = mortar.gameObject.AddComponent<ShipDamageModule>(); module.Configure(ShipModuleKind.Battery);
                    var muzzle = Child(mortar, "Muzzle", Vector3.up * 0.5f).gameObject.AddComponent<Cannon>(); muzzle.Configure(root.transform, projectile);
                    special.Configure(muzzle, module);
                    Module(rig, "SailsDamageZone", new Vector3(box.center.x, 4f, box.center.z), new Vector3(halfWidth, 3f, 1f), ShipModuleKind.Sails);
                    var crew = Module(rig, "CrewDamageZone", new Vector3(box.center.x, 1.5f, box.center.z), new Vector3(halfWidth, 0.5f, halfLength), ShipModuleKind.Crew);
                    var crewFire = new SerializedObject(root.GetComponent<CrewAutoFireSystem>());
                    crewFire.FindProperty("crewHealth").objectReferenceValue = crew; crewFire.ApplyModifiedPropertiesWithoutUndo();
                    if (PrefabUtility.SaveAsPrefabAsset(root, PlayerPath) == null) throw new IOException("PlayerShip save failed.");
                }
                if (AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPath) == null)
                {
                    root.name = "CombatTestEnemy";
                    root.GetComponent<CombatShip>().Configure(2);
                    root.GetComponent<CannonPlayerInput>().enabled = false;
                    root.GetComponent<FireRangeVisualizer>().enabled = false;
                    foreach (var component in root.GetComponents<MonoBehaviour>())
                        if (component.GetType().Namespace == "GameWork.Framework.Ships.Movement") component.enabled = false;
                    var automatic = new SerializedObject(root.GetComponent<ShipAutoFire>());
                    automatic.FindProperty("automaticFire").boolValue = true; automatic.ApplyModifiedPropertiesWithoutUndo();
                    if (PrefabUtility.SaveAsPrefabAsset(root, EnemyPath) == null) throw new IOException("Enemy save failed.");
                }
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            AssetDatabase.SaveAssets();
            Validate(PlayerPath); Validate(EnemyPath);
            CheckGeometryAndDamage();
        }
        private static void CheckGeometryAndDamage()
        {
            var instance = PrefabUtility.LoadPrefabContents(PlayerPath);
            int checks = 0;
            try
            {
                foreach (var battery in instance.GetComponentsInChildren<WeaponBattery>())
                {
                    Vector3 center = battery.transform.position, forward = battery.transform.forward;
                    float mid = (battery.MinimumRange + battery.MaximumRange) * 0.5f;
                    Check(battery.Contains(center + forward * mid), "forward inside", ref checks);
                    Check(!battery.Contains(center), "zero distance", ref checks);
                    Check(!battery.Contains(center + forward * (battery.MinimumRange - 0.1f)), "minimum range", ref checks);
                    Check(!battery.Contains(center + forward * (battery.MaximumRange + 0.1f)), "maximum range", ref checks);
                    Check(!battery.Contains(center - forward * mid), "opposite direction", ref checks);
                    Check(!battery.Contains(center + Quaternion.AngleAxis(battery.FiringArc * 0.5f + 1f, Vector3.up) * forward * mid), "outside arc", ref checks);
                    Check(battery.Contains(center + forward * mid + Vector3.up * 20), "horizontal sector", ref checks);
                    var module = battery.GetComponent<ShipDamageModule>();
                    module.TakeDamage(75); Check(Mathf.Approximately(module.Fraction, 0.25f), "module damage", ref checks);
                    module.TakeDamage(999); Check(module.Destroyed, "module depleted", ref checks);
                    module.Repair(100); Check(Mathf.Approximately(module.Fraction, 1f), "module repair", ref checks);
                }
                Debug.Log("Combat edit-mode checks passed: " + checks + ". No Play Mode test was run.");
            }
            finally { PrefabUtility.UnloadPrefabContents(instance); }
        }
        private static void Check(bool value, string label, ref int checks)
        {
            if (!value) throw new InvalidOperationException("Combat check failed: " + label);
            checks++;
        }
        private static void Validate(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab.GetComponentsInChildren<WeaponBattery>().Length != 4 || prefab.GetComponentsInChildren<Cannon>().Length != 11
                || prefab.GetComponent<CombatShip>() == null || prefab.GetComponent<ShipCollisionDamage>() == null)
                throw new InvalidOperationException("Combat asset validation failed: " + path);
            foreach (var component in prefab.GetComponentsInChildren<Transform>(true))
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(component.gameObject) > 0) throw new InvalidOperationException("Missing script: " + component.name);
        }
        private static T Add<T>(GameObject root) where T : Component => root.GetComponent<T>() ?? root.AddComponent<T>();
        private static void SetInt(UnityEngine.Object target, string field, int value) { var serialized = new SerializedObject(target); serialized.FindProperty(field).intValue = value; serialized.ApplyModifiedPropertiesWithoutUndo(); }
        private static Transform Child(Transform parent, string label, Vector3 position)
        {
            var child = new GameObject(label).transform; child.SetParent(parent, false); child.localPosition = position; return child;
        }
        private static ShipDamageModule Module(Transform parent, string label, Vector3 position, Vector3 size, ShipModuleKind kind)
        {
            var module = Child(parent, label, position).gameObject.AddComponent<ShipDamageModule>(); module.Configure(kind);
            module.gameObject.AddComponent<BoxCollider>().size = size; return module;
        }
        private static void Battery(Transform parent, string label, Vector3 position, float yaw, int count, float halfLength, Transform owner, CannonProjectile projectile)
        {
            var mount = Child(parent, label, position); mount.localRotation = Quaternion.Euler(0, yaw, 0);
            mount.gameObject.AddComponent<WeaponBattery>();
            var zone = mount.gameObject.AddComponent<BoxCollider>(); zone.size = new Vector3(count > 1 ? halfLength : 0.8f, 0.4f, 0.25f);
            for (int i = 0; i < count; i++)
            {
                float offset = count == 1 ? 0 : Mathf.Lerp(-halfLength * 0.65f, halfLength * 0.65f, (float)i / (count - 1));
                var cannon = Child(mount, "Muzzle_" + (i + 1), new Vector3(offset, 0, 0.4f)).gameObject.AddComponent<Cannon>();
                cannon.Configure(owner, projectile);
            }
        }
    }
}
