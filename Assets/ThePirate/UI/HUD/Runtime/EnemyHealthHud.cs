using Framework.Health;
using ThePirate.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace ThePirate.UI.HUD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatShip))]
    public sealed class EnemyHealthHud : MonoBehaviour
    {
        [SerializeField] private string displayName = "WROGI STATEK";
        [SerializeField] private Vector3 worldOffset = new Vector3(0, 5, 0);
        [SerializeField] private Vector2 screenOffset = new Vector2(0, 24);
        private Health health;
        private CombatShip ship;
        private GameObject view;
        private RectTransform panel;
        private RectTransform canvasRect;
        private RectTransform fill;
        private Text title;
        private Text value;
        private CanvasGroup visibility;
        private bool subscribed;

        private void Awake() { health = GetComponent<Health>(); ship = GetComponent<CombatShip>(); }
        private void OnEnable()
        {
            if (health == null) return;
            health.Changed += Refresh;
            subscribed = true;
            if (view != null) Refresh(health.CurrentHealth, health.MaxHealth);
        }
        private void Start()
        {
            var prefab = Resources.Load<GameObject>("GameWork/EnemyHealthHud");
            view = prefab != null ? Instantiate(prefab) : EnemyHealthHudLayout.Create();
            view.name = "EnemyHUD_" + gameObject.name;
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(view, gameObject.scene);
            canvasRect = view.GetComponent<RectTransform>();
            visibility = view.GetComponent<CanvasGroup>();
            panel = view.transform.Find("Panel") as RectTransform;
            fill = view.transform.Find("Panel/Track/Fill") as RectTransform;
            title = view.transform.Find("Panel/Name").GetComponent<Text>();
            value = view.transform.Find("Panel/HP").GetComponent<Text>();
            Refresh(health.CurrentHealth, health.MaxHealth);
        }
        private void Refresh(int current, int maximum)
        {
            if (view == null) return;
            fill.anchorMax = new Vector2(maximum > 0 ? Mathf.Clamp01((float)current / maximum) : 0, 1);
            title.text = current <= 0 ? "ZATOPIONY" : displayName;
            value.text = current + " / " + maximum;
        }
        private void LateUpdate()
        {
            if (view == null) return;
            Camera camera = Camera.main;
            if (camera == null || ship == null || !ship.isActiveAndEnabled) { visibility.alpha = 0; return; }
            Vector3 screen = camera.WorldToScreenPoint(transform.position + worldOffset);
            Rect rect = camera.pixelRect;
            bool visible = screen.z > 0 && rect.Contains(new Vector2(screen.x, screen.y));
            visibility.alpha = visible ? 1 : 0;
            if (!visible) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 position);
            panel.anchoredPosition = position + screenOffset;
        }
        private void OnDisable()
        {
            if (subscribed && health != null) health.Changed -= Refresh;
            subscribed = false;
            if (visibility != null) visibility.alpha = 0;
        }
        private void OnDestroy() { if (view != null) Destroy(view); }
    }

    // Shared by the runtime fallback and the editable prefab generator.
    public static class EnemyHealthHudLayout
    {
        public static GameObject Create()
        {
            var root = new GameObject("EnemyHealthHud", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            var group = root.GetComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            group.alpha = 0;
            var panel = Rect("Panel", root.transform, new Vector2(190, 54), Vector2.zero);
            Image background = panel.gameObject.AddComponent<Image>();
            background.color = new Color(0.04f, 0.06f, 0.08f, 0.88f);
            background.raycastTarget = false;
            Label("Name", panel, new Vector2(180, 20), new Vector2(0, 15), 13, new Color(1f, 0.84f, 0.55f));
            var track = Rect("Track", panel, new Vector2(174, 10), new Vector2(0, 0));
            var trackImage = track.gameObject.AddComponent<Image>();
            trackImage.color = new Color(0.2f, 0.07f, 0.06f, 1);
            trackImage.raycastTarget = false;
            var fill = Rect("Fill", track, Vector2.zero, Vector2.zero);
            fill.anchorMin = Vector2.zero; fill.anchorMax = Vector2.one;
            fill.offsetMin = Vector2.zero; fill.offsetMax = Vector2.zero;
            var fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.color = new Color(0.82f, 0.18f, 0.12f, 1);
            fillImage.raycastTarget = false;
            Label("HP", panel, new Vector2(180, 18), new Vector2(0, -16), 12, Color.white);
            return root;
        }
        private static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 position)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            var rect = obj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.sizeDelta = size; rect.anchoredPosition = position;
            return rect;
        }
        private static void Label(string name, Transform parent, Vector2 size, Vector2 position, int fontSize, Color color)
        {
            var rect = Rect(name, parent, size, position);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize; text.color = color; text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            text.text = name == "Name" ? "WROGI STATEK" : "500 / 500";
        }
    }
}
