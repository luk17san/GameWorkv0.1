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

        private Rigidbody shipRigidbody;
        private ShipMovementInput movementInput;
        private float steeringInput;
        private bool brakeWasHeld;

        public ShipMovementState State => state;

        private void Awake()
        {
            shipRigidbody = GetComponent<Rigidbody>();
            movementInput = GetComponent<ShipMovementInput>();
            state.Reset();
        }

        private void Update()
        {
            if (movementStats == null)
            {
                steeringInput = 0f;
                return;
            }

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

            CalculateEffectiveStats();
            ClampAllowedTargetSpeed();
            UpdateCurrentSpeed();
            Quaternion rotation = ApplySteering();
            ApplyMovement(rotation);
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
