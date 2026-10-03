using UnityEngine;

namespace ThePirate.Combat
{
    public enum ShipModuleKind { Battery, Sails, Crew, Rudder, Mast, Magazine }
    [DisallowMultipleComponent]
    public sealed class ShipDamageModule : MonoBehaviour
    {
        [SerializeField] private ShipModuleKind kind;
        [SerializeField, Min(1)] private int maxHealth = 100;
        [SerializeField] private int currentHealth = 100;
        public ShipModuleKind Kind => kind;
        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public float Fraction => Mathf.Clamp01((float)currentHealth / Mathf.Max(1, maxHealth));
        public bool Destroyed => currentHealth <= 0;
        private void Awake() => currentHealth = Mathf.Max(1, maxHealth);
        public void Configure(ShipModuleKind value) => kind = value;
        public void TakeDamage(int value)
        {
            if (value <= 0 || Destroyed) return;
            currentHealth = Mathf.Max(0, currentHealth - value);
            GetComponentInParent<CombatShip>()?.RecordCombat();
        }
        public void Repair(int value) { currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Max(0, value)); }
        public void RestoreCurrentHealth(int value) { currentHealth = Mathf.Clamp(value, 0, Mathf.Max(1, maxHealth)); }
    }
}
