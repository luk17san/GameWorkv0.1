using ThePirate.Combat;
using UnityEngine;

namespace ThePirate.UI.HUD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ShipHudAdapter))]
    public sealed class PlayerHudController : MonoBehaviour
    {
        [SerializeField, Min(0.02f)] private float refreshInterval = 0.1f;
        private ShipHudAdapter adapter;
        private float nextRefresh;
        public PlayerHudStateStore State { get; } = new PlayerHudStateStore();

        private void OnEnable()
        {
            adapter = GetComponent<ShipHudAdapter>();
            adapter.ResolveSources();
            nextRefresh = 0;
        }

        // LateUpdate: wszystkie Awake i odtwarzanie save są już zakończone.
        // Czas nieskalowany pozwala odświeżyć stan podczas pauzy; reload używa czasu gry.
        private void LateUpdate()
        {
            if (Time.unscaledTime < nextRefresh) return;
            RefreshNow();
        }

        public void RefreshNow()
        {
            if (adapter == null) return;
            adapter.ReadInto(State);
            nextRefresh = Time.unscaledTime + Mathf.Max(0.02f, refreshInterval);
        }

        public void SetTarget(CombatShip target)
        {
            if (adapter == null) return;
            adapter.SetTarget(target);
            RefreshNow();
        }

        private void OnDisable()
        {
            State.SetDurability(default); State.SetMovement(default); State.SetCargo(default);
            State.SetAim(default); State.SetTarget(default); State.SetBatteries(null); State.SetMarkers(null);
            State.SetWind(default); State.SetObjective(default);
        }
    }
}
