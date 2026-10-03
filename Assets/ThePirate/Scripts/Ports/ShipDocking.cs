using GameWork.Framework.Ships.Movement;
using ThePirate.Combat;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ThePirate.Ports
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatShip), typeof(ShipMovementController), typeof(Rigidbody))]
    public sealed class ShipDocking : MonoBehaviour
    {
        private CombatShip ship;
        private ShipMovementController movement;
        private Rigidbody body;
        private PortDock dock;
        private bool previousKinematic;
        private float berthHeight;
        private Text label;
        private GameObject ui;
        private string feedback;
        private float feedbackUntil;
        public bool IsDocked => dock != null;
        public string PortId => IsDocked ? dock.Id : null;
        public static ShipDocking Ensure(CombatShip ship)
        {
            if (ship == null || ship.Team != 1) return null;
            return ship.GetComponent<ShipDocking>() ?? ship.gameObject.AddComponent<ShipDocking>();
        }
        private void Awake()
        {
            ship = GetComponent<CombatShip>(); movement = GetComponent<ShipMovementController>(); body = GetComponent<Rigidbody>();
        }
        private void Start() => CreatePrompt();
        private void Update()
        {
            bool paused = global::Framework.Menu.PauseService.GameplayInputBlocked;
            if (ui != null) ui.SetActive(!paused && ship.Alive);
            if (paused || !ship.Alive) return;
            if (IsDocked && ship.DockingBlocked) { Undock(); Show("Zagrożenie — cumy zwolnione."); }
            PortDock nearby = IsDocked ? dock : PortDock.Nearest(gameObject.scene, body.position);
            string reason = null;
            bool ready = nearby != null && CanDock(nearby, out reason);
            if (label != null) label.text = Time.time < feedbackUntil ? feedback : IsDocked ? dock.DisplayName + " — E: odcumuj" :
                nearby == null ? "" : ready ? nearby.DisplayName + " — E: zacumuj" : nearby.DisplayName + " — " + reason;
            if (!Application.isFocused || Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;
            if (IsDocked) { Undock(); Show("Odcumowano."); }
            else if (nearby != null)
            {
                if (CanDock(nearby, out reason) && nearby.TryClaim(this)) { Attach(nearby); Show("Zacumowano. E: odcumuj"); }
                else Show(reason ?? "Punkt cumowania jest zajęty.");
            }
        }
        public bool CanDock(PortDock port, out string reason)
        {
            reason = null;
            if (!ship.Alive || port == null || !port.isActiveAndEnabled || port.gameObject.scene != gameObject.scene) reason = "Port niedostępny.";
            else if (PortDock.Find(gameObject.scene, port.Id) != port) reason = "Port wymaga unikalnego identyfikatora.";
            else if (ship.DockingBlocked) reason = "Cumowanie niemożliwe podczas walki.";
            else if (!port.InRange(body.position)) reason = "Podpłyń bliżej.";
            else if (!port.AvailableFor(this)) reason = "Punkt cumowania jest zajęty.";
            else if (body.linearVelocity.magnitude > port.MaximumDockSpeed || Mathf.Abs(movement.State.CurrentPropulsionSpeed) > port.MaximumDockSpeed) reason = "Zwolnij do " + port.MaximumDockSpeed.ToString("0.0") + " m/s.";
            else if (!ClearApproach(port)) reason = "Podejście lub miejsce przy pomoście jest zablokowane.";
            return reason == null;
        }
        private bool ClearApproach(PortDock port)
        {
            var hull = GetComponent<BoxCollider>();
            if (hull == null) return false;
            Vector3 destination = port.Berth.position; destination.y = body.position.y;
            Vector3 half = Vector3.Scale(hull.size * 0.5f, transform.lossyScale);
            half = new Vector3(Mathf.Abs(half.x), Mathf.Abs(half.y), Mathf.Abs(half.z)) * 0.98f;
            Vector3 center = destination + port.Berth.rotation * Vector3.Scale(hull.center, transform.lossyScale);
            foreach (var hit in Physics.OverlapBox(center, half, port.Berth.rotation, ~0, QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(transform)) return false;
            Vector3 start = transform.TransformPoint(hull.center);
            Vector3 delta = destination - body.position;
            if (delta.sqrMagnitude > 0.001f)
                foreach (var hit in Physics.BoxCastAll(start, half, delta.normalized, body.rotation, delta.magnitude, ~0, QueryTriggerInteraction.Ignore))
                    if (!hit.transform.IsChildOf(transform)) return false;
            return true;
        }
        private void Attach(PortDock port)
        {
            dock = port; previousKinematic = body.isKinematic; berthHeight = body.position.y;
            movement.RestoreMotion(0f, 0f, false, Vector3.zero);
            movement.SetMoored(true);
            if (!body.isKinematic) { body.linearVelocity = Vector3.zero; body.angularVelocity = Vector3.zero; }
            body.isKinematic = true;
            var special = GetComponent<SpecialWeaponController>(); if (special != null) special.Cancel();
            HoldBerth();
        }
        private void FixedUpdate() { if (IsDocked) HoldBerth(); }
        private void HoldBerth()
        {
            Vector3 position = dock.Berth.position; position.y = berthHeight;
            body.position = position; body.rotation = dock.Berth.rotation;
        }
        public bool RestoreDock(PortDock port)
        {
            Undock();
            if (port == null || !port.TryClaim(this)) return false;
            Attach(port); return true;
        }
        public void Undock()
        {
            if (dock == null) { movement?.SetMoored(false); return; }
            PortDock previous = dock; dock = null; previous.Release(this);
            body.isKinematic = previousKinematic;
            movement.SetMoored(false); movement.RestoreMotion(0f, 0f, false, Vector3.zero);
        }
        private void OnDisable() { Undock(); if (ui != null) ui.SetActive(false); }
        private void OnEnable() { if (ui != null) ui.SetActive(true); }
        private void OnDestroy() { if (ui != null) Destroy(ui); }
        private void Show(string text) { feedback = text; feedbackUntil = Time.time + 3f; }
        private void CreatePrompt()
        {
            ui = new GameObject("DockingPrompt", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(ui, gameObject.scene);
            var canvas = ui.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 30;
            var scaler = ui.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080);
            ui.GetComponent<CanvasGroup>().blocksRaycasts = false;
            var text = new GameObject("Message", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); text.transform.SetParent(ui.transform, false);
            label = text.GetComponent<Text>(); label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = 24; label.alignment = TextAnchor.MiddleCenter; label.color = Color.white; label.raycastTarget = false;
            var rect = text.GetComponent<RectTransform>(); rect.anchorMin = new Vector2(0.5f, 0.8f); rect.anchorMax = rect.anchorMin; rect.sizeDelta = new Vector2(1000, 70);
        }
    }
}
