using System;
using ThePirate.Combat;
using UnityEngine;

namespace ThePirate.Ships
{
    [DisallowMultipleComponent]
    public sealed class ShipIdentity : MonoBehaviour
    {
        [SerializeField] private string typeId = "starter";
        private string instanceId;
        public string TypeId => typeId;
        public string InstanceId => instanceId;
        private void Awake() => instanceId = Guid.NewGuid().ToString("N");
        public void Configure(string type, string instance = null)
        { typeId = type; instanceId = string.IsNullOrEmpty(instance) ? Guid.NewGuid().ToString("N") : instance; }
        public static ShipIdentity Ensure(CombatShip ship)
        {
            if (ship == null || ship.Team != 1) return null;
            return ship.GetComponent<ShipIdentity>() ?? ship.gameObject.AddComponent<ShipIdentity>();
        }
    }
}
