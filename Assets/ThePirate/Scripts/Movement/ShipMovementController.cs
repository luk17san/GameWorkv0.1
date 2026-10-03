using UnityEngine;

namespace GameWork.Framework.Ships.Movement
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(ShipMovementInput))]
    public sealed class ShipMovementController : MonoBehaviour
    {
        private const float StopEpsilon = 0.01f;
        private const float InputDeadZone = 0.1f;

        [Header("Configuration")]
        [SerializeField] private ShipMovementStats movementStats;
        [SerializeField] private ShipCargoStats cargoStats;

        [Header("Runtime state")]
        [SerializeField] private ShipMovementState state = new ShipMovementState();
        [SerializeField, Min(0f)] private float currentCargoWeight;

        [Header("Condition multipliers")]
        [SerializeField, Range(0f, 1f)] private float speedCondition = 1f;
        [SerializeField, Range(0f, 1f)] private float accelerationCondition = 1f;
        [SerializeField, Range(0f, 1f)] private float steeringCondition = 1f;

        private readonly System.Collections.Generic.List<Vector3> contactNormals = new System.Collections.Generic.List<Vector3>();
        private Rigidbody shipRigidbody;
        private ShipMovementInput movementInput;
        private float steeringInput;
        private bool brakeWasHeld;
        private bool moored;
        public void SetMoored(bool value) { moored = value; steeringInput = 0f; brakeWasHeld = false; }
        [SerializeField] private bool externalControl;
        private float navigationSpeed;
        private float navigationSteering;
        private float lastNavigationCommand = float.NegativeInfinity;

        public bool ExternalControl => externalControl;
        public bool HasMovementConfiguration => movementStats != null;
        public float BrakingDeceleration => movementStats == null ? 0f :
            movementStats.Acceleration * accelerationCondition *
            (cargoStats == null ? 1f : cargoStats.GetAccelerationMultiplier(currentCargoWeight)) *
            movementStats.BrakingMultiplier;

        // External commands use the same acceleration, cargo and damage rules as player input.
        public void SetNavigationCommand(float speed, float steering)
        {
            if (!externalControl) return;
            navigationSpeed = float.IsNaN(speed) || float.IsInfinity(speed) ? 0f : Mathf.Max(0f, speed);
            navigationSteering = float.IsNaN(steering) || float.IsInfinity(steering) ? 0f : Mathf.Clamp(steering, -1f, 1f);
            lastNavigationCommand = Time.time;
        }

        public ShipMovementState State => state;
        public float CurrentCargoWeight => currentCargoWeight;
        public bool HasCargoCapacity => cargoStats != null;
        public float CargoCapacity => cargoStats != null ? cargoStats.CargoCapacity : 0f;

        public void RestoreMotion(float requestedSpeed, float currentSpeed, bool reverseArmed, Vector3 externalVelocity)
        {
            contactNormals.Clear();
            state.SetRequestedTargetSpeed(requestedSpeed);
            state.SetCurrentPropulsionSpeed(currentSpeed);
            state.SetReverseArmed(reverseArmed);
            state.SetExternalVelocity(externalVelocity);
            state.SetFinalVelocity(shipRigidbody.rotation * Vector3.forward * currentSpeed + externalVelocity);
            steeringInput = 0f;
            brakeWasHeld = false;
            if (!shipRigidbody.isKinematic)
            {
                shipRigidbody.linearVelocity = state.FinalVelocity;
                shipRigidbody.angularVelocity = Vector3.zero;
            }
        }

        private void Awake()
        {
            shipRigidbody = GetComponent<Rigidbody>();
            movementInput = GetComponent<ShipMovementInput>();
            state.Reset();
        }

        private void Update()
        {
            if (moored || global::Framework.Menu.PauseService.GameplayInputBlocked)
            {
                steeringInput = 0f;
                brakeWasHeld = false;
                return;
            }

            if (movementStats == null)
            {
                steeringInput = 0f;
                return;
            }

            if (externalControl) return;

            Vector2 input = movementInput.ReadMovement();
            steeringInput = Mathf.Abs(input.x) >= InputDeadZone ? input.x : 0f;

            bool forwardHeld = input.y >= InputDeadZone;
            bool brakeHeld = input.y <= -InputDeadZone;
            bool brakeReleased = brakeWasHeld && !brakeHeld;

            UpdateRequestedSpeed(forwardHeld, brakeHeld, brakeReleased);
            brakeWasHeld = brakeHeld;
        }

        private void FixedUpdate()
        {
            if (movementStats == null)
            {
                return;
            }

            if (moored) { RestoreMotion(0f, 0f, false, Vector3.zero); return; }
            SynchronizeCollisionSpeed();
            CalculateEffectiveStats();
            if (externalControl)
            {
                bool fresh = Time.time - lastNavigationCommand <= 0.25f &&
                    !global::Framework.Menu.PauseService.GameplayInputBlocked;
                state.SetRequestedTargetSpeed(fresh ? navigationSpeed : 0f);
                state.SetReverseArmed(false);
                steeringInput = fresh ? navigationSteering : 0f;
            }
            ClampAllowedTargetSpeed();
            UpdateCurrentSpeed();
            Quaternion rotation = ApplySteering();
            ApplyMovement(rotation);
            contactNormals.Clear();
        }

        public void SetCargoWeight(float weight)
        {
            if (cargoStats == null)
            {
                currentCargoWeight = Mathf.Max(0f, weight);
                return;
            }

            currentCargoWeight = Mathf.Clamp(weight, 0f, cargoStats.CargoCapacity);
        }

        public bool TryAddCargo(float addedWeight)
        {
            if (cargoStats == null || !cargoStats.CanAddCargo(currentCargoWeight, addedWeight))
            {
                return false;
            }

            currentCargoWeight += addedWeight;
            return true;
        }

        public void SetCondition(float speed, float acceleration, float steering)
        {
            speedCondition = Mathf.Clamp01(speed);
            accelerationCondition = Mathf.Clamp01(acceleration);
            steeringCondition = Mathf.Clamp01(steering);
        }

        public void SetExternalVelocity(Vector3 velocity)
        {
            state.SetExternalVelocity(velocity);
        }

        private void UpdateRequestedSpeed(bool forwardHeld, bool brakeHeld, bool brakeReleased)
        {
            float requestedSpeed = state.RequestedTargetSpeed;
            float acceleration = GetCommandAcceleration();
            float deceleration = acceleration * movementStats.BrakingMultiplier;

            if (forwardHeld)
            {
                state.SetReverseArmed(false);

                if (requestedSpeed < 0f)
                {
                    requestedSpeed = Mathf.MoveTowards(requestedSpeed, 0f, deceleration * Time.deltaTime);
                }
                else
                {
                    requestedSpeed = Mathf.MoveTowards(
                        requestedSpeed,
                        movementStats.MaxForwardSpeed,
                        acceleration * Time.deltaTime);
                }
            }
            else if (brakeHeld)
            {
                if (requestedSpeed > 0f || state.CurrentPropulsionSpeed > StopEpsilon)
                {
                    requestedSpeed = Mathf.MoveTowards(requestedSpeed, 0f, deceleration * Time.deltaTime);
                }
                else if (requestedSpeed < -StopEpsilon ||
                         state.CurrentPropulsionSpeed < -StopEpsilon ||
                         state.ReverseArmed)
                {
                    state.SetReverseArmed(false);
                    requestedSpeed = Mathf.MoveTowards(
                        requestedSpeed,
                        -movementStats.MaxReverseSpeed,
                        movementStats.ReverseAcceleration * Time.deltaTime);
                }
                else
                {
                    requestedSpeed = 0f;
                }
            }

            if (brakeReleased &&
                Mathf.Abs(requestedSpeed) <= StopEpsilon &&
                Mathf.Abs(state.CurrentPropulsionSpeed) <= StopEpsilon)
            {
                state.SetReverseArmed(true);
            }

            state.SetRequestedTargetSpeed(requestedSpeed);
        }

        private float GetCommandAcceleration()
        {
            return state.EffectiveAcceleration > 0f
                ? state.EffectiveAcceleration
                : movementStats.Acceleration;
        }

        private void CalculateEffectiveStats()
        {
            float cargoSpeedMultiplier = cargoStats == null
                ? 1f
                : cargoStats.GetSpeedMultiplier(currentCargoWeight);
            float cargoAccelerationMultiplier = cargoStats == null
                ? 1f
                : cargoStats.GetAccelerationMultiplier(currentCargoWeight);

            float effectiveMaxSpeed = movementStats.MaxForwardSpeed *
                                      speedCondition *
                                      cargoSpeedMultiplier;
            float effectiveAcceleration = movementStats.Acceleration *
                                          accelerationCondition *
                                          cargoAccelerationMultiplier;

            state.SetEffectiveMaxSpeed(effectiveMaxSpeed);
            state.SetEffectiveAcceleration(effectiveAcceleration);
        }

        private void ClampAllowedTargetSpeed()
        {
            float effectiveMaxReverseSpeed =
                state.EffectiveMaxSpeed * movementStats.MaxReverseSpeedRatio;

            float allowedSpeed = Mathf.Clamp(
                state.RequestedTargetSpeed,
                -effectiveMaxReverseSpeed,
                state.EffectiveMaxSpeed);

            state.SetAllowedTargetSpeed(allowedSpeed);
        }

        private void UpdateCurrentSpeed()
        {
            float currentSpeed = state.CurrentPropulsionSpeed;
            float targetSpeed = state.AllowedTargetSpeed;
            bool accelerating = Mathf.Sign(currentSpeed) == Mathf.Sign(targetSpeed) &&
                                Mathf.Abs(targetSpeed) > Mathf.Abs(currentSpeed);

            float changeRate;
            if (!accelerating)
            {
                changeRate = state.EffectiveAcceleration * movementStats.BrakingMultiplier;
            }
            else if (targetSpeed < 0f)
            {
                changeRate = state.EffectiveAcceleration * movementStats.ReverseAccelerationRatio;
            }
            else
            {
                changeRate = state.EffectiveAcceleration;
            }

            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                targetSpeed,
                changeRate * Time.fixedDeltaTime);

            if (Mathf.Abs(currentSpeed) <= StopEpsilon && Mathf.Abs(targetSpeed) <= StopEpsilon)
            {
                currentSpeed = 0f;
            }

            state.SetCurrentPropulsionSpeed(currentSpeed);
            UpdateDriveState(currentSpeed, targetSpeed);
        }

        private void UpdateDriveState(float currentSpeed, float targetSpeed)
        {
            if (currentSpeed < -StopEpsilon || targetSpeed < -StopEpsilon)
            {
                state.SetDriveState(ShipDriveState.Reverse);
            }
            else if (currentSpeed > targetSpeed + StopEpsilon)
            {
                state.SetDriveState(ShipDriveState.Braking);
            }
            else if (currentSpeed > StopEpsilon || targetSpeed > StopEpsilon)
            {
                state.SetDriveState(ShipDriveState.Forward);
            }
            else
            {
                state.SetDriveState(ShipDriveState.Stopped);
            }
        }

        private void ApplyMovement(Quaternion rotation)
        {
            Vector3 propulsionVelocity = rotation * Vector3.forward * state.CurrentPropulsionSpeed;
            Vector3 finalVelocity = propulsionVelocity + state.ExternalVelocity;
            // Contacts describe the preceding physics step. Remove propulsion into the surface.
            foreach (Vector3 normal in contactNormals)
            {
                float inwardSpeed = Vector3.Dot(finalVelocity, normal);
                if (inwardSpeed < 0f) finalVelocity -= normal * inwardSpeed;
            }
            state.SetFinalVelocity(finalVelocity);

            if (shipRigidbody.isKinematic)
            {
                shipRigidbody.MovePosition(shipRigidbody.position + finalVelocity * Time.fixedDeltaTime);
            }
            else
            {
                // Dynamiczne Rigidbody przesuwa fizyka, zachowujac interpolacje obrazu.
                shipRigidbody.linearVelocity = finalVelocity;
            }
        }

        private Quaternion ApplySteering()
        {
            float normalizedSpeed = state.EffectiveMaxSpeed <= StopEpsilon
                ? 0f
                : Mathf.Abs(state.CurrentPropulsionSpeed) / state.EffectiveMaxSpeed;
            float turnEfficiency = movementStats.GetTurnEfficiency(normalizedSpeed);
            state.SetCurrentTurnEfficiency(turnEfficiency);

            float turnDegrees = steeringInput *
                                movementStats.BaseTurnSpeed *
                                turnEfficiency *
                                steeringCondition *
                                Time.fixedDeltaTime;

            Quaternion turnRotation = Quaternion.Euler(0f, turnDegrees, 0f);
            Quaternion rotation = shipRigidbody.rotation * turnRotation;
            if (!shipRigidbody.isKinematic)
            {
                shipRigidbody.angularVelocity = Vector3.zero;
            }
            shipRigidbody.MoveRotation(rotation);
            return rotation;
        }

        private void OnCollisionEnter(Collision collision) => RecordCollision(collision);
        private void OnCollisionStay(Collision collision) => RecordCollision(collision);

        private void RecordCollision(Collision collision)
        {
            if (movementStats == null || shipRigidbody.isKinematic || collision.collider.GetComponentInParent<ThePirate.Combat.CannonProjectile>() != null) return;
            var health = collision.collider.GetComponentInParent<global::Framework.Health.Health>();
            if (health != null && health.IsDepleted) return;
            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector3 normal = collision.GetContact(i).normal;
                normal.y = 0f;
                if (normal.sqrMagnitude > 0.01f) contactNormals.Add(normal.normalized);
            }
        }

        private void SynchronizeCollisionSpeed()
        {
            if (contactNormals.Count == 0 || shipRigidbody.isKinematic) return;
            float previous = state.CurrentPropulsionSpeed;
            float actual = Vector3.Dot(shipRigidbody.linearVelocity - state.ExternalVelocity, shipRigidbody.rotation * Vector3.forward);
            // A shove or bounce is not extra engine speed or an automatic reverse command.
            float retained = previous >= 0f ? Mathf.Clamp(actual, 0f, previous) : Mathf.Clamp(actual, previous, 0f);
            state.SetCurrentPropulsionSpeed(retained);
            if (!externalControl)
            {
                float requested = state.RequestedTargetSpeed;
                if (requested * previous > 0f && Mathf.Abs(requested) > Mathf.Abs(retained))
                    state.SetRequestedTargetSpeed(retained);
            }
        }

        private void OnDisable() => contactNormals.Clear();

        private void OnValidate()
        {
            currentCargoWeight = Mathf.Max(0f, currentCargoWeight);
            speedCondition = Mathf.Clamp01(speedCondition);
            accelerationCondition = Mathf.Clamp01(accelerationCondition);
            steeringCondition = Mathf.Clamp01(steeringCondition);

            if (cargoStats != null)
            {
                currentCargoWeight = Mathf.Min(currentCargoWeight, cargoStats.CargoCapacity);
            }
        }
    }
}
