using UnityEngine;

namespace ThePirate.Combat
{
    // Emiter nie ma własnego przeładowania ani parametrów obrażeń.
    [DisallowMultipleComponent]
    public sealed class Cannon : MonoBehaviour
    {
        [SerializeField] private Transform owner;
        [SerializeField] private Transform firePoint;
        [SerializeField] private CannonProjectile projectilePrefab;
        public Transform Muzzle => firePoint != null ? firePoint : transform;
        public float ProjectileRadius
        {
            get
            {
                if (projectilePrefab == null) return 0.15f;
                var sphere = projectilePrefab.GetComponent<SphereCollider>();
                Vector3 scale = projectilePrefab.transform.lossyScale;
                return sphere == null ? 0.15f : sphere.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            }
        }
        public bool Ready => isActiveAndEnabled && owner != null && projectilePrefab != null && projectilePrefab.enabled && projectilePrefab.gameObject.activeSelf;
        public void Configure(Transform ship, CannonProjectile prefab) { owner = ship; projectilePrefab = prefab; firePoint = transform; }
        public bool Emit(Vector3 direction, int damage, float speed, float lifetime, AmmunitionKind ammunition, bool gravity = false, float radius = 0f,
            bool damageAllies = false, bool damageOwner = false, float waterHeight = 0f)
        {
            if (!Ready || direction.sqrMagnitude < 0.001f) return false;
            var ship = owner.GetComponent<CombatShip>();
            if (ship != null && !ship.WeaponsAllowed) return false;
            var projectile = Instantiate(projectilePrefab, Muzzle.position, Quaternion.LookRotation(direction));
            projectile.Launch(owner, damage, speed, lifetime, ammunition, gravity, radius, damageAllies, damageOwner, waterHeight);
            if (ship != null) ship.RecordCombat();
            return true;
        }
    }
}
