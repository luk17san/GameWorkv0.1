using Framework.Health;
using UnityEngine;

namespace ThePirate.Combat
{
    public enum AmmunitionKind { Cannonball, Mortar, SmallArms }
    public static class CombatDamage
    {
        public static Component Receiver(Collider collider)
        {
            var module = collider.GetComponentInParent<ShipDamageModule>();
            if (module != null && !module.Destroyed) return module;
            return collider.GetComponentInParent<Health>();
        }
        public static void Apply(Component receiver, int damage)
        {
            if (receiver is ShipDamageModule module) module.TakeDamage(damage);
            else if (receiver is Health health) health.TakeDamage(damage);
        }
        public static bool ClearRay(Vector3 start, Vector3 end, Transform owner, CombatShip target, Collider destination = null)
        {
            var delta = end - start;
            foreach (var hit in Physics.RaycastAll(start, delta.normalized, delta.magnitude, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.GetComponentInParent<CannonProjectile>() != null) continue;
                if (hit.collider == destination) continue;
                if (hit.transform.IsChildOf(owner)) continue;
                if (target != null && hit.collider.GetComponentInParent<CombatShip>() == target) continue;
                return false;
            }
            return true;
        }
    }
}
