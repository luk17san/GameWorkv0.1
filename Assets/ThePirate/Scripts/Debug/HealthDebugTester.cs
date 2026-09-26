using Framework.Health;
using UnityEngine;

namespace ThePirate.Debugging
{
    // Pomoc do sceny testowej. Docelowo obrażenia zada pocisk.
    [RequireComponent(typeof(Health))]
    [DisallowMultipleComponent]
    public sealed class HealthDebugTester : MonoBehaviour
    {
        [SerializeField, Min(1)] private int testDamage = 25;
        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            health.Changed += OnHealthChanged;
            health.Depleted += OnDepleted;
        }

        private void Start()
        {
            LogState();
        }

        private void OnDisable()
        {
            health.Changed -= OnHealthChanged;
            health.Depleted -= OnDepleted;
        }

        [ContextMenu("Test/Apply Damage")]
        private void ApplyTestDamage()
        {
            if (!CanTest())
                return;

            health.TakeDamage(testDamage);
            LogState();
        }

        [ContextMenu("Test/Apply Lethal Damage")]
        private void ApplyLethalDamage()
        {
            if (!CanTest())
                return;

            health.TakeDamage(int.MaxValue);
            LogState();
        }

        [ContextMenu("Test/Log State")]
        private void LogState()
        {
            if (!CanTest())
                return;

            Debug.Log($"{name}: HP {health.CurrentHealth}/{health.MaxHealth}, "
                + $"wyczerpane: {health.IsDepleted}", this);
        }

        private bool CanTest()
        {
            if (Application.isPlaying && isActiveAndEnabled && health != null)
                return true;

            Debug.LogWarning("Test zdrowia wymaga Play Mode i aktywnego komponentu.", this);
            return false;
        }

        private void OnHealthChanged(int current, int maximum)
        {
            Debug.Log($"{name}: zmiana zdrowia -> {current}/{maximum}", this);
        }

        private void OnDepleted()
        {
            Debug.Log($"{name}: ZDROWIE WYCZERPANE (zdarzenie Depleted)", this);
        }
    }
}
