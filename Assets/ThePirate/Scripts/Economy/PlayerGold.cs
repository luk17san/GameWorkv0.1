using Framework.Economy;
using ThePirate.Combat;
using UnityEngine;

namespace ThePirate.Economy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatShip))]
    public sealed class PlayerGold : MonoBehaviour
    {
        [SerializeField, Tooltip("Podgląd salda podczas gry. Nowa gra zaczyna od zera.")]
        private int currentGold;
        public GoldWallet Wallet { get; } = new GoldWallet();
        public int Balance => Wallet.Balance;

        public static PlayerGold Ensure(CombatShip ship)
        {
            if (ship == null || ship.Team != 1) return null;
            var result = ship.GetComponent<PlayerGold>();
            return result != null ? result : ship.gameObject.AddComponent<PlayerGold>();
        }

        private void Awake()
        {
            currentGold = 0;
            Wallet.Changed += BalanceChanged;
        }

        private void BalanceChanged(int value) => currentGold = value;
        private void OnDestroy() => Wallet.Changed -= BalanceChanged;
    }
}
