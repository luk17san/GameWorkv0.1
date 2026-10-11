using UnityEngine;

namespace GameWork.VFX
{
    /// <summary>Continuous hull smoke and small flames driven by the ship's single durability value.</summary>
    public sealed class ShipDurabilityVFX : MonoBehaviour
    {
        [SerializeField] private Transform smokeAnchor;
        [SerializeField] private Transform fireAnchor;
        [SerializeField] private Texture2D smokeTexture;
        [SerializeField] private Texture2D fireTexture;
        [SerializeField, Range(0f, 1f)] private float smokeStartsBelow = 0.62f;
        [SerializeField, Range(0f, 1f)] private float fireStartsBelow = 0.24f;

        private ParticleSystem smoke;
        private ParticleSystem fire;

        private void Awake()
        {
            smokeTexture ??= Resources.Load<Texture2D>("GameWork/CannonVFX/CannonSmokePuff");
            fireTexture ??= Resources.Load<Texture2D>("GameWork/CannonVFX/CannonMuzzleFlash");
            smoke = MakeSystem("DamageSmoke", smokeAnchor != null ? smokeAnchor : transform, smokeTexture,
                new Color(0.35f, 0.32f, 0.29f, 0.58f), 1.8f, 1.3f, 0.6f);
            fire = MakeSystem("DamageFlame", fireAnchor != null ? fireAnchor : transform, fireTexture,
                new Color(1f, 0.55f, 0.18f, 0.9f), 0.55f, 0.7f, 0.25f);
            SetDurability01(1f);
        }

        /// <summary>Pass current durability normalized to 0..1 after damage or repair.</summary>
        public void SetDurability01(float durability01)
        {
            float d = Mathf.Clamp01(durability01);
            SetRate(smoke, d >= smokeStartsBelow ? 0f : Mathf.Lerp(2f, 12f, Mathf.InverseLerp(smokeStartsBelow, 0f, d)));
            SetRate(fire, d >= fireStartsBelow ? 0f : Mathf.Lerp(0.5f, 4f, Mathf.InverseLerp(fireStartsBelow, 0f, d)));
        }

        private static void SetRate(ParticleSystem ps, float rate)
        {
            if (ps == null) return;
            var emission = ps.emission;
            emission.rateOverTime = rate;
            if (rate > 0f && !ps.isPlaying) ps.Play();
            if (rate <= 0f && ps.isPlaying) ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }

        private static ParticleSystem MakeSystem(string name, Transform anchor, Texture2D texture, Color color,
            float life, float size, float speed)
        {
            var go = new GameObject(name);
            go.transform.SetParent(anchor, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(life * 0.7f, life * 1.3f);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.7f, size * 1.25f);
            main.startSpeed = speed;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Shape;
            main.maxParticles = 60;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.25f;
            var overLife = ps.colorOverLifetime;
            overLife.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.8f, 0.15f), new GradientAlphaKey(0f, 1f) });
            overLife.color = new ParticleSystem.MinMaxGradient(g);
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = VFXMaterialCache.Get(texture);
            return ps;
        }
    }
}
