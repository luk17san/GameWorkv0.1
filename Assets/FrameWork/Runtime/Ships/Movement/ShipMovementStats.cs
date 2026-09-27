using UnityEngine;

namespace GameWork.Framework.Ships.Movement
{
    [CreateAssetMenu(
        fileName = "ShipMovementStats",
        menuName = "GameWork/Ships/Movement Stats")]
    public sealed class ShipMovementStats : ScriptableObject
    {
        [Header("Forward movement")]
        [SerializeField, Min(0.01f)] private float maxForwardSpeed = 15f;
        [SerializeField, Min(0.01f)] private float acceleration = 4f;
        [SerializeField, Range(1f, 3f)] private float brakingMultiplier = 1.5f;

        [Header("Reverse movement")]
        [SerializeField, Range(0f, 1f)] private float maxReverseSpeedRatio = 0.2f;
        [SerializeField, Range(0f, 1f)] private float reverseAccelerationRatio = 0.5f;

        [Header("Steering")]
        [SerializeField, Min(0.01f)] private float baseTurnSpeed = 30f;
        [SerializeField, Range(0f, 1f)] private float turnEfficiencyAtStop = 0.6f;
        [SerializeField, Range(0.01f, 0.99f)] private float optimalTurnSpeedRatio = 0.4f;
        [SerializeField, Range(0f, 1f)] private float turnEfficiencyAtOptimalSpeed = 1f;
        [SerializeField, Range(0f, 1f)] private float turnEfficiencyAtMaxSpeed = 0.3f;

        public float MaxForwardSpeed => maxForwardSpeed;
        public float Acceleration => acceleration;
        public float BrakingMultiplier => brakingMultiplier;
        public float MaxReverseSpeed => maxForwardSpeed * maxReverseSpeedRatio;
        public float ReverseAcceleration => acceleration * reverseAccelerationRatio;
        public float BaseTurnSpeed => baseTurnSpeed;

        public float GetTurnEfficiency(float normalizedSpeed)
        {
            float speed = Mathf.Clamp01(Mathf.Abs(normalizedSpeed));

            if (speed <= optimalTurnSpeedRatio)
            {
                float t = speed / optimalTurnSpeedRatio;
                return Mathf.Lerp(turnEfficiencyAtStop, turnEfficiencyAtOptimalSpeed, t);
            }

            float highSpeedRange = 1f - optimalTurnSpeedRatio;
            float highSpeedT = (speed - optimalTurnSpeedRatio) / highSpeedRange;
            return Mathf.Lerp(turnEfficiencyAtOptimalSpeed, turnEfficiencyAtMaxSpeed, highSpeedT);
        }

        private void OnValidate()
        {
            maxForwardSpeed = Mathf.Max(0.01f, maxForwardSpeed);
            acceleration = Mathf.Max(0.01f, acceleration);
            brakingMultiplier = Mathf.Clamp(brakingMultiplier, 1f, 3f);
            maxReverseSpeedRatio = Mathf.Clamp01(maxReverseSpeedRatio);
            reverseAccelerationRatio = Mathf.Clamp01(reverseAccelerationRatio);
            baseTurnSpeed = Mathf.Max(0.01f, baseTurnSpeed);
            optimalTurnSpeedRatio = Mathf.Clamp(optimalTurnSpeedRatio, 0.01f, 0.99f);
        }
    }
}
