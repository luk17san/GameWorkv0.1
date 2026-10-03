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
            AmmunitionKind ammunition = AmmunitionKind.Cannonball, bool gravity = false, float radius = 0f)
        {
            if (launched) return;
            owner = shotOwner; damage = Mathf.Max(1, shotDamage); launched = true;
            Ammunition = ammunition; blastRadius = radius;
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
            // Moździerz detonuje również na poziomie wody bez collidera wody.
            if (launched && !hasHit && Ammunition == AmmunitionKind.Mortar && body.linearVelocity.y < 0 && transform.position.y <= 0f) Impact(null);
        }
        private void Impact(Collider direct)
        {
            hasHit = true; hitCollider.enabled = false; body.linearVelocity = Vector3.zero;
            if (blastRadius > 0)
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
            else if (direct != null) CombatDamage.Apply(CombatDamage.Receiver(direct), damage);
            if (impactEffect != null) Destroy(Instantiate(impactEffect, transform.position, Quaternion.identity), 5f);
            Destroy(gameObject);
        }
    }
}
