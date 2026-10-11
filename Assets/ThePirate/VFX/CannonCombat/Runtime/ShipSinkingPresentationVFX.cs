using System.Collections;
using UnityEngine;

namespace GameWork.VFX
{
    /// <summary>Optional visual-only sinking motion. Assign only the rendered ship mesh, not gameplay root/colliders.</summary>
    public sealed class ShipSinkingPresentationVFX : MonoBehaviour
    {
        [SerializeField] private Transform shipVisual;
        [SerializeField] private Texture2D smokeTexture;
        [SerializeField, Min(0.5f)] private float duration = 4f;
        [SerializeField, Min(0f)] private float sinkDistance = 0.8f;
        [SerializeField, Range(0f, 60f)] private float rollDegrees = 24f;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private bool started;

        public void PlaySinking()
        {
            if (started) return;
            if (shipVisual == null) shipVisual = transform;
            started = true;
            startPosition = shipVisual.localPosition;
            startRotation = shipVisual.localRotation;
            StartCoroutine(Sink());
        }

        private IEnumerator Sink()
        {
            float t = 0f;
            float nextSmoke = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / duration);
                float eased = u * u;
                shipVisual.localPosition = startPosition + Vector3.down * (sinkDistance * eased);
                shipVisual.localRotation = startRotation * Quaternion.Euler(0f, 0f, rollDegrees * eased);
                nextSmoke -= Time.deltaTime;
                if (nextSmoke <= 0f)
                {
                    nextSmoke = 0.32f;
                    VFXBurst.Sprite("Sinking_Smoke", shipVisual.position, Vector3.up, smokeTexture,
                        new Color(0.28f, 0.27f, 0.25f, 0.58f), Random.Range(0.55f, 0.85f), 1.1f, 1, Vector3.up * 0.18f);
                    VFXBurst.Particles("Sinking_Bubbles", shipVisual.position, Vector3.up,
                        new Color(0.68f, 0.88f, 0.95f, 0.65f), 6, 0.09f, 1.25f, 0.15f, 0.65f);
                }
                yield return null;
            }
        }
    }
}
