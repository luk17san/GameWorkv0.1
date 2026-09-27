using UnityEngine;

namespace GameWork.Framework.Ships.Movement
{
    [CreateAssetMenu(
        fileName = "ShipCargoStats",
        menuName = "GameWork/Ships/Cargo Stats")]
    public sealed class ShipCargoStats : ScriptableObject
    {
        [Header("Capacity")]
        [SerializeField, Min(0.01f)] private float cargoCapacity = 1000f;

        [Header("Movement penalties")]
        [SerializeField, Range(0f, 0.99f)] private float performancePenaltyThreshold = 0.4f;
        [SerializeField, Range(0f, 1f)] private float maxSpeedPenaltyAtFullLoad = 0.15f;
        [SerializeField, Range(0f, 1f)] private float maxAccelerationPenaltyAtFullLoad = 0.35f;

        public float CargoCapacity => cargoCapacity;

        public bool CanAddCargo(float currentCargoWeight, float addedWeight)
        {
            if (currentCargoWeight < 0f || addedWeight < 0f)
            {
                return false;
            }

            return currentCargoWeight + addedWeight <= cargoCapacity;
        }

        public float GetLoadRatio(float currentCargoWeight)
        {
            return Mathf.Clamp01(currentCargoWeight / cargoCapacity);
        }

        public float GetPenaltyRatio(float currentCargoWeight)
        {
            float loadRatio = GetLoadRatio(currentCargoWeight);

            if (loadRatio <= performancePenaltyThreshold)
            {
                return 0f;
            }

            return Mathf.InverseLerp(performancePenaltyThreshold, 1f, loadRatio);
        }

        public float GetSpeedMultiplier(float currentCargoWeight)
        {
            return 1f - GetPenaltyRatio(currentCargoWeight) * maxSpeedPenaltyAtFullLoad;
        }

        public float GetAccelerationMultiplier(float currentCargoWeight)
        {
            return 1f - GetPenaltyRatio(currentCargoWeight) * maxAccelerationPenaltyAtFullLoad;
        }

        private void OnValidate()
        {
            cargoCapacity = Mathf.Max(0.01f, cargoCapacity);
            performancePenaltyThreshold = Mathf.Clamp(performancePenaltyThreshold, 0f, 0.99f);
            maxSpeedPenaltyAtFullLoad = Mathf.Clamp01(maxSpeedPenaltyAtFullLoad);
            maxAccelerationPenaltyAtFullLoad = Mathf.Clamp01(maxAccelerationPenaltyAtFullLoad);
        }
    }
}
