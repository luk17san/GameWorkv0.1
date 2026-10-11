using Framework.Inventory;
using GameWork.Framework.Ships.Movement;
using ThePirate.Combat;
using UnityEngine;

namespace ThePirate.Ships
{
    [CreateAssetMenu(fileName = "ShipDefinition", menuName = "GameWork/Ships/Ship Definition")]
    public sealed class ShipDefinition : ScriptableObject
    {
        [SerializeField] private string typeId;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private GameObject prefab;
        [SerializeField, Min(1)] private int price = 200;
        public string Id => typeId;
        public string DisplayName => displayName;
        public string Description => description;
        public GameObject Prefab => prefab;
        public int Price => price;
        public int HullHealth => prefab != null && prefab.GetComponent<Framework.Health.Health>() != null ? prefab.GetComponent<Framework.Health.Health>().ConfiguredMaxHealth : 0;
        public float Capacity => prefab != null && prefab.GetComponent<ShipMovementController>() != null ? prefab.GetComponent<ShipMovementController>().CargoCapacity : 0f;
        public float Speed => prefab != null && prefab.GetComponent<ShipMovementController>() != null ? prefab.GetComponent<ShipMovementController>().BaseForwardSpeed : 0f;
        public bool Validate(out string error)
        {
            error = null;
            if (!ResourceDefinition.ValidId(typeId) || string.IsNullOrWhiteSpace(displayName) || price < 1)
                error = "Projekt wymaga stałego ID, nazwy i dodatniej ceny.";
            else if (prefab == null || !prefab.activeSelf || prefab.GetComponent<CombatShip>() == null || prefab.GetComponent<CombatShip>().Team != 1
                || prefab.GetComponentsInChildren<CombatShip>(true).Length != 1 || prefab.GetComponent<Rigidbody>() == null || prefab.GetComponent<BoxCollider>() == null
                || HullHealth < 1 || prefab.GetComponent<ShipMovementController>() == null || !prefab.GetComponent<ShipMovementController>().HasMovementConfiguration
                || !prefab.GetComponent<ShipMovementController>().HasCargoCapacity || !FinitePositive(Capacity) || !FinitePositive(Speed))
                error = "Prefab projektu wymaga aktywnego gracza z kadłubem, Rigidbody, BoxCollider i konfiguracją ruchu/ładowni.";
            return error == null;
        }
        private static bool FinitePositive(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
    }
}
