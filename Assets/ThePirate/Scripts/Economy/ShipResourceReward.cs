using Framework.Health;
using ThePirate.Combat;
using ThePirate.UI.HUD;
using UnityEngine;

namespace ThePirate.Economy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatShip))]
    public sealed class ShipResourceReward : MonoBehaviour
    {
        [SerializeField] private bool rewardEnabled = true;
        [SerializeField, Tooltip("Puste: używa GameWork/ShipLootTable. Możesz przypisać inną tabelę temu przeciwnikowi.")]
        private ShipLootTable lootTable;
        [SerializeField] private float lootSurfaceHeight = 0.25f;
        private Health hull;
        private CombatShip ship;
        private bool handled;
        public static ShipResourceReward Ensure(CombatShip target)
        {
            if (target == null || target.Team == 0 || target.Team == 1) return null;
            return target.GetComponent<ShipResourceReward>() ?? target.gameObject.AddComponent<ShipResourceReward>();
        }
        private void Awake() { ship = GetComponent<CombatShip>(); hull = GetComponent<Health>(); }
        private void OnEnable() { if (hull != null) hull.Damaged += DamageReceived; }
        private void OnDisable() { if (hull != null) hull.Damaged -= DamageReceived; }
        private void DamageReceived(int amount)
        {
            // RestoreCurrentHealth nie emituje Damaged, więc wczytanie nie przyznaje nagrody.
            if (handled || amount <= 0 || hull == null || !hull.IsDepleted || ship.Team == 0 || ship.Team == 1) return;
            handled = true; // Także odmowa jest ostateczna; brak ponownych prób po naprawie/reenable.
            if (!rewardEnabled) return;
            CombatShip player = null;
            foreach (var candidate in CombatShip.ActiveShips)
            {
                if (candidate == null || candidate.Team != 1 || !candidate.Alive || candidate.gameObject.scene != gameObject.scene) continue;
                if (player != null) return;
                player = candidate;
            }
            if (player == null) return;
            var table = lootTable != null ? lootTable : Resources.Load<ShipLootTable>("GameWork/ShipLootTable");
            if (table == null) { Notify(player, "Nie przyznano materiałów: brak tabeli łupu.", true); return; }
            if (!table.TrySnapshot(out var rewards, out string error)) { Notify(player, "Nie przyznano materiałów: " + error, true); return; }
            if (rewards.Length == 0) return;
            var catalog = Resources.Load<Framework.Inventory.ResourceCatalog>(ShipCargo.CatalogPath);
            foreach (var reward in rewards)
                if (catalog == null || catalog.Find(reward.resourceId) == null)
                { Notify(player, "Nie utworzono łupu: nieznany zasób w tabeli.", true); return; }
            var origin = transform.position;
            // Water uses Y=0 in ShipSandbox; keep loot visible above its surface.
            origin.y = lootSurfaceHeight;
            for (int i = 0; i < rewards.Length; i++)
            {
                float angle = i * Mathf.PI * 2f / rewards.Length;
                var position = origin + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 3f;
                FloatingLoot.Spawn(gameObject.scene, new FloatingLootData
                {
                    position = position, yaw = i * 47f, appearance = rewards[i].resourceId == "wood" ? 1 : 0,
                    resources = new[] { rewards[i] }
                });
            }
            Notify(player, "Na wodzie pozostał łup — podpłyń i naciśnij E.", false);
        }
        private static void Notify(CombatShip player, string message, bool warning)
        {
            var hud = player.GetComponent<PlayerHudController>();
            if (hud != null) hud.State.Publish(new HudMessage(message, warning ? HudMessageKind.Warning : HudMessageKind.Info, 6f));
            if (warning) Debug.LogWarning(message, player); else Debug.Log(message, player);
        }
    }
}
