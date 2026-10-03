using UnityEngine;

namespace ThePirate.Combat
{
    [RequireComponent(typeof(CombatShip))]
    public sealed class ShipAutoFire : MonoBehaviour
    {
        [SerializeField] private bool automaticFire;
        [SerializeField, Min(0.05f)] private float scanInterval = 0.25f;
        private CombatShip ship;
        private WeaponBattery[] batteries;
        private float nextScan;
        private void Awake() { ship = GetComponent<CombatShip>(); batteries = GetComponentsInChildren<WeaponBattery>(); }
        private void Update()
        {
            if (!automaticFire || !ship.Alive || Time.timeScale <= 0 || Time.time < nextScan) return;
            nextScan = Time.time + scanInterval;
            foreach (var battery in batteries)
            {
                if (!battery.Ready) continue;
                CombatShip best = null; float score = float.MaxValue;
                foreach (var enemy in CombatShip.ActiveShips)
                {
                    if (!ship.IsEnemy(enemy) || !battery.CanFire(enemy.transform.position, true, enemy)) continue;
                    float distance = (enemy.transform.position - battery.transform.position).sqrMagnitude;
                    if (distance < score) { score = distance; best = enemy; }
                }
                if (best != null) battery.TryFire(best.transform.position, true, best);
            }
        }
    }
}
