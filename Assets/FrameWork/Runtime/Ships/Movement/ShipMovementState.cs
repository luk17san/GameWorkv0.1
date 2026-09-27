using System;
using UnityEngine;

namespace GameWork.Framework.Ships.Movement
{
    public enum ShipDriveState
    {
        Stopped,
        Forward,
        Braking,
        Reverse
    }

    [Serializable]
    public sealed class ShipMovementState
    {
        [SerializeField] private float requestedTargetSpeed;
        [SerializeField] private float allowedTargetSpeed;
        [SerializeField] private float currentPropulsionSpeed;
        [SerializeField] private float effectiveMaxSpeed;
        [SerializeField] private float effectiveAcceleration;
        [SerializeField] private float currentTurnEfficiency;
        [SerializeField] private Vector3 externalVelocity;
        [SerializeField] private Vector3 finalVelocity;
        [SerializeField] private ShipDriveState driveState = ShipDriveState.Stopped;
        [SerializeField] private bool reverseArmed;

        public float RequestedTargetSpeed => requestedTargetSpeed;
        public float AllowedTargetSpeed => allowedTargetSpeed;
        public float CurrentPropulsionSpeed => currentPropulsionSpeed;
        public float EffectiveMaxSpeed => effectiveMaxSpeed;
        public float EffectiveAcceleration => effectiveAcceleration;
        public float CurrentTurnEfficiency => currentTurnEfficiency;
        public Vector3 ExternalVelocity => externalVelocity;
        public Vector3 FinalVelocity => finalVelocity;
        public ShipDriveState DriveState => driveState;
        public bool ReverseArmed => reverseArmed;

        internal void SetRequestedTargetSpeed(float value) => requestedTargetSpeed = value;
        internal void SetAllowedTargetSpeed(float value) => allowedTargetSpeed = value;
        internal void SetCurrentPropulsionSpeed(float value) => currentPropulsionSpeed = value;
        internal void SetEffectiveMaxSpeed(float value) => effectiveMaxSpeed = Mathf.Max(0f, value);
        internal void SetEffectiveAcceleration(float value) => effectiveAcceleration = Mathf.Max(0f, value);
        internal void SetCurrentTurnEfficiency(float value) => currentTurnEfficiency = Mathf.Clamp01(value);
        internal void SetExternalVelocity(Vector3 value) => externalVelocity = value;
        internal void SetFinalVelocity(Vector3 value) => finalVelocity = value;
        internal void SetDriveState(ShipDriveState value) => driveState = value;
        internal void SetReverseArmed(bool value) => reverseArmed = value;

        internal void Reset()
        {
            requestedTargetSpeed = 0f;
            allowedTargetSpeed = 0f;
            currentPropulsionSpeed = 0f;
            effectiveMaxSpeed = 0f;
            effectiveAcceleration = 0f;
            currentTurnEfficiency = 0f;
            externalVelocity = Vector3.zero;
            finalVelocity = Vector3.zero;
            driveState = ShipDriveState.Stopped;
            reverseArmed = false;
        }
    }
}
