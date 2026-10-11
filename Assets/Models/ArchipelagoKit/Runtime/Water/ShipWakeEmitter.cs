using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace GameWork.ArchipelagoKit.Water
{
    [DisallowMultipleComponent]
    public sealed class ShipWakeEmitter : MonoBehaviour
    {
        [Header("References")]
        public Material wakeMaterial;
        public StylizedOceanSurface ocean;
        [Tooltip("Optional emitter at the stern. Otherwise Local Stern Offset is used.")]
        public Transform stern;
        public Vector3 localSternOffset = new Vector3(0, 0, -4);
        public float seaLevel;
        [Min(0.001f)] public float surfaceOffset = 0.06f;
        [Header("Size in world metres")]
        [Min(0.1f)] public float shipWidth = 3;
        [Min(0)] public float spreadSpeed = 0.9f;
        [Min(0.1f)] public float foamTileLength = 6;
        [Header("Emission")]
        [Min(0.01f)] public float minimumSpeed = 0.3f;
        [Min(0.1f)] public float speedForFullWake = 9;
        [Min(0.1f)] public float lifetime = 10;
        [Range(0.03f, 0.3f)] public float sampleInterval = 0.08f;
        [Range(16, 256)] public int maximumSamples = 160;
        [Min(1)] public float teleportDistance = 40;
        [Range(0, 1)] public float opacity = 0.8f;

        private struct Sample
        {
            public Vector3 center, right;
            public float time, strength, distance;
            public int stroke;
        }
        private readonly List<Sample> samples = new List<Sample>(256);
        private readonly List<Vector3> vertices = new List<Vector3>(1536);
        private readonly List<Vector2> uv = new List<Vector2>(1536);
        private readonly List<Color> colors = new List<Color>(1536);
        private readonly List<int> triangles = new List<int>(4590);
        private GameObject trailObject;
        private Mesh mesh;
        private MeshRenderer trailRenderer;
        private Vector3 previousPosition;
        private Vector3 filteredVelocity;
        private float nextEmission, travelled;
        private int stroke;
        private bool wasMoving;

        private void OnValidate()
        {
            shipWidth = Mathf.Max(.1f, shipWidth);
            lifetime = Mathf.Max(.1f, lifetime);
            minimumSpeed = Mathf.Max(.01f, minimumSpeed);
            speedForFullWake = Mathf.Max(minimumSpeed + .1f, speedForFullWake);
            sampleInterval = Mathf.Clamp(sampleInterval, .03f, .3f);
            maximumSamples = Mathf.Clamp(maximumSamples, 16, 256);
            foamTileLength = Mathf.Max(.1f, foamTileLength);
        }
        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            previousPosition = transform.position;
            nextEmission = Time.time;
            travelled = 0;
            filteredVelocity = Vector3.zero;
            wasMoving = false;
            samples.Clear();
            trailObject = new GameObject(name + "_Wake_Runtime");
            // Root at identity: old vertices must not move or rotate with the ship.
            SceneManager.MoveGameObjectToScene(trailObject, gameObject.scene);
            trailObject.layer = gameObject.layer;
            var filter = trailObject.AddComponent<MeshFilter>();
            trailRenderer = trailObject.AddComponent<MeshRenderer>();
            trailRenderer.sharedMaterial = wakeMaterial;
            trailRenderer.shadowCastingMode = ShadowCastingMode.Off;
            trailRenderer.receiveShadows = false;
            mesh = new Mesh { name = "ShipWake_Dynamic" };
            mesh.MarkDynamic();
            filter.sharedMesh = mesh;
        }
        private void LateUpdate()
        {
            if (!mesh) return;
            float now = Time.time;
            float dt = Time.deltaTime;
            if (dt <= 0) { previousPosition = transform.position; return; }
            Vector3 delta = transform.position - previousPosition;
            delta.y = 0;
            previousPosition = transform.position;
            float step = delta.magnitude;
            if (step > teleportDistance)
            {
                ClearWake();
                nextEmission = now;
                return;
            }
            // Smooth over FixedUpdate steps, so a frame without a physics tick does not break the trail.
            filteredVelocity = Vector3.Lerp(filteredVelocity, delta / dt, 1f - Mathf.Exp(-10f * dt));
            float speed = filteredVelocity.magnitude;
            bool moving = speed >= minimumSpeed;
            if (moving && !wasMoving) { stroke++; nextEmission = now; }
            travelled += step;
            if (moving && now >= nextEmission && wakeMaterial)
            {
                Vector3 direction = filteredVelocity / Mathf.Max(speed, .00001f);
                Vector3 emitter = stern ? stern.position : transform.TransformPoint(localSternOffset);
                // A reversed ship emits from the opposite end unless an explicit stern is supplied.
                if (!stern && Vector3.Dot(direction, transform.forward) < 0)
                    emitter = transform.TransformPoint(-localSternOffset);
                if (samples.Count == 0 || samples[samples.Count - 1].stroke != stroke ||
                    (emitter - samples[samples.Count - 1].center).sqrMagnitude > .0025f)
                samples.Add(new Sample
                {
                    center = emitter,
                    right = Vector3.Cross(Vector3.up, direction).normalized,
                    time = now,
                    strength = Mathf.InverseLerp(minimumSpeed, Mathf.Max(minimumSpeed + .1f, speedForFullWake), speed),
                    distance = travelled,
                    stroke = stroke
                });
                nextEmission = now + sampleInterval;
            }
            wasMoving = moving;
            while (samples.Count > 0 && (now - samples[0].time > lifetime || samples.Count > maximumSamples))
                samples.RemoveAt(0);
            trailRenderer.sharedMaterial = wakeMaterial;
            BuildMesh(now);
        }

        [ContextMenu("Clear Wake")]
        public void ClearWake()
        {
            samples.Clear();
            travelled = 0;
            previousPosition = transform.position;
            filteredVelocity = Vector3.zero;
            nextEmission = Time.time;
            wasMoving = false;
            if (mesh) mesh.Clear();
        }

        private void BuildMesh(float now)
        {
            vertices.Clear(); uv.Clear(); colors.Clear(); triangles.Clear();
            float level = (ocean ? ocean.SeaLevel : seaLevel) + surfaceOffset;
            for (int i = 0; i < samples.Count; i++)
            {
                Sample s = samples[i];
                float age = Mathf.Max(0, now - s.time);
                float normalizedAge = Mathf.Clamp01(age / Mathf.Max(.1f, lifetime));
                float fade = (1 - normalizedAge) * (1 - normalizedAge);
                // Also taper the oldest retained sample when the capacity, not lifetime, is reached.
                float tailFade = Mathf.Clamp01(i / 3f);
                for (int ribbon = -1; ribbon <= 1; ribbon++)
                {
                    bool centerRibbon = ribbon == 0;
                    float spread = shipWidth * .35f + age * spreadSpeed * Mathf.Lerp(.45f, 1, s.strength);
                    float halfWidth = centerRibbon ? shipWidth * .26f + age * .22f : shipWidth * .075f + age * .09f;
                    Vector3 center = s.center + s.right * (ribbon * spread);
                    center.y = level;
                    float alpha = opacity * s.strength * fade * tailFade * (centerRibbon ? .8f : 1f);
                    for (int side = 0; side < 2; side++)
                    {
                        vertices.Add(center + s.right * ((side * 2 - 1) * halfWidth));
                        uv.Add(new Vector2(side, s.distance / Mathf.Max(.1f, foamTileLength) + ribbon * 0.37f));
                        colors.Add(new Color(1, 1, 1, alpha));
                    }
                    // Separate strokes prevent a bridge across stop/restart or a mooring turn.
                    if (i > 0 && s.stroke == samples[i - 1].stroke)
                    {
                        int b = i * 6 + (ribbon + 1) * 2;
                        int a = b - 6;
                        triangles.Add(a); triangles.Add(b); triangles.Add(a + 1);
                        triangles.Add(a + 1); triangles.Add(b); triangles.Add(b + 1);
                    }
                }
            }
            mesh.Clear();
            if (vertices.Count < 12) return;
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uv);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0, true);
        }
        private void OnDisable()
        {
            if (trailObject) Destroy(trailObject);
            if (mesh) Destroy(mesh);
            trailObject = null; mesh = null; trailRenderer = null;
            samples.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 emitter = stern ? stern.position : transform.TransformPoint(localSternOffset);
            emitter.y = (ocean ? ocean.SeaLevel : seaLevel) + surfaceOffset;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(emitter, .25f);
            Gizmos.DrawLine(emitter - transform.right * shipWidth * .5f, emitter + transform.right * shipWidth * .5f);
        }
    }
}
