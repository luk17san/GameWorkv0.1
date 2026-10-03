using UnityEngine;

namespace ThePirate.Combat
{
    [RequireComponent(typeof(CombatShip))]
    public sealed class CrewAutoFireSystem : MonoBehaviour
    {
        [SerializeField, Min(1)] private float range = 18f;
        [SerializeField, Min(0.1f)] private float reloadTime = 2f;
        [SerializeField, Min(1)] private int damage = 4;
        [SerializeField] private Transform firePoint;
        [SerializeField] private ShipDamageModule crewHealth;
        private CombatShip ship;
        private float nextShot;
        private void Awake() => ship = GetComponent<CombatShip>();
        private void Update()
        {
            if (!ship.WeaponsAllowed || Time.timeScale <= 0 || Time.time < nextShot || (crewHealth != null && crewHealth.Destroyed)) return;
            nextShot = Time.time + reloadTime;
            Vector3 start = firePoint != null ? firePoint.position : transform.position + Vector3.up;
            CombatShip selected = null; Component receiver = null; float best = range * range;
            foreach (var enemy in CombatShip.ActiveShips)
            {
                if (!ship.IsEnemy(enemy)) continue;
                ShipDamageModule targetCrew = null;
                foreach (var module in enemy.GetComponentsInChildren<ShipDamageModule>())
                    if (module.Kind == ShipModuleKind.Crew && !module.Destroyed) { targetCrew = module; break; }
                if (targetCrew == null && !enemy.LightlyArmored) continue;
                var point = targetCrew != null ? targetCrew.transform.position : enemy.transform.position + Vector3.up;
                float distance = (point - start).sqrMagnitude;
                if (distance >= best || !CombatDamage.ClearRay(start, point, transform, enemy)) continue;
                best = distance; selected = enemy;
                receiver = targetCrew != null ? (Component)targetCrew : enemy.Hull;
            }
            // Broń ręczna jest osobnym atakiem natychmiastowym z kontrolą widoczności.
            if (selected != null) { ship.RecordCombat(); selected.RecordCombat(); CombatDamage.Apply(receiver, damage); }
        }
    }
}
