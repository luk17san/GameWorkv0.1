using System.Collections.Generic;
using Framework.Health;
using UnityEngine;

namespace ThePirate.Combat
{
    [RequireComponent(typeof(Rigidbody), typeof(Health))]
    public sealed class ShipCollisionDamage : MonoBehaviour
    {
        [SerializeField, Min(0)] private float safeSpeed = 2f;
        [SerializeField, Min(0)] private float damageScale = 0.02f;
        [SerializeField, Range(0f, 1f)] private float impactDamageMultiplier = 0.1f;
        [SerializeField, Range(0f, 1f)] private float maxImpactHealthFraction = 0.25f;
        [SerializeField, Min(0.05f)] private float pairCooldown = 0.75f;
        [SerializeField, Min(0.1f)] private float hullResistance = 1f;
        [SerializeField, Min(0.1f)] private float bowAttack = 1.5f;
        [SerializeField, Min(0.1f)] private float sideAttack = 0.6f;
        [SerializeField, Min(0.1f)] private float sternAttack = 0.5f;
        private readonly Dictionary<Object, float> recent = new Dictionary<Object, float>();
        private readonly List<Object> expired = new List<Object>();
        private Rigidbody body;
        private Health hull;
        private void Awake() { body = GetComponent<Rigidbody>(); hull = GetComponent<Health>(); }
        private void OnCollisionEnter(Collision collision)
        {
            if (hull.IsDepleted || collision.contactCount == 0 || collision.collider.GetComponentInParent<CannonProjectile>() != null) return;
            var other = collision.collider.GetComponentInParent<ShipCollisionDamage>();
            var otherHull = collision.collider.GetComponentInParent<Health>();
            // The living ship also receives a callback when touching a defeated hull.
            if (otherHull != null && otherHull.IsDepleted) return;
            // Pierwszy callback blokuje parę w obu instancjach przed zadaniem obrażeń.
            Object key = collision.rigidbody != null ? (Object)collision.rigidbody : collision.collider;
            if (recent.TryGetValue(key, out float until) && Time.time < until) return;
            var contact = collision.GetContact(0);
            float speed = Mathf.Abs(Vector3.Dot(collision.relativeVelocity, contact.normal));
            if (speed <= safeSpeed) return;
            expired.Clear();
            foreach (var entry in recent) if (entry.Value <= Time.time) expired.Add(entry.Key);
            foreach (Object entry in expired) recent.Remove(entry);
            float cooldown = other != null ? Mathf.Max(pairCooldown, other.pairCooldown) : pairCooldown;
            recent[key] = Time.time + cooldown;
            if (other != null) other.recent[body] = Time.time + cooldown;
            float otherMass = collision.rigidbody != null && !collision.rigidbody.isKinematic ? collision.rigidbody.mass : body.mass;
            float reducedMass = body.mass * otherMass / Mathf.Max(0.01f, body.mass + otherMass);
            float force = (speed - safeSpeed) * (speed - safeSpeed) * reducedMass * damageScale;
            float ownAttack = Attack(contact.point);
            float enemyAttack = other != null ? other.Attack(contact.point) : 1f;
            int ownDamage = ImpactDamage(force * enemyAttack * Vulnerability(contact.point) / Mathf.Max(0.1f, hullResistance), hull.MaxHealth, impactDamageMultiplier, maxImpactHealthFraction);
            int otherDamage = other != null ? ImpactDamage(force * ownAttack * other.Vulnerability(contact.point) / Mathf.Max(0.1f, other.hullResistance), other.hull.MaxHealth, other.impactDamageMultiplier, other.maxImpactHealthFraction) : 0;
            hull.TakeDamage(ownDamage);
            if (other != null) other.hull.TakeDamage(otherDamage);
            else
            {
                var target = collision.collider.GetComponentInParent<Health>();
                if (target != null && target != hull) target.TakeDamage(ImpactDamage(force * ownAttack, target.MaxHealth, impactDamageMultiplier, maxImpactHealthFraction));
            }
        }
        private static int ImpactDamage(float rawDamage, int maxHealth, float multiplier, float healthFraction)
        {
            int limit = Mathf.FloorToInt(maxHealth * Mathf.Clamp01(healthFraction));
            return Mathf.CeilToInt(Mathf.Clamp(rawDamage * Mathf.Clamp01(multiplier), 0f, limit));
        }
        private float Facing(Vector3 point)
        {
            Vector3 local = transform.InverseTransformPoint(point);
            var box = GetComponent<BoxCollider>();
            if (box != null) { local -= box.center; local.x /= Mathf.Max(0.01f, box.size.x); local.z /= Mathf.Max(0.01f, box.size.z); }
            return Mathf.Abs(local.z) > Mathf.Abs(local.x) ? Mathf.Sign(local.z) : 0f;
        }
        private float Attack(Vector3 point) { float facing = Facing(point); return facing > 0 ? bowAttack : facing < 0 ? sternAttack : sideAttack; }
        private float Vulnerability(Vector3 point) => Facing(point) > 0 ? 0.65f : 1f;
    }
}
