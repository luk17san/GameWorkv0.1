using UnityEngine;

namespace GameWork.VFX
{
    /// <summary>
    /// Prototype muzzle flash: a short flame sprite followed by a soft smoke puff.
    /// Attach to a cannon or weapon mount and call Play() for each shot.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CannonMuzzleFlashVFX : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private Texture2D flashTexture;
        [SerializeField] private Texture2D smokeTexture;

        [Header("Flash")]
        [SerializeField, Min(0.01f)] private float flashSize = 2.4f;
        [SerializeField, Min(0.01f)] private float flashLifetime = 0.18f;
        [SerializeField, Min(0f)] private float flashForwardNudge = 0.12f;

        [Header("Smoke")]
        [SerializeField, Range(1, 8)] private int smokePuffs = 4;
        [SerializeField, Min(0.01f)] private float smokeLifetime = 1.3f;
        [SerializeField, Min(0.01f)] private float smokeSize = 1.55f;
        [SerializeField, Min(0f)] private float smokeRiseSpeed = 0.6f;
        [SerializeField, Min(0f)] private float smokeForwardSpeed = 1.1f;
        [SerializeField, Range(0f, 1f)] private float smokeOpacity = 0.88f;

        private ParticleSystem flashParticles;
        private ParticleSystem smokeParticles;
        private Material flashMaterial;
        private Material smokeMaterial;

        private void Awake()
        {
            flashTexture ??= Resources.Load<Texture2D>("GameWork/CannonVFX/CannonMuzzleFlash");
            smokeTexture ??= Resources.Load<Texture2D>("GameWork/CannonVFX/CannonSmokePuff");
            CreateParticleSystems();
        }

        private void OnDestroy()
        {
            if (flashMaterial != null) Destroy(flashMaterial);
            if (smokeMaterial != null) Destroy(smokeMaterial);
        }

        /// <summary>Plays one shot effect. Call once for each cannon that fires in a staggered salvo.</summary>
        public void Configure(Transform muzzle) => muzzlePoint = muzzle;
        public void Play()
        {
            if (flashParticles == null || smokeParticles == null) CreateParticleSystems();

            Transform origin = muzzlePoint != null ? muzzlePoint : transform;
            Vector3 forward = origin.forward.normalized;
            if (forward.sqrMagnitude < 0.001f) forward = transform.forward;

            var flash = new ParticleSystem.EmitParams
            {
                position = origin.position,
                velocity = forward * flashForwardNudge,
                startLifetime = flashLifetime,
                startSize = flashSize,
                startColor = Color.white,
                rotation = GetScreenRotation(forward)
            };
            flashParticles.Emit(flash, 1);
            VFXBurst.Sprite("MuzzleHotCore", origin.position + forward * 0.25f, forward, flashTexture,
                new Color(1f, 0.9f, 0.55f, 1f), flashSize * 0.55f, 0.09f, 1);
            VFXBurst.Particles("MuzzleEmbers", origin.position, forward,
                new Color(1f, 0.65f, 0.2f, 1f), 5, 0.12f, 0.28f, 3f, 6f);
            flashParticles.Play();

            for (int i = 0; i < smokePuffs; i++)
            {
                Vector3 drift = Random.insideUnitSphere * 0.12f;
                drift.y = 0f;
                var smoke = new ParticleSystem.EmitParams
                {
                    position = origin.position + Random.insideUnitSphere * 0.035f,
                    velocity = Vector3.up * smokeRiseSpeed + forward * smokeForwardSpeed + drift,
                    startLifetime = smokeLifetime * Random.Range(0.78f, 1.18f),
                    startSize = smokeSize * Random.Range(0.78f, 1.2f),
                    startColor = new Color(1f, 1f, 1f, smokeOpacity),
                    rotation = Random.Range(0f, Mathf.PI * 2f)
                };
                smokeParticles.Emit(smoke, 1);
            }
            smokeParticles.Play();
        }

        private void CreateParticleSystems()
        {
            if (flashParticles == null)
                flashParticles = CreateSystem("MuzzleFlashParticles", 12, flashLifetime, out flashMaterial, flashTexture);
            if (smokeParticles == null)
                smokeParticles = CreateSystem("MuzzleSmokeParticles", 32, smokeLifetime * 1.25f, out smokeMaterial, smokeTexture);
        }

        private ParticleSystem CreateSystem(string childName, int maxParticles, float duration,
            out Material material, Texture2D texture)
        {
            var child = new GameObject(childName);
            child.transform.SetParent(transform, false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;

            var ps = child.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = Mathf.Max(0.1f, duration);
            main.startLifetime = duration;
            main.startSpeed = 0f;
            main.startSize = 1f;
            main.startColor = Color.white;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Shape;
            main.maxParticles = maxParticles;
            main.stopAction = ParticleSystemStopAction.None;

            var emission = ps.emission;
            emission.enabled = false;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.maxParticleSize = 0.35f;
            material = BuildMaterial(texture);
            renderer.material = material;

            ConfigureFade(ps, childName == "MuzzleSmokeParticles");
            return ps;
        }

        private static Material BuildMaterial(Texture2D texture)
        {
            return new Material(VFXMaterialCache.Get(texture));
        }
        private static Material LegacyMaterial(Texture2D texture)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            var material = new Material(shader) { name = "CannonMuzzleVFX_RuntimeMaterial" };
            if (texture != null)
            {
                if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
                if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
            }
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
            if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
            material.enableInstancing = true;
            return material;
        }

        private static void ConfigureFade(ParticleSystem ps, bool smoke)
        {
            var color = ps.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                smoke
                    ? new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.16f), new GradientAlphaKey(0.65f, 0.62f), new GradientAlphaKey(0f, 1f) }
                    : new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.85f, 0.25f), new GradientAlphaKey(0f, 1f) });
            color.color = new ParticleSystem.MinMaxGradient(gradient);

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = smoke
                ? new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.72f), new Keyframe(0.5f, 1f), new Keyframe(1f, 1.3f)))
                : new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.82f), new Keyframe(0.18f, 1f), new Keyframe(1f, 0.72f)));
        }

        private static float GetScreenRotation(Vector3 forward)
        {
            Camera camera = Camera.main;
            if (camera != null)
            {
                Vector3 projected = Vector3.ProjectOnPlane(forward, camera.transform.forward).normalized;
                if (projected.sqrMagnitude > 0.001f)
                    return Mathf.Atan2(Vector3.Dot(projected, camera.transform.up), Vector3.Dot(projected, camera.transform.right));
            }
            return Mathf.Atan2(forward.z, forward.x);
        }
    }
}
