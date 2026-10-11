using Framework.Health;
using ThePirate.Combat;
using UnityEngine;

namespace ThePirate.Economy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatShip))]
    public sealed class ShipGoldReward : MonoBehaviour
    {
        [SerializeField, Min(0), Tooltip("Jednorazowa nagroda za zatopienie. Zero wyłącza nagrodę.")]
        private int goldReward = 100;
        private Health hull;
        private CombatShip ship;
        private bool handled;

        public static ShipGoldReward Ensure(CombatShip target)
        {
            if (target == null || target.Team == 0 || target.Team == 1) return null;
            var result = target.GetComponent<ShipGoldReward>();
            return result != null ? result : target.gameObject.AddComponent<ShipGoldReward>();
        }

        private void Awake()
        {
            ship = GetComponent<CombatShip>();
            hull = GetComponent<Health>();
        }
        private void OnEnable() => hull.Damaged += DamageReceived;
        private void OnDisable() => hull.Damaged -= DamageReceived;

        private void DamageReceived(int amount)
        {
            // Damaged jest emitowane po zmianie HP, przed Depleted. Restore go nie emituje.
            if (handled || amount <= 0 || !hull.IsDepleted || ship.Team == 0 || ship.Team == 1) return;
            handled = true;
            if (goldReward <= 0) return;
            CombatShip player = null;
            foreach (CombatShip candidate in CombatShip.ActiveShips)
            {
                if (candidate == null || candidate.Team != 1 || !candidate.Alive
                    || candidate.gameObject.scene != gameObject.scene) continue;
                // Niejednoznaczna scena nie może przyznać nagrody przypadkowemu graczowi.
                if (player != null) return;
                player = candidate;
            }
            if (player == null) return;
            PlayerGold gold = PlayerGold.Ensure(player);
            if (gold.Wallet.TryCredit(goldReward))
                Debug.Log($"Nagroda: +{goldReward} złota. Saldo: {gold.Balance}.", player);
            else
                Debug.LogWarning("Nie przyznano złota: przekroczono maksymalne saldo.", player);
        }
    }
}
