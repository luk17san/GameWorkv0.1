using UnityEngine;

namespace ThePirate.Cameras
{
    [DisallowMultipleComponent]
    public sealed class TopDownCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [Tooltip("Stałe przesunięcie w świecie, niezależne od obrotu statku.")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 18f, -12f);
        [SerializeField, Min(0.01f)] private float smoothTime = 0.2f;

        private Vector3 followVelocity;

        private void OnEnable()
        {
            followVelocity = Vector3.zero;
            if (target != null)
                transform.position = target.position + offset;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            transform.position = Vector3.SmoothDamp(transform.position,
                target.position + offset, ref followVelocity, smoothTime);

            if (offset.sqrMagnitude > 0.001f)
            {
                // Dla widoku dokładnie pionowego wybieramy inną oś góry.
                Vector3 up = Mathf.Abs(Vector3.Dot(offset.normalized, Vector3.up)) > 0.99f
                    ? Vector3.forward : Vector3.up;
                transform.rotation = Quaternion.LookRotation(-offset, up);
            }
        }
    }
}
