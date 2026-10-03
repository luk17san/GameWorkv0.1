using GameWork.Framework.Ships.Movement;
using UnityEngine;

namespace ThePirate.Combat
{
    [RequireComponent(typeof(CombatShip))]
    public sealed class ShipConditionController : MonoBehaviour
    {
        private CombatShip ship;
        private ShipMovementController movement;
        private ShipDamageModule[] modules;
        private void Awake() { ship = GetComponent<CombatShip>(); movement = GetComponent<ShipMovementController>(); modules = GetComponentsInChildren<ShipDamageModule>(); }
        private void Update()
        {
            if (movement == null) return;
            float sails = 1, rudder = 1;
            foreach (var module in modules)
            {
                if (module.Kind == ShipModuleKind.Sails || module.Kind == ShipModuleKind.Mast) sails = Mathf.Min(sails, module.Fraction);
                if (module.Kind == ShipModuleKind.Rudder) rudder = Mathf.Min(rudder, module.Fraction);
            }
            movement.SetCondition(ship.Alive ? Mathf.Lerp(0.15f, 1, sails) : 0, ship.Alive ? Mathf.Lerp(0.25f, 1, sails) : 1, ship.Alive ? Mathf.Lerp(0.1f, 1, rudder) : 0);
        }
    }
}
