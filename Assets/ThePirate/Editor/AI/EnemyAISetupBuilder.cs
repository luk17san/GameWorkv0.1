using System;
using GameWork.Framework.Ships.Movement;
using ThePirate.AI;
using ThePirate.Combat;
using UnityEditor;
using UnityEngine;

namespace ThePirate.EditorTools
{
    public static class EnemyAISetupBuilder
    {
        private const string Source = "Assets/ThePirate/Prefab/CombatTestEnemy.prefab";
        private const string Output = "Assets/ThePirate/Prefab/EnemyAIApproach.prefab";

        [MenuItem("Tools/GameWork/AI/Create approach test prefab")]
        public static void CreatePrefab()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before creating the prefab.");
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(Output);
            if (existing != null)
            {
                Selection.activeObject = existing;
                Debug.Log("EnemyAIApproach already exists. Existing settings preserved.");
                return;
            }
            var root = PrefabUtility.LoadPrefabContents(Source);
            try
            {
                root.name = "EnemyAIApproach";
                var movement = root.GetComponent<ShipMovementController>();
                if (movement == null) throw new InvalidOperationException("Source enemy has no movement controller.");
                var serialized = new SerializedObject(movement);
                if (serialized.FindProperty("movementStats").objectReferenceValue == null)
                    throw new InvalidOperationException("Assign Movement Stats on the source enemy first.");
                serialized.FindProperty("externalControl").boolValue = true;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                movement.enabled = true;
                foreach (var input in root.GetComponentsInChildren<ShipMovementInput>(true)) input.enabled = false;
                foreach (var input in root.GetComponentsInChildren<CannonPlayerInput>(true)) input.enabled = false;
                foreach (var fire in root.GetComponentsInChildren<ShipAutoFire>(true)) fire.enabled = false;
                foreach (var fire in root.GetComponentsInChildren<CrewAutoFireSystem>(true)) fire.enabled = false;
                foreach (var combat in root.GetComponentsInChildren<ShipCombatController>(true)) combat.enabled = false;
                if (root.GetComponent<EnemyShipAI>() == null) root.AddComponent<EnemyShipAI>();
                var body = root.GetComponent<Rigidbody>();
                body.useGravity = false;
                body.isKinematic = false;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                var prefab = PrefabUtility.SaveAsPrefabAsset(root, Output);
                if (prefab == null) throw new InvalidOperationException("Prefab creation failed.");
                Selection.activeObject = prefab;
                Debug.Log("Created EnemyAIApproach. Place it in an open-water test scene; source enemy and scene unchanged.");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
