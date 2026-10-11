using System;

namespace Framework.Economy
{
    // Czysta logika waluty: bez zależności od statków i Unity.
    public sealed class GoldWallet
    {
        public int Balance { get; private set; }
        public event Action<int> Changed;

        public bool TryCredit(int amount)
        {
            if (amount <= 0 || amount > int.MaxValue - Balance) return false;
            SetBalance(Balance + amount);
            return true;
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0 || amount > Balance) return false;
            SetBalance(Balance - amount);
            return true;
        }

        // Wczytanie zastępuje saldo; nigdy nie dodaje go do obecnego.
        public bool TryRestore(int balance)
        {
            if (balance < 0) return false;
            SetBalance(balance);
            return true;
        }

        private void SetBalance(int value)
        {
            if (value == Balance) return;
            Balance = value;
            Changed?.Invoke(Balance);
        }
    }
}
