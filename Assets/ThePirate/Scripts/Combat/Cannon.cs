using UnityEngine;

namespace ThePirate.Combat
{
    [DisallowMultipleComponent]
    public sealed class Cannon : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Główny obiekt statku. Pocisk ignoruje collidery w jego hierarchii.")]
        [SerializeField] private Transform owner;
        [SerializeField] private Transform firePoint;
        [SerializeField] private CannonProjectile projectilePrefab;

        [Header("Shot")]
        [SerializeField, Min(1)] private int damage = 25;
        [SerializeField, Min(0.1f)] private float projectileSpeed = 20f;
        [SerializeField, Min(0.1f)] private float projectileLifetime = 5f;
        [SerializeField, Min(0.1f)] private float reloadTime = 1.5f;

        private float nextShotTime;

        public float ReloadRemaining => Mathf.Max(0f, nextShotTime - Time.time);

        public bool TryFire()
        {
            if (!isActiveAndEnabled || Time.timeScale <= 0f || ReloadRemaining > 0f)
                return false;

            if (owner == null || firePoint == null || projectilePrefab == null)
            {
                Debug.LogWarning("Cannon: przypisz Owner, Fire Point i Projectile Prefab.", this);
                return false;
            }

            if (!projectilePrefab.gameObject.activeSelf || !projectilePrefab.enabled)
            {
                Debug.LogWarning("Cannon: prefab pocisku i jego komponent muszą być aktywne.", this);
                return false;
            }

            CannonProjectile projectile = Instantiate(projectilePrefab,
                firePoint.position, firePoint.rotation);
            projectile.Launch(owner, Mathf.Max(1, damage),
                Mathf.Max(0.1f, projectileSpeed), Mathf.Max(0.1f, projectileLifetime));
            nextShotTime = Time.time + Mathf.Max(0.1f, reloadTime);
            return true;
        }
    }
}
