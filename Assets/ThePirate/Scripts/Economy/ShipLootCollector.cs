using ThePirate.Combat;
using ThePirate.Ports;
using ThePirate.UI.HUD;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ThePirate.Economy
{
    [DisallowMultipleComponent, RequireComponent(typeof(CombatShip))]
    public sealed class ShipLootCollector : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float collectRange = 10f;
        private CombatShip ship;
        private GameObject ui;
        private Text label;
        public static void Ensure(CombatShip target)
        {
            if (target != null && target.Team == 1 && target.GetComponent<ShipLootCollector>() == null) target.gameObject.AddComponent<ShipLootCollector>();
        }
        private void Awake() => ship = GetComponent<CombatShip>();
        private void Start()
        {
            ui = new GameObject("LootPrompt", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(ui, gameObject.scene);
            ui.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay; ui.GetComponent<Canvas>().sortingOrder = 30;
            var scaler = ui.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080);
            ui.GetComponent<CanvasGroup>().blocksRaycasts = false;
            var text = new GameObject("Message", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); text.transform.SetParent(ui.transform, false);
            label = text.GetComponent<Text>(); label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = 24; label.alignment = TextAnchor.MiddleCenter; label.raycastTarget = false;
            var rect = text.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.73f); rect.sizeDelta = new Vector2(1000f, 70f);
        }
        private void Update()
        {
            bool blocked = !ship.Alive || ship.Team != 1 || global::Framework.Menu.PauseService.GameplayInputBlocked
                || ship.IsDocked || PortDock.Nearest(gameObject.scene, transform.position) != null;
            var item = blocked ? null : FloatingLoot.Nearest(gameObject.scene, transform.position, collectRange);
            if (ui != null) ui.SetActive(item != null);
            if (item == null) return;
            label.text = item.Label + " — E: zbierz";
            if (!Application.isFocused || Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;
            var cargo = ShipCargo.Ensure(ship);
            bool collected = item.TryCollect(cargo, out string error);
            string message = collected ? "Zebrano: " + item.Label + "." : "Nie zebrano łupu: " + error;
            var hud = ship.GetComponent<PlayerHudController>();
            if (hud != null) hud.State.Publish(new HudMessage(message, collected ? HudMessageKind.Info : HudMessageKind.Warning, 4f));
            if (!collected) Debug.LogWarning(message, ship);
        }
        private void OnDisable() { if (ui != null) ui.SetActive(false); }
        private void OnDestroy() { if (ui != null) Destroy(ui); }
    }
}
