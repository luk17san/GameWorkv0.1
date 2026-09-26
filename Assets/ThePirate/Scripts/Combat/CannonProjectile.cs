using Framework.Health;
using UnityEngine;

namespace ThePirate.Combat
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    [DisallowMultipleComponent]
    public sealed class CannonProjectile : MonoBehaviour
    {
        private Rigidbody body;
        private SphereCollider hitCollider;
        private Transform owner;
        private int damage;
        private bool launched;
        private bool hasHit;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            hitCollider = GetComponent<SphereCollider>();
            hitCollider.isTrigger = false;
            body.useGravity = false;
            body.isKinematic = false;
            body.linearDamping = 0f;
            body.angularDamping = 0f;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        public void Launch(Transform shotOwner, int shotDamage, float speed, float lifetime)
        {
            if (launched)
                return;

            owner = shotOwner;
            damage = Mathf.Max(1, shotDamage);
            launched = true;

            // Wykluczamy własny kadłub, zanim fizyka wykona pierwszy krok.
            if (owner != null)
            {
                foreach (Collider ownerCollider in owner.GetComponentsInChildren<Collider>(true))
                    Physics.IgnoreCollision(hitCollider, ownerCollider);
            }

            body.linearVelocity = transform.forward * Mathf.Max(0.1f, speed);
            Destroy(gameObject, Mathf.Max(0.1f, lifetime));
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!launched || hasHit)
                return;

            // Dodatkowa ochrona, np. dla collidera dodanego po wystrzale.
            if (owner != null && collision.transform.IsChildOf(owner))
                return;

            // Destroy wykonuje się później, więc blokujemy kolejne trafienia już teraz.
            hasHit = true;
            hitCollider.enabled = false;
            body.linearVelocity = Vector3.zero;

            Health targetHealth = collision.collider.GetComponentInParent<Health>();
            if (targetHealth != null)
                targetHealth.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}
