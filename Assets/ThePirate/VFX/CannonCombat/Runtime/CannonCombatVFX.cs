using System.Collections;
using UnityEngine;

namespace GameWork.VFX
{
    /// <summary>Impact, repair, and special-shot effects. Put on a reusable VFX manager or ship.</summary>
    public sealed class CannonCombatVFX : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] private Texture2D waterSplash;
        [SerializeField] private Texture2D woodSplinters;
        [SerializeField] private Texture2D smokePuff;
        [SerializeField] private Texture2D flashSprite;

        [Header("Scale")]
        [SerializeField, Min(0.1f)] private float waterScale = 3.2f;
        [SerializeField, Min(0.1f)] private float hullScale = 2.2f;
        [SerializeField, Min(0.1f)] private float groundScale = 2.4f;

        private void Awake()
        {
            waterSplash ??= Resources.Load<Texture2D>("GameWork/CannonVFX/WaterImpactSplash");
            woodSplinters ??= Resources.Load<Texture2D>("GameWork/CannonVFX/WoodSplinters");
            smokePuff ??= Resources.Load<Texture2D>("GameWork/CannonVFX/CannonSmokePuff");
            flashSprite ??= Resources.Load<Texture2D>("GameWork/CannonVFX/CannonMuzzleFlash");
        }
        public void PlayWaterImpact(Vector3 point, Vector3 normal, bool special = false)
        {
            Vector3 up = SafeNormal(normal);
            float scale = waterScale * (special ? 1.65f : 1f);
            VFXBurst.Sprite("WaterImpact_Splash", point, up, waterSplash, Color.white, scale, special ? 0.85f : 0.65f, 1);
            VFXBurst.Particles("WaterImpact_Droplets", point, up, new Color(0.72f, 0.9f, 1f, 0.92f), 30, scale * 0.085f, 0.7f, 3.4f, 7.2f);
            VFXBurst.Particles("WaterImpact_Foam", point, up, new Color(0.9f, 0.98f, 1f, 0.7f), 18, scale * 0.15f, 0.9f, 0.35f, 1.2f);
            ImpactPulseRingVFX.Play(point, up, scale * 0.85f, special ? 1.3f : 1f, new Color(0.78f, 0.94f, 1f, 0.75f));
        }

        public void PlayHullImpact(Vector3 point, Vector3 normal, bool critical = false)
        {
            Vector3 away = SafeNormal(normal);
            float scale = hullScale * (critical ? 1.55f : 1f);
            point += away * 0.12f;
            VFXBurst.Sprite("HullImpact_HotCore", point, away, flashSprite,
                new Color(1f, 0.8f, 0.4f, 1f), scale * 0.65f, 0.16f, 1);
            VFXBurst.Sprite("HullImpact_Splinters", point, away, woodSplinters, Color.white, scale, 0.5f, 1);
            VFXBurst.Particles("HullImpact_Debris", point, away, new Color(0.5f, 0.28f, 0.12f, 1f), critical ? 22 : 12, scale * 0.13f, 0.5f, 2.2f, 5.4f);
            VFXBurst.Particles("HullImpact_Sparks", point, away, new Color(1f, 0.64f, 0.24f, 1f), critical ? 16 : 7, scale * 0.07f, 0.22f, 2.8f, 6.2f);
            VFXBurst.Sprite("HullImpact_Smoke", point, away, smokePuff, new Color(0.8f, 0.72f, 0.62f, critical ? 0.95f : 0.8f), scale * (critical ? 0.95f : 0.7f), critical ? 1.3f : 0.95f, 1);
        }

        public void PlayGroundImpact(Vector3 point, Vector3 normal)
        {
            Vector3 up = SafeNormal(normal);
            VFXBurst.Sprite("GroundImpact_Dust", point, up, smokePuff, new Color(0.72f, 0.63f, 0.47f, 0.9f), groundScale, 0.72f, 1);
            VFXBurst.Particles("GroundImpact_Debris", point, up, new Color(0.48f, 0.43f, 0.35f, 1f), 18, groundScale * 0.12f, 0.52f, 1.5f, 4.8f);
        }

        /// <summary>Oversized mortar/special-round impact. Choose the hit type at the collision point.</summary>
        public void PlaySpecialImpact(Vector3 point, Vector3 normal, CannonImpactSurface surface)
        {
            if (surface == CannonImpactSurface.Water) PlayWaterImpact(point, normal, true);
            else if (surface == CannonImpactSurface.Wood) PlayHullImpact(point, normal, true);
            else
            {
                Vector3 up = SafeNormal(normal);
                VFXBurst.Sprite("SpecialImpact_Dust", point, up, smokePuff, new Color(0.76f, 0.66f, 0.48f, 1f), groundScale * 1.5f, 0.9f, 1);
                VFXBurst.Particles("SpecialImpact_Debris", point, up, new Color(0.55f, 0.48f, 0.36f, 1f), 32, groundScale * 0.16f, 0.8f, 2.2f, 6.2f);
            }
        }

        /// <summary>Small repair glints and wood chips centered on the repaired hull section.</summary>
        public void PlayRepair(Vector3 point, Vector3 normal, float amount01 = 0.5f)
        {
            Vector3 away = SafeNormal(normal);
            int count = Mathf.RoundToInt(Mathf.Lerp(5f, 14f, Mathf.Clamp01(amount01)));
            VFXBurst.Particles("Repair_Glints", point, away, new Color(1f, 0.76f, 0.39f, 1f), count, 0.18f, 0.65f, 0.5f, 1.8f);
            VFXBurst.Particles("Repair_WoodChips", point, away, new Color(0.62f, 0.39f, 0.2f, 1f), Mathf.Max(3, count / 2), 0.12f, 0.65f, 0.25f, 0.9f);
            VFXBurst.Sprite("Repair_WarmPulse", point, away, flashSprite, new Color(1f, 0.76f, 0.38f, 0.38f), 1.2f + amount01 * 0.6f, 0.4f, 1);
            ImpactPulseRingVFX.Play(point, away, 1.2f + amount01 * 0.8f, 0.7f, new Color(1f, 0.72f, 0.35f, 0.42f));
        }

        /// <summary>Call after the first shot to leave a soft smoke trace above the battery.</summary>
        public void PlayLingeringBatterySmoke(Transform mount, int gunCount = 1)
        {
            if (mount == null) return;
            int count = Mathf.Clamp(gunCount, 1, 6);
            for (int i = 0; i < count; i++)
            {
                Vector3 jitter = new Vector3(Random.Range(-0.22f, 0.22f), 0.05f, Random.Range(-0.22f, 0.22f));
                VFXBurst.Sprite("BatterySmoke", mount.position + jitter, Vector3.up, smokePuff,
                    new Color(0.68f, 0.65f, 0.6f, 0.62f), Random.Range(1.1f, 1.6f), Random.Range(1.6f, 2.2f), 1, Vector3.up * 0.45f);
            }
        }

        private static Vector3 SafeNormal(Vector3 normal) => normal.sqrMagnitude > 0.001f ? normal.normalized : Vector3.up;
    }

    public enum CannonImpactSurface { Water, Wood, Ground }

    internal static class VFXBurst
    {
        private static Material sharedWhiteMaterial;

        public static void Sprite(string name, Vector3 position, Vector3 forward, Texture2D texture, Color tint,
            float size, float lifetime, int count, Vector3 velocity = default)
        {
            var go = NewSystem(name, position, forward, Mathf.Max(count, 1), lifetime);
            var ps = go.GetComponent<ParticleSystem>();
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = MakeMaterial(texture);
            var growth = ps.sizeOverLifetime;
            growth.enabled = true;
            growth.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.65f), new Keyframe(0.2f, 1f), new Keyframe(1f, 1.55f)));
            for (int i = 0; i < count; i++)
            {
                var ep = new ParticleSystem.EmitParams
                {
                    position = position,
                    velocity = velocity + Random.insideUnitSphere * 0.08f,
                    startLifetime = lifetime * Random.Range(0.85f, 1.1f),
                    startSize = size * Random.Range(0.88f, 1.12f),
                    startColor = tint,
                    rotation = Random.Range(0f, Mathf.PI * 2f)
                };
                ps.Emit(ep, 1);
            }
            ps.Play();
        }

        public static void Particles(string name, Vector3 position, Vector3 normal, Color color,
            int count, float size, float lifetime, float minSpeed, float maxSpeed)
        {
            var go = NewSystem(name, position, normal, count, lifetime);
            var ps = go.GetComponent<ParticleSystem>();
            ps.GetComponent<ParticleSystemRenderer>().sharedMaterial = MakeMaterial(null);
            var main = ps.main;
            main.gravityModifier = name.Contains("Smoke") || name.Contains("Foam") || name.Contains("Bubbles") ? 0f : 0.55f;
            for (int i = 0; i < count; i++)
            {
                Vector3 dir = (normal + Random.insideUnitSphere * 0.75f).normalized;
                if (Vector3.Dot(dir, normal) < 0f) dir = Vector3.Reflect(dir, normal);
                var ep = new ParticleSystem.EmitParams
                {
                    position = position,
                    velocity = dir * Random.Range(minSpeed, maxSpeed),
                    startLifetime = lifetime * Random.Range(0.65f, 1.15f),
                    startSize = size * Random.Range(0.55f, 1.25f),
                    startColor = color,
                    rotation = Random.Range(0f, Mathf.PI * 2f)
                };
                ps.Emit(ep, 1);
            }
            ps.Play();
        }

        private static GameObject NewSystem(string name, Vector3 position, Vector3 forward, int maxParticles, float duration)
        {
            var go = new GameObject("VFX_" + name);
            go.transform.position = position;
            if (forward.sqrMagnitude > 0.001f) go.transform.rotation = Quaternion.LookRotation(forward);
            var ps = go.AddComponent<ParticleSystem>();
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
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.maxParticleSize = 0.35f;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            main.maxParticles = Mathf.Max(4, maxParticles);
            main.stopAction = ParticleSystemStopAction.Destroy;
            var emission = ps.emission;
            emission.enabled = false;
            var color = ps.colorOverLifetime;
            color.enabled = true;
            var fade = new Gradient();
            fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0.75f, 0.55f), new GradientAlphaKey(0f, 1f) });
            color.color = new ParticleSystem.MinMaxGradient(fade);
            Object.Destroy(go, duration + 1.5f);
            return go;
        }

        private static Material MakeMaterial(Texture2D texture) => VFXMaterialCache.Get(texture);
    }

    /// <summary>Plays a brief local recoil and returns the barrel to its captured rest position.</summary>
    public sealed class CannonBarrelRecoilVFX : MonoBehaviour
    {
        [SerializeField] private Transform barrel;
        [SerializeField, Min(0f)] private float distance = 0.12f;
        [SerializeField, Min(0.01f)] private float recoilSeconds = 0.07f;
        [SerializeField, Min(0.01f)] private float returnSeconds = 0.16f;
        private Vector3 restPosition;
        private Coroutine running;

        private void Awake()
        {
            if (barrel == null) barrel = transform;
            restPosition = barrel.localPosition;
        }

        public void Play()
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(Recoil());
        }

        private IEnumerator Recoil()
        {
            Vector3 back = restPosition - barrel.localRotation * Vector3.forward * distance;
            yield return Move(barrel.localPosition, back, recoilSeconds);
            yield return Move(back, restPosition, returnSeconds);
            barrel.localPosition = restPosition;
            running = null;
        }

        private IEnumerator Move(Vector3 from, Vector3 to, float seconds)
        {
            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / seconds));
                barrel.localPosition = Vector3.LerpUnclamped(from, to, eased);
                yield return null;
            }
        }
    }
}
