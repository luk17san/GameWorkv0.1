using UnityEngine;

namespace GameWork.VFX
{
    /// <summary>Small intermittent foam bursts beside a moving ship; assign port and starboard waterline points.</summary>
    public sealed class ShipWaterFoamVFX : MonoBehaviour
    {
        [SerializeField] private Rigidbody shipBody;
        [SerializeField] private Transform portWaterline;
        [SerializeField] private Transform starboardWaterline;
        [SerializeField] private Texture2D foamTexture;
        [SerializeField, Min(0f)] private float speedThreshold = 0.8f;
        [SerializeField, Min(0.02f)] private float interval = 0.12f;
        [SerializeField, Min(0.01f)] private float particleSize = 0.65f;
        private ParticleSystem foam;
        private float timer;

        private void Awake()
        {
            foamTexture ??= Resources.Load<Texture2D>("GameWork/CannonVFX/WaterImpactSplash");
            if (shipBody == null) shipBody = GetComponentInParent<Rigidbody>();
            var go = new GameObject("ShipWakeFoamParticles");
            go.transform.SetParent(transform, false);
            foam = go.AddComponent<ParticleSystem>();
            foam.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = foam.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 0.5f;
            main.startSpeed = 0f;
            main.startSize = particleSize;
            main.startColor = new Color(0.82f, 0.95f, 1f, 0.75f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Shape;
            main.maxParticles = 100;
            var emission = foam.emission;
            emission.enabled = false;
            foam.GetComponent<ParticleSystemRenderer>().material = VFXMaterialCache.Get(foamTexture);
        }

        public void Configure(Rigidbody body, Transform port, Transform starboard)
        { shipBody = body; portWaterline = port; starboardWaterline = starboard; }
        private void Update()
        {
            if (shipBody == null || shipBody.linearVelocity.magnitude < speedThreshold) return;
            timer += Time.deltaTime;
            if (timer < interval) return;
            timer = 0f;
            EmitAt(portWaterline);
            EmitAt(starboardWaterline);
        }

        private void EmitAt(Transform point)
        {
            if (point == null) return;
            var ep = new ParticleSystem.EmitParams
            {
                position = point.position,
                velocity = -shipBody.linearVelocity * 0.12f + Vector3.up * 0.12f,
                startLifetime = Random.Range(0.3f, 0.55f),
                startSize = particleSize * Random.Range(0.65f, 1.2f),
                startColor = new Color(0.86f, 0.96f, 1f, 0.75f),
                rotation = Random.Range(0f, Mathf.PI * 2f)
            };
            foam.Emit(ep, 1);
            foam.Play();
        }
    }
}
