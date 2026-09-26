using System;
using UnityEngine;

namespace Framework.Health
{
    [DisallowMultipleComponent]
    public sealed class Health : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maxHealth = 100;

        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public bool IsDepleted => CurrentHealth == 0;

        // Argumenty: aktualne i maksymalne zdrowie.
        public event Action<int, int> Changed;
        public event Action Depleted;

        private void Awake()
        {
            // Maksimum ustalamy na początku życia obiektu.
            MaxHealth = Mathf.Max(1, maxHealth);
            CurrentHealth = MaxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || IsDepleted)
                return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            bool depletedNow = IsDepleted;

            Changed?.Invoke(CurrentHealth, MaxHealth);
            if (depletedNow)
                Depleted?.Invoke();
        }
    }
}
