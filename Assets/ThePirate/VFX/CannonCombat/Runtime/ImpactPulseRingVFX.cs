using System.Collections;
using UnityEngine;

namespace GameWork.VFX
{
    /// <summary>Expanding thin ring used for sea impact and repair feedback.</summary>
    public sealed class ImpactPulseRingVFX : MonoBehaviour
    {
        private LineRenderer line;
        private Vector3 center;
        private Vector3 normal;
        private float radius;
        private float duration;
        private Color tint;

        public static void Play(Vector3 point, Vector3 surfaceNormal, float finalRadius, float seconds, Color color)
        {
            var go = new GameObject("VFX_ImpactPulseRing");
            var ring = go.AddComponent<ImpactPulseRingVFX>();
            ring.center = point + surfaceNormal.normalized * 0.025f;
            ring.normal = surfaceNormal.sqrMagnitude > 0.001f ? surfaceNormal.normalized : Vector3.up;
            ring.radius = finalRadius;
            ring.duration = seconds;
            ring.tint = color;
            ring.StartCoroutine(ring.Animate());
        }

        private IEnumerator Animate()
        {
            line = gameObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.loop = true;
            line.positionCount = 40;
            line.startWidth = Mathf.Clamp(radius * 0.065f, 0.08f, 0.24f);
            line.endWidth = line.startWidth;
            line.sharedMaterial = VFXMaterialCache.Get(null);
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.startColor = tint;
            line.endColor = tint;
            Vector3 tangent = Vector3.Cross(normal, Vector3.forward).normalized;
            if (tangent.sqrMagnitude < 0.001f) tangent = Vector3.Cross(normal, Vector3.right).normalized;
            Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / duration);
                float r = Mathf.Lerp(0.08f, radius, u);
                float a = tint.a * (1f - u);
                Color c = new Color(tint.r, tint.g, tint.b, a);
                line.startColor = c;
                line.endColor = c;
                for (int i = 0; i < line.positionCount; i++)
                {
                    float angle = i * Mathf.PI * 2f / line.positionCount;
                    line.SetPosition(i, center + (tangent * Mathf.Cos(angle) + bitangent * Mathf.Sin(angle)) * r);
                }
                yield return null;
            }
            // Shared material is retained for subsequent rings.
            Destroy(gameObject);
        }
    }
}
