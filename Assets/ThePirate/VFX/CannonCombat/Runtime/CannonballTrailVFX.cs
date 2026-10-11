using UnityEngine;

namespace GameWork.VFX
{
    /// <summary>Attach to the moving cannonball prefab for a short, restrained smoke trail.</summary>
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class CannonballTrailVFX : MonoBehaviour
    {
        [SerializeField] private Texture2D smokeTexture;
        [SerializeField, Min(1f)] private float rate = 32f;
        [SerializeField, Min(0.01f)] private float particleSize = 0.42f;
        [SerializeField] private Color tint = new Color(0.76f, 0.73f, 0.68f, 0.8f);

        private Material runtimeMaterial;

        private void Awake()
        {
            smokeTexture ??= Resources.Load<Texture2D>("GameWork/CannonVFX/CannonSmokePuff");
            var ps = GetComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.5f);
            main.startSpeed = 0.05f;
            main.startSize = new ParticleSystem.MinMaxCurve(particleSize * 0.7f, particleSize * 1.25f);
            main.startColor = tint;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Shape;
            main.maxParticles = 120;
            var emission = ps.emission;
            emission.rateOverTime = rate;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.025f;
            var color = ps.colorOverLifetime;
            color.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.9f, 0.12f), new GradientAlphaKey(0f, 1f) });
            color.color = new ParticleSystem.MinMaxGradient(g);
            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.45f), new Keyframe(1f, 1.5f)));
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = VFXMaterialCache.Get(smokeTexture);
            ps.Play();
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null) Destroy(runtimeMaterial);
        }

        private static Material CreateMaterial(Texture2D texture)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
            var mat = new Material(shader) { name = "CannonballTrail_RuntimeMaterial" };
            if (texture != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", texture);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);
            }
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
            return mat;
        }
    }
}
