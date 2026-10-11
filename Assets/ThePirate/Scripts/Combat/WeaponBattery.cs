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
        [Tooltip("Prędkość pozioma salwy; odległość kursora wyznacza czas lotu i opadnięcie na wodę.")]
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
        private int upgradeDamageBonus;
        private float upgradeReloadMultiplier = 1f;
        public int BaseDamage => damage;
        public float BaseReloadTime => reloadTime;
        private float spawnDamageMultiplier = 1f, spawnReloadMultiplier = 1f;
        public void SetSpawnMultipliers(float damageValue, float reloadValue)
        {
            spawnDamageMultiplier = Mathf.Clamp(damageValue, float.Epsilon, 10f);
            spawnReloadMultiplier = Mathf.Clamp(reloadValue, .1f, 10f);
        }
        public int EffectiveDamage => Mathf.Max(1, Mathf.RoundToInt((damage + upgradeDamageBonus) * spawnDamageMultiplier));
        public float EffectiveReloadTime => reloadTime * upgradeReloadMultiplier * spawnReloadMultiplier;
        public bool SetUpgrade(int bonus, float reloadMultiplier)
        {
            if (bonus < 0 || (long)damage + bonus > int.MaxValue || float.IsNaN(reloadMultiplier)
                || float.IsInfinity(reloadMultiplier) || reloadMultiplier < 0.1f || reloadMultiplier > 1f) return false;
            upgradeDamageBonus = bonus; upgradeReloadMultiplier = reloadMultiplier;
            // nextFireTime i reloadDuration pozostają bez zmian.
            return true;
        }
        public float ReloadDuration => reloadDuration;
        public int CannonCount => cannons != null ? cannons.Length : 0;
        public float WaterHeight => ship != null && ship.TryGetComponent<ShipCombatController>(out var combat) ? combat.WaterHeight : 0f;
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
        public bool TryGetTrajectory(int index, Vector3 target, out Vector3 origin, out Vector3 velocity, out float duration)
        {
            origin = velocity = Vector3.zero; duration = 0f;
            if (cannons == null || index < 0 || index >= cannons.Length || cannons[index] == null || Physics.gravity.y >= -0.01f) return false;
            Vector3 center = Vector3.zero;
            int count = 0;
            foreach (var cannon in cannons)
                if (cannon != null) { center += cannon.Muzzle.position; count++; }
            if (count == 0) return false;
            center /= count;
            origin = cannons[index].Muzzle.position;
            // Wspólne przesunięcie poziome zachowuje układ salwy zamiast skupiać kule w jednym punkcie.
            Vector3 delta = Vector3.ProjectOnPlane(target - center, Vector3.up);
            duration = Mathf.Max(0.1f, delta.magnitude / Mathf.Max(1f, projectileSpeed));
            velocity = (delta - 0.5f * Physics.gravity * duration * duration) / duration;
            velocity.y = (WaterHeight - origin.y - 0.5f * Physics.gravity.y * duration * duration) / duration;
            return true;
        }
        private bool Safe(Cannon cannon, Vector3 velocity, float duration, bool automatic, CombatShip target)
        {
            if (cannon == null || !cannon.Ready) return false;
            var start = cannon.Muzzle.position;
            // Raycast nie wykrywa collidera zawierającego początek promienia.
            float radius = Mathf.Max(0.01f, cannon.ProjectileRadius);
            foreach (var collider in Physics.OverlapSphere(start, radius, ~0, QueryTriggerInteraction.Ignore))
                if (collider.transform.IsChildOf(ship.transform)) return false;
            // Sprawdzaj rzeczywisty łuk, nie prostą linię ponad/pod przeszkodami.
            const int segments = 24;
            for (int i = 1; i <= segments; i++)
            {
                float time = duration * i / segments;
                Vector3 end = cannon.Muzzle.position + velocity * time + 0.5f * Physics.gravity * time * time;
                Vector3 delta = end - start;
                if (delta.sqrMagnitude > 0.000001f)
                    foreach (var hit in Physics.SphereCastAll(start, radius, delta.normalized, delta.magnitude, ~0, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.collider.GetComponentInParent<CannonProjectile>() != null) continue;
                        if (hit.transform.IsChildOf(ship.transform)) return false;
                        if (automatic && (target == null || hit.collider.GetComponentInParent<CombatShip>() != target)) return false;
                    }
                start = end;
            }
            return true;
        }
        public bool CanFire(Vector3 target, bool automatic = false, CombatShip enemy = null)
        {
            if (!Ready || !Contains(target)) return false;
            if (automatic && !ship.IsEnemy(enemy)) return false;
            for (int i = 0; i < CannonCount; i++)
                if (TryGetTrajectory(i, target, out _, out var velocity, out var duration)
                    && Safe(cannons[i], velocity, duration, automatic, enemy)) return true;
            return false;
        }
        public bool TryFire(Vector3 target, bool automatic = false, CombatShip enemy = null)
        {
            if (Time.timeScale <= 0 || !CanFire(target, automatic, enemy)) return false;
            ship.RecordCombat();
            if (enemy != null) enemy.RecordCombat();
            float penalty = condition.Fraction <= 0.25f ? 2.5f : condition.Fraction <= 0.5f ? 1.5f : 1f;
            reloadDuration = EffectiveReloadTime * penalty;
            nextFireTime = Time.time + reloadDuration;
            firing = true;
            StartCoroutine(Salvo(target, automatic, enemy, penalty));
            return true;
        }
        private IEnumerator Salvo(Vector3 target, bool automatic, CombatShip enemy, float penalty)
        {
            // Opóźnienia bezwzględne względem początku salwy, nie suma opóźnień dział.
            float start = Time.time;
            var delays = new float[cannons.Length];
            var done = new bool[cannons.Length];
            var velocities = new Vector3[cannons.Length];
            var durations = new float[cannons.Length];
            for (int i = 0; i < delays.Length; i++)
            {
                delays[i] = Random.Range(salvoDelay.x, salvoDelay.y);
                TryGetTrajectory(i, target, out _, out velocities[i], out durations[i]);
                velocities[i] = Quaternion.AngleAxis(Random.Range(-spread, spread) * penalty, Vector3.up) * velocities[i];
            }
            int remaining = cannons.Length;
            while (remaining > 0 && ship.WeaponsAllowed && !condition.Destroyed)
            {
                if (automatic && !ship.IsEnemy(enemy)) break;
                for (int i = 0; i < cannons.Length; i++)
                {
                    if (done[i] || Time.time - start < delays[i]) continue;
                    done[i] = true; remaining--;
                    Vector3 shot = velocities[i];
                    if (durations[i] > 0f && Safe(cannons[i], shot, durations[i], automatic, enemy))
                        cannons[i].Emit(shot.normalized, EffectiveDamage, shot.magnitude, durations[i] + 2f, ammunition, true, waterHeight: WaterHeight);
                }
                yield return null;
            }
            firing = false;
        }
    }
}
