using System.Collections;
using UnityEngine;

namespace ThePirate.Combat
{
    [RequireComponent(typeof(ShipDamageModule))]
    [DisallowMultipleComponent]
    public sealed class WeaponBattery : MonoBehaviour
    {
        [SerializeField, Min(0)] private float minimumRange = 8f;
        [SerializeField, Min(1)] private float maximumRange = 80f;
        [SerializeField, Range(1, 360)] private float firingArc = 70f;
        [SerializeField, Min(1)] private int damage = 25;
        [SerializeField, Min(1)] private float projectileSpeed = 35f;
        [SerializeField, Min(0.1f)] private float reloadTime = 6f;
        [SerializeField, Range(0, 10)] private float spread = 0.5f;
        [SerializeField] private Vector2 salvoDelay = new Vector2(0.001f, 0.01f);
        [SerializeField] private AmmunitionKind ammunition;
        private Cannon[] cannons;
        private CombatShip ship;
        private ShipDamageModule condition;
        private float nextFireTime;
        private bool firing;
        private float reloadDuration;
        public float ReloadDuration => reloadDuration;
        public int CannonCount => cannons != null ? cannons.Length : 0;
        public float MinimumRange => minimumRange;
        public float MaximumRange => maximumRange;
        public float FiringArc => firingArc;
        public float ReloadRemaining => Mathf.Max(0, nextFireTime - Time.time);
        public void RestoreReload(float seconds)
        {
            StopAllCoroutines();
            firing = false;
            reloadDuration = Mathf.Max(0f, seconds);
            nextFireTime = Time.time + reloadDuration;
        }
        public bool Ready => isActiveAndEnabled && ship != null && ship.WeaponsAllowed && condition != null && !condition.Destroyed && !firing && ReloadRemaining <= 0;
        private void Awake() { cannons = GetComponentsInChildren<Cannon>(); ship = GetComponentInParent<CombatShip>(); condition = GetComponent<ShipDamageModule>(); }
        private void OnDisable() { StopAllCoroutines(); firing = false; }
        public bool Contains(Vector3 target)
        {
            Vector3 delta = Vector3.ProjectOnPlane(target - transform.position, Vector3.up);
            return delta.sqrMagnitude > 0.001f && delta.magnitude >= minimumRange && delta.magnitude <= maximumRange
                && Vector3.Angle(Vector3.ProjectOnPlane(transform.forward, Vector3.up), delta) <= firingArc * 0.5f;
        }
        public float Alignment(Vector3 target) => Vector3.Angle(Vector3.ProjectOnPlane(transform.forward, Vector3.up), Vector3.ProjectOnPlane(target - transform.position, Vector3.up));
        private bool Safe(Cannon cannon, Vector3 direction, float distance, bool automatic, CombatShip target)
        {
            if (cannon == null || !cannon.Ready) return false;
            var start = cannon.Muzzle.position;
            // Raycast nie wykrywa collidera zawierającego początek promienia.
            float radius = Mathf.Max(0.01f, cannon.ProjectileRadius);
            foreach (var collider in Physics.OverlapSphere(start, radius, ~0, QueryTriggerInteraction.Ignore))
                if (collider.transform.IsChildOf(ship.transform)) return false;
            foreach (var hit in Physics.SphereCastAll(start, radius, direction, distance, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.GetComponentInParent<CannonProjectile>() != null) continue;
                if (hit.transform.IsChildOf(ship.transform)) return false;
                if (automatic && (target == null || hit.collider.GetComponentInParent<CombatShip>() != target)) return false;
            }
            return true;
        }
        public bool CanFire(Vector3 target, bool automatic = false, CombatShip enemy = null)
        {
            if (!Ready || !Contains(target)) return false;
            if (automatic && !ship.IsEnemy(enemy)) return false;
            var direction = Vector3.ProjectOnPlane(target - transform.position, Vector3.up).normalized;
            foreach (var cannon in cannons)
                if (Safe(cannon, direction, automatic ? maximumRange : Vector3.Distance(transform.position, target), automatic, enemy)) return true;
            return false;
        }
        public bool TryFire(Vector3 target, bool automatic = false, CombatShip enemy = null)
        {
            if (Time.timeScale <= 0 || !CanFire(target, automatic, enemy)) return false;
            ship.RecordCombat();
            if (enemy != null) enemy.RecordCombat();
            float penalty = condition.Fraction <= 0.25f ? 2.5f : condition.Fraction <= 0.5f ? 1.5f : 1f;
            reloadDuration = reloadTime * penalty;
            nextFireTime = Time.time + reloadDuration;
            firing = true;
            StartCoroutine(Salvo(target, automatic, enemy, penalty));
            return true;
        }
        private IEnumerator Salvo(Vector3 target, bool automatic, CombatShip enemy, float penalty)
        {
            var direction = Vector3.ProjectOnPlane(target - transform.position, Vector3.up).normalized;
            float distance = Vector3.Distance(transform.position, target);
            // Opóźnienia bezwzględne względem początku salwy, nie suma opóźnień dział.
            float start = Time.time;
            var delays = new float[cannons.Length];
            var done = new bool[cannons.Length];
            for (int i = 0; i < delays.Length; i++) delays[i] = Random.Range(salvoDelay.x, salvoDelay.y);
            int remaining = cannons.Length;
            while (remaining > 0 && ship.WeaponsAllowed && !condition.Destroyed)
            {
                if (automatic && !ship.IsEnemy(enemy)) break;
                for (int i = 0; i < cannons.Length; i++)
                {
                    if (done[i] || Time.time - start < delays[i]) continue;
                    done[i] = true; remaining--;
                    Vector3 shot = Quaternion.AngleAxis(Random.Range(-spread, spread) * penalty, Vector3.up) * direction;
                    if (Safe(cannons[i], shot, automatic ? maximumRange : distance, automatic, enemy))
                        cannons[i].Emit(shot, damage, projectileSpeed, maximumRange / projectileSpeed, ammunition);
                }
                yield return null;
            }
            firing = false;
        }
    }
}
