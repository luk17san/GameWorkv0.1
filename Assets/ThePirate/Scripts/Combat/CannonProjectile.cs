using System.Collections.Generic;
using UnityEngine;

namespace ThePirate.Combat
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    [DisallowMultipleComponent]
    public sealed class CannonProjectile : MonoBehaviour
    {
        [SerializeField] private GameObject impactEffect;
        private Rigidbody body;
        private SphereCollider hitCollider;
        private Transform owner;
        private int damage;
        private bool launched, hasHit;
        private float blastRadius;
        private bool damageAllies, damageOwner;
        private float waterHeight;
        private Vector3 previousPosition;
        public int Team { get; private set; }
        public AmmunitionKind Ammunition { get; private set; }
        private void Awake()
        {
            body = GetComponent<Rigidbody>(); hitCollider = GetComponent<SphereCollider>();
            hitCollider.isTrigger = false; body.useGravity = false; body.isKinematic = false;
            body.linearDamping = 0; body.angularDamping = 0;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        public void Launch(Transform shotOwner, int shotDamage, float speed, float lifetime,
            AmmunitionKind ammunition = AmmunitionKind.Cannonball, bool gravity = false, float radius = 0f,
            bool allies = false, bool self = false, float surfaceHeight = 0f)
        {
            if (launched) return;
            owner = shotOwner; damage = Mathf.Max(1, shotDamage); launched = true;
            Ammunition = ammunition; blastRadius = radius;
            damageAllies = allies; damageOwner = self; waterHeight = surfaceHeight;
            previousPosition = body.position;
            var ship = owner != null ? owner.GetComponent<CombatShip>() : null;
            Team = ship != null ? ship.Team : 0;
            if (owner != null)
                foreach (Collider collider in owner.GetComponentsInChildren<Collider>(true)) Physics.IgnoreCollision(hitCollider, collider);
            // Pociski jednej salwy nie zderzają się ze sobą.
            foreach (var projectile in FindObjectsByType<CannonProjectile>())
                if (projectile != this && projectile.owner == owner && projectile.hitCollider != null) Physics.IgnoreCollision(hitCollider, projectile.hitCollider);
            body.useGravity = gravity;
            body.linearVelocity = transform.forward * Mathf.Max(0.1f, speed);
            Destroy(gameObject, Mathf.Max(0.1f, lifetime));
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (!launched || hasHit || (owner != null && collision.transform.IsChildOf(owner))) return;
            Impact(collision.collider);
        }
        private void FixedUpdate()
        {
            if (!launched || hasHit || Ammunition != AmmunitionKind.Mortar) return;
            Vector3 current = body.position;
            Vector3 delta = current - previousPosition;
            float nearest = float.PositiveInfinity;
            Collider obstruction = null;
            Vector3 contact = current;
            float radius = hitCollider.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y), Mathf.Abs(transform.lossyScale.z));
            if (delta.sqrMagnitude > 0.000001f)
                foreach (var hit in Physics.SphereCastAll(previousPosition, radius, delta.normalized, delta.magnitude, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (hit.collider == hitCollider || (owner != null && hit.transform.IsChildOf(owner))) continue;
                    var other = hit.collider.GetComponentInParent<CannonProjectile>();
                    if (other != null && other.owner == owner) continue;
                    if (hit.distance >= nearest) continue;
                    nearest = hit.distance; obstruction = hit.collider;
                    contact = previousPosition + delta.normalized * hit.distance;
                }
            // Woda nie potrzebuje collidera. Wybieramy pierwsze trafienie na odcinku.
            if (body.linearVelocity.y < 0f && current.y <= waterHeight)
            {
                float fraction = previousPosition.y > waterHeight ? (previousPosition.y - waterHeight) / (previousPosition.y - current.y) : 0f;
                Vector3 surface = Vector3.Lerp(previousPosition, current, fraction);
                surface.y = waterHeight;
                if (Vector3.Distance(previousPosition, surface) < nearest) { contact = surface; obstruction = null; nearest = 0f; }
            }
            if (!float.IsPositiveInfinity(nearest)) { body.position = contact; transform.position = contact; Impact(obstruction); }
            previousPosition = current;
        }
        private void Impact(Collider direct)
        {
            hasHit = true; hitCollider.enabled = false; body.linearVelocity = Vector3.zero;
            if (blastRadius > 0)
            {
                if (Ammunition == AmmunitionKind.Mortar)
                {
                    ExplodeMortar();
                }
                else
                {
                var receivers = new HashSet<Component>();
                foreach (var collider in Physics.OverlapSphere(transform.position, blastRadius, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (owner != null && collider.transform.IsChildOf(owner)) continue;
                    var receiver = CombatDamage.Receiver(collider);
                    if (receiver == null || receivers.Contains(receiver)) continue;
                    var targetShip = collider.GetComponentInParent<CombatShip>();
                    if (!CombatDamage.ClearRay(transform.position + Vector3.up * 0.2f, collider.bounds.center, transform, targetShip, collider)) continue;
                    receivers.Add(receiver); CombatDamage.Apply(receiver, damage);
                }
                }
            }
            else if (direct != null) CombatDamage.Apply(CombatDamage.Receiver(direct), damage);
            if (impactEffect != null) Destroy(Instantiate(impactEffect, transform.position, Quaternion.identity), 5f);
            Destroy(gameObject);
        }
        private void ExplodeMortar()
        {
            // Wiele colliderów jednego statku daje jedno trafienie kadłuba.
            var distances = new Dictionary<CombatShip, float>();
            foreach (var collider in Physics.OverlapSphere(transform.position, blastRadius, ~0, QueryTriggerInteraction.Ignore))
            {
                var target = collider.GetComponentInParent<CombatShip>();
                if (target == null || !target.Alive) continue;
                bool own = owner != null && target.transform == owner;
                if (own ? !damageOwner : (target.Team == 0 || Team == 0 || (target.Team == Team && !damageAllies))) continue;
                float distance = Vector3.Distance(transform.position, collider.ClosestPoint(transform.position));
                if (!distances.TryGetValue(target, out float old) || distance < old) distances[target] = distance;
            }
            foreach (var pair in distances)
            {
                int amount = Mathf.RoundToInt(damage * Mathf.Clamp01(1f - pair.Value / blastRadius));
                if (amount > 0) pair.Key.Hull.TakeDamage(amount);
            }
        }
    }
}
