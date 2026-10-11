using System;
using System.Collections.Generic;
using Framework.Inventory;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThePirate.Economy
{
    [Serializable]
    public sealed class FloatingLootData
    {
        public Vector3 position;
        public float yaw;
        public int appearance;
        public ResourceStack[] resources;
    }

    [DisallowMultipleComponent]
    public sealed class FloatingLoot : MonoBehaviour
    {
        private static readonly HashSet<FloatingLoot> active = new HashSet<FloatingLoot>();
        [SerializeField] private ResourceStack[] resources = { new ResourceStack { resourceId = "wood", quantity = 5 } };
        [SerializeField, Range(0, 1)] private int appearance;
        [SerializeField, Min(0f)] private float bobHeight = 0.12f;
        [SerializeField, Min(0f)] private float bobSpeed = 1.3f;
        [SerializeField] private float roll = 6f;
        private Vector3 anchor;
        private float yaw;
        private float phase;
        public Vector3 Anchor => anchor;
        public string Label => appearance == 0 ? "Deski" : "Szczątki statku";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() => active.Clear();
        private void OnEnable() { anchor = transform.position; yaw = transform.eulerAngles.y; phase = UnityEngine.Random.value * Mathf.PI * 2f; active.Add(this); }
        private void OnDisable() => active.Remove(this);
        private void Update()
        {
            if (global::Framework.Menu.PauseService.GameplayInputBlocked) return;
            phase += Time.deltaTime * bobSpeed;
            transform.SetPositionAndRotation(anchor + Vector3.up * (Mathf.Sin(phase) * bobHeight), Quaternion.Euler(Mathf.Sin(phase * 0.8f) * roll, yaw, Mathf.Cos(phase) * roll));
        }
        public static FloatingLoot Nearest(Scene scene, Vector3 position, float range)
        {
            FloatingLoot result = null; float best = range * range;
            foreach (var item in active)
            {
                if (item == null || item.gameObject.scene != scene) continue;
                Vector3 delta = item.anchor - position; delta.y = 0f;
                if (delta.sqrMagnitude > best) continue;
                best = delta.sqrMagnitude; result = item;
            }
            return result;
        }
        public bool TryCollect(ShipCargo cargo, out string error)
        {
            error = null;
            if (!isActiveAndEnabled || cargo == null) { error = "Brak dostępnego łupu lub ładowni."; return false; }
            if (resources == null || resources.Length == 0) { error = "Pusta paczka łupu."; return false; }
            if (!cargo.TryGetInventory(out var inventory, out error) || !inventory.TryAddBatch(resources, out error)) return false;
            gameObject.SetActive(false); Destroy(gameObject); return true;
        }
        public FloatingLootData Capture() => new FloatingLootData { position = anchor, yaw = yaw, appearance = appearance, resources = Copy(resources) };
        private static ResourceStack[] Copy(ResourceStack[] source)
        {
            if (source == null) return null;
            var result = new ResourceStack[source.Length];
            for (int i = 0; i < source.Length; i++) if (source[i] != null) result[i] = new ResourceStack { resourceId = source[i].resourceId, quantity = source[i].quantity };
            return result;
        }
        public static FloatingLootData[] CaptureScene(Scene scene)
        {
            var result = new List<FloatingLootData>();
            foreach (var item in active) if (item != null && item.gameObject.scene == scene) result.Add(item.Capture());
            return result.ToArray();
        }
        public static bool Valid(FloatingLootData[] data)
        {
            if (data == null) return true; // Older saves have no floating loot snapshot.
            if (data.Length > 4096) return false;
            var catalog = Resources.Load<ResourceCatalog>(ShipCargo.CatalogPath);
            if (data.Length > 0 && catalog == null) return false;
            foreach (var item in data)
            {
                if (item == null || !Finite(item.position.x) || !Finite(item.position.y) || !Finite(item.position.z) || !Finite(item.yaw)
                    || item.appearance < 0 || item.appearance > 1 || item.resources == null || item.resources.Length == 0 || item.resources.Length > 1024) return false;
                var ids = new HashSet<string>();
                foreach (var stack in item.resources)
                    if (stack == null || !ResourceDefinition.ValidId(stack.resourceId) || stack.quantity <= 0 || !ids.Add(stack.resourceId) || catalog.Find(stack.resourceId) == null) return false;
            }
            return true;
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        public static void RestoreScene(Scene scene, FloatingLootData[] data)
        {
            // New snapshots replace scene objects, including an empty snapshot after collection.
            // Older saves retain manually placed scene objects.
            if (data == null) return;
            foreach (var item in new List<FloatingLoot>(active))
                if (item != null && item.gameObject.scene == scene) { item.gameObject.SetActive(false); Destroy(item.gameObject); }
            foreach (var item in data) Spawn(scene, item);
        }
        public static FloatingLoot Spawn(Scene scene, FloatingLootData data)
        {
            var prefab = Resources.Load<GameObject>("GameWork/FloatingLoot/" + (data.appearance == 0 ? "Planks" : "Wreckage"));
            var go = prefab != null ? Instantiate(prefab) : CreateVisual(data.appearance);
            SceneManager.MoveGameObjectToScene(go, scene);
            var loot = go.GetComponent<FloatingLoot>() ?? go.AddComponent<FloatingLoot>();
            loot.resources = Copy(data.resources); loot.appearance = data.appearance;
            loot.anchor = data.position; loot.yaw = data.yaw;
            go.transform.SetPositionAndRotation(data.position, Quaternion.Euler(0f, data.yaw, 0f));
            return loot;
        }
        // Also used by the editor builder: ordinary editable mesh objects, no physics obstacles.
        public static GameObject CreateVisual(int kind)
        {
            var root = new GameObject(kind == 0 ? "Planks" : "Wreckage");
            int count = kind == 0 ? 3 : 5;
            for (int i = 0; i < count; i++)
            {
                var plank = GameObject.CreatePrimitive(PrimitiveType.Cube); plank.name = "Plank " + (i + 1);
                plank.transform.SetParent(root.transform, false);
                plank.transform.localPosition = new Vector3((i - (count - 1) * 0.5f) * 0.45f, i % 2 * 0.09f, i % 2 * 0.25f);
                plank.transform.localScale = new Vector3(0.32f, 0.18f, kind == 0 ? 2.2f : 3f);
                plank.transform.localRotation = Quaternion.Euler(0f, kind == 0 ? i * 7f : i * 27f, 0f);
                var collider = plank.GetComponent<Collider>(); collider.enabled = false;
                if (Application.isPlaying) Destroy(collider); else DestroyImmediate(collider);
                var block = new MaterialPropertyBlock(); block.SetColor("_BaseColor", new Color(0.32f, 0.18f, 0.08f)); block.SetColor("_Color", new Color(0.32f, 0.18f, 0.08f));
                plank.GetComponent<Renderer>().SetPropertyBlock(block);
            }
            return root;
        }
    }
}
