using System;
using UnityEngine;

namespace Framework.Health
{
    [DisallowMultipleComponent]
    public sealed class Health : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maxHealth = 100;

        public int ConfiguredMaxHealth => Mathf.Max(1, maxHealth);
        public int BaseMaxHealth { get; private set; }
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public bool IsDepleted => CurrentHealth == 0;

        // Argumenty: aktualne i maksymalne zdrowie.
        public event Action<int, int> Changed;
        public event Action Depleted;
        public event Action<int> Damaged;

        private void Awake()
        {
            // Maksimum ustalamy na początku życia obiektu.
            BaseMaxHealth = Mathf.Max(1, maxHealth);
            MaxHealth = BaseMaxHealth;
            CurrentHealth = MaxHealth;
        }

        // Parametr runtime nie zmienia bazowej konfiguracji prefabu.
        public bool SetMaximumPreservingDamage(int value)
        {
            if (value < 1) return false;
            if (value == MaxHealth) return true;
            bool wasDepleted = IsDepleted;
            int missing = MaxHealth - CurrentHealth;
            MaxHealth = value;
            CurrentHealth = wasDepleted ? 0 : Mathf.Clamp(value - missing, 0, value);
            Changed?.Invoke(CurrentHealth, MaxHealth);
            if (!wasDepleted && IsDepleted) Depleted?.Invoke();
            return true;
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || IsDepleted)
                return;

            int previous = CurrentHealth;
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            bool depletedNow = IsDepleted;
            Damaged?.Invoke(previous - CurrentHealth);

            Changed?.Invoke(CurrentHealth, MaxHealth);
            if (depletedNow)
                Depleted?.Invoke();
        }

        public void RestoreCurrentHealth(int value)
        {
            int restored = Mathf.Clamp(value, 0, MaxHealth);
            if (restored == CurrentHealth) return;
            bool depletedNow = CurrentHealth > 0 && restored == 0;
            CurrentHealth = restored;
            Changed?.Invoke(CurrentHealth, MaxHealth);
            if (depletedNow) Depleted?.Invoke();
        }
    }
}
