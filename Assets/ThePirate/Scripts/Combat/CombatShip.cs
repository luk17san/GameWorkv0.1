using System.Collections.Generic;
using Framework.Health;
using ThePirate.AI;
using ThePirate.Ports;
using UnityEngine;

namespace ThePirate.Combat
{
    [RequireComponent(typeof(Health))]
    [DisallowMultipleComponent]
    public sealed class CombatShip : MonoBehaviour
    {
        private static readonly HashSet<CombatShip> ships = new HashSet<CombatShip>();
        public static IEnumerable<CombatShip> ActiveShips => ships;
        [SerializeField] private int team = 1;
        [SerializeField] private bool lightlyArmored = true;
        [SerializeField, Min(0f)] private float dockingCombatDelay = 10f;
        private float combatUntil = float.NegativeInfinity;
        public int Team => team;
        public bool LightlyArmored => lightlyArmored;
        public Health Hull { get; private set; }
        public bool Alive => isActiveAndEnabled && Hull != null && !Hull.IsDepleted;
        public bool IsDocked => GetComponent<ShipDocking>() != null && GetComponent<ShipDocking>().IsDocked;
        public bool WeaponsAllowed => Alive && !IsDocked;
        public float CombatRemaining => Mathf.Max(0f, combatUntil - Time.time);
        public bool DockingBlocked
        {
            get
            {
                if (UnderThreat()) RecordCombat();
                return CombatRemaining > 0f;
            }
        }
        private bool UnderThreat()
        {
            foreach (var enemy in ships)
            {
                if (!IsEnemy(enemy) || enemy.gameObject.scene != gameObject.scene) continue;
                var ai = enemy.GetComponent<EnemyShipAI>();
                if (ai != null && ai.isActiveAndEnabled && ai.Target == this &&
                    ai.CurrentState != EnemyShipState.Returning && ai.CurrentState != EnemyShipState.Destroyed && ai.CurrentState != EnemyShipState.Waiting) return true;
            }
            return false;
        }
        public void RecordCombat()
        {
            combatUntil = Mathf.Max(combatUntil, Time.time + dockingCombatDelay);
            var docking = GetComponent<ShipDocking>();
            if (docking != null && docking.IsDocked) docking.Undock();
        }
        public void RestoreCombat(float remaining) => combatUntil = Time.time + Mathf.Max(0f, remaining);
        private void DamageReceived(int amount) => RecordCombat();
        private void Awake()
        {
            Hull = GetComponent<Health>();
            Hull.Damaged += DamageReceived;
            ShipDocking.Ensure(this);
        }
        private void Start() => ShipSinking.Ensure(this);
        private void OnEnable() => ships.Add(this);
        private void OnDisable() => ships.Remove(this);
        private void OnDestroy() { if (Hull != null) Hull.Damaged -= DamageReceived; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() => ships.Clear();
        public bool IsEnemy(CombatShip other) => other != null && other != this && other.Alive && team != 0 && other.team != 0 && team != other.team;
        public void Configure(int value) => team = value;
    }
}
