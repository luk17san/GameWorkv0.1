using UnityEngine;

namespace GameWork.ArchipelagoKit.Water
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class StylizedOceanSurface : MonoBehaviour
    {
        [SerializeField] private Material oceanMaterial;
        [SerializeField, Min(10f)] private float size = 4000f;

        public float SeaLevel => transform.position.y;

        private void OnEnable() => Apply();
        private void OnValidate() => Apply();

        private void Apply()
        {
            if (!TryGetComponent(out MeshRenderer renderer)) return;
            if (oceanMaterial) renderer.sharedMaterial = oceanMaterial;
            transform.localScale = new Vector3(size / 10f, 1f, size / 10f);
        }

        public void AssignMaterial(Material material)
        {
            oceanMaterial = material;
            // Applying a look must not overwrite an existing object's dimensions.
            if (TryGetComponent(out MeshRenderer renderer)) renderer.sharedMaterial = material;
        }

        public void Configure(Material material, float worldSize, float seaLevel)
        {
            oceanMaterial = material;
            size = Mathf.Max(10f, worldSize);
            transform.position = new Vector3(transform.position.x, seaLevel, transform.position.z);
            Apply();
        }
    }
}
