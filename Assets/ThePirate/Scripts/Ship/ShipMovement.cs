using UnityEngine;

namespace ThePirate.Ships
{
    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    public sealed class ShipMovement : MonoBehaviour
    {
        [Header("Speed")]
        [SerializeField, Min(0f)] private float maxSpeed = 10f;
        [Tooltip("Zmiana zadanej prędkości na sekundę trzymania W lub S.")]
        [SerializeField, Min(0f)] private float throttleRate = 5f;
        [SerializeField, Min(0f)] private float acceleration = 3f;
        [SerializeField, Min(0f)] private float braking = 5f;

        [Header("Steering")]
        [Tooltip("Prędkość skręcania w stopniach na sekundę.")]
        [SerializeField, Min(0f)] private float turnSpeed = 60f;

        public float TargetSpeed { get; private set; }
        public float CurrentSpeed { get; private set; }

        private Rigidbody body;
        private float throttleInput;
        private float steeringInput;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            // Prototyp porusza się na stałej wysokości, bez kołysania.
            body.useGravity = false;
            body.isKinematic = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.constraints = RigidbodyConstraints.FreezePositionY
                | RigidbodyConstraints.FreezeRotationX
                | RigidbodyConstraints.FreezeRotationZ;
        }

        public void SetInput(float throttle, float steering)
        {
            throttleInput = Mathf.Clamp(throttle, -1f, 1f);
            steeringInput = Mathf.Clamp(steering, -1f, 1f);
        }

        private void FixedUpdate()
        {
            float step = Time.fixedDeltaTime;
            // Bez wejścia wartość zadana zostaje zapamiętana. Nie cofamy.
            TargetSpeed = Mathf.Clamp(TargetSpeed + throttleInput * throttleRate * step,
                0f, maxSpeed);
            float changeRate = TargetSpeed > CurrentSpeed ? acceleration : braking;
            CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, TargetSpeed, changeRate * step);

            Quaternion rotation = body.rotation
                * Quaternion.Euler(0f, steeringInput * turnSpeed * step, 0f);
            body.angularVelocity = Vector3.zero;
            body.MoveRotation(rotation);
            body.linearVelocity = rotation * Vector3.forward * CurrentSpeed;
        }

        private void OnDisable()
        {
            throttleInput = 0f;
            steeringInput = 0f;
            TargetSpeed = 0f;
            CurrentSpeed = 0f;
            if (body != null && !body.isKinematic)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }
    }
}
