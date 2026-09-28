using UnityEngine;
using UnityEngine.InputSystem;

namespace ThePirate.Cameras
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class TopDownCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [Tooltip("Stałe przesunięcie w świecie, niezależne od obrotu statku. W Perspective określa też początkową odległość.")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 18f, -12f);
        [SerializeField, Min(0.01f)] private float smoothTime = 0.2f;

        [Header("Zoom")]
        [Tooltip("Zmiana rozmiaru lub odległości na krok kółka myszy.")]
        [SerializeField, Min(0f)] private float zoomSpeed = 2f;
        [SerializeField, Min(0.01f)] private float zoomSmoothTime = 0.15f;
        [Header("Orthographic — rozmiar widoku")]
        [SerializeField, Min(0.01f)] private float minOrthographicSize = 5f;
        [SerializeField, Min(0.01f)] private float maxOrthographicSize = 40f;
        [Header("Perspective — odległość od statku")]
        [SerializeField, Min(0.01f)] private float minPerspectiveDistance = 8f;
        [SerializeField, Min(0.01f)] private float maxPerspectiveDistance = 80f;

        private Camera attachedCamera;
        private Vector3 followPosition;
        private Vector3 followVelocity;
        private float orthographicSize;
        private float desiredOrthographicSize;
        private float perspectiveDistance;
        private float desiredPerspectiveDistance;
        private float orthographicVelocity;
        private float perspectiveVelocity;
        private bool hasFollowPosition;

        private Vector3 OffsetDirection => offset.sqrMagnitude > 0.001f
            ? offset.normalized : Vector3.up;

        private void OnEnable()
        {
            attachedCamera = GetComponent<Camera>();
            if (attachedCamera == null)
            {
                Debug.LogError("TopDownCamera wymaga komponentu Camera na tym samym obiekcie.", this);
                enabled = false;
                return;
            }

            OnValidate();
            followVelocity = Vector3.zero;
            orthographicVelocity = perspectiveVelocity = 0f;
            orthographicSize = desiredOrthographicSize = Mathf.Clamp(
                attachedCamera.orthographicSize, minOrthographicSize, maxOrthographicSize);
            perspectiveDistance = desiredPerspectiveDistance = Mathf.Clamp(
                offset.magnitude, minPerspectiveDistance, maxPerspectiveDistance);
            hasFollowPosition = target != null;
            if (hasFollowPosition)
            {
                followPosition = target.position;
                ApplyCameraPose();
            }
        }

        private void Update()
        {
            if (target == null || Mouse.current == null)
                return;

            float scroll = Mouse.current.scroll.ReadValue().y;
            // Input System domyślnie zwraca kroki. Tryb natywny Windows używa 120 na krok.
            if (InputSystem.settings.scrollDeltaBehavior == InputSettings.ScrollDeltaBehavior.KeepPlatformSpecificInputRange
                && (Application.platform == RuntimePlatform.WindowsEditor
                    || Application.platform == RuntimePlatform.WindowsPlayer))
                scroll /= 120f;

            if (attachedCamera.orthographic)
                desiredOrthographicSize = Mathf.Clamp(desiredOrthographicSize - scroll * zoomSpeed,
                    minOrthographicSize, maxOrthographicSize);
            else
                desiredPerspectiveDistance = Mathf.Clamp(desiredPerspectiveDistance - scroll * zoomSpeed,
                    minPerspectiveDistance, maxPerspectiveDistance);
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                hasFollowPosition = false;
                return;
            }

            if (!hasFollowPosition)
            {
                followPosition = target.position;
                followVelocity = Vector3.zero;
                hasFollowPosition = true;
            }

            followPosition = Vector3.SmoothDamp(followPosition,
                target.position, ref followVelocity, smoothTime);
            orthographicSize = Mathf.SmoothDamp(orthographicSize,
                desiredOrthographicSize, ref orthographicVelocity, zoomSmoothTime);
            perspectiveDistance = Mathf.SmoothDamp(perspectiveDistance,
                desiredPerspectiveDistance, ref perspectiveVelocity, zoomSmoothTime);
            ApplyCameraPose();
        }

        private void ApplyCameraPose()
        {
            Vector3 direction = OffsetDirection;
            transform.position = followPosition + (attachedCamera.orthographic
                ? offset : direction * perspectiveDistance);
            if (attachedCamera.orthographic)
                attachedCamera.orthographicSize = orthographicSize;

            // Dla widoku dokładnie pionowego wybieramy inną oś góry.
            Vector3 up = Mathf.Abs(Vector3.Dot(direction, Vector3.up)) > 0.99f
                ? Vector3.forward : Vector3.up;
            transform.rotation = Quaternion.LookRotation(-direction, up);
        }

        private void OnValidate()
        {
            smoothTime = Mathf.Max(0.01f, smoothTime);
            zoomSpeed = Mathf.Max(0f, zoomSpeed);
            zoomSmoothTime = Mathf.Max(0.01f, zoomSmoothTime);
            minOrthographicSize = Mathf.Max(0.01f, minOrthographicSize);
            maxOrthographicSize = Mathf.Max(minOrthographicSize, maxOrthographicSize);
            minPerspectiveDistance = Mathf.Max(0.01f, minPerspectiveDistance);
            maxPerspectiveDistance = Mathf.Max(minPerspectiveDistance, maxPerspectiveDistance);
            desiredOrthographicSize = Mathf.Clamp(desiredOrthographicSize, minOrthographicSize, maxOrthographicSize);
            desiredPerspectiveDistance = Mathf.Clamp(desiredPerspectiveDistance, minPerspectiveDistance, maxPerspectiveDistance);
        }
    }
}
