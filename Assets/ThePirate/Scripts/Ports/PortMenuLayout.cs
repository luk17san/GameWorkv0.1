using UnityEngine;
using UnityEngine.UI;

namespace ThePirate.Ports
{
    // Ten sam edytowalny układ jest używany przez generator prefabu i awaryjny widok runtime.
    public static class PortMenuLayout
    {
        public static GameObject Create()
        {
            var root = new GameObject("PortMenu", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 60;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            var window = Rect("Window", root.transform); Stretch(window);
            var shade = Rect("Shade", window); Stretch(shade); Image(shade, new Color(0, 0, 0, 0.65f));
            var card = Rect("PortPanel", window); card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(900, 680); Image(card, new Color(0.06f, 0.10f, 0.15f, 1f));
            var title = Label("PortName", card, 40, 28, 820, 45, 30);
            title.text = "Port";
            var gold = Label("Gold", card, 40, 80, 820, 34, 21);
            gold.text = "Złoto: 0";
            var repairTab = Button("RepairTab", card, "Naprawa", 40, 127, 190, 44);
            var cargoTab = Button("CargoTab", card, "Ładownia", 245, 127, 190, 44);
            var repairs = Rect("RepairPanel", card); Place(repairs, 40, 190, 820, 305);
            var repairSummary = Label("RepairSummary", repairs, 0, 0, 820, 76, 21);
            repairSummary.text = "Stan kadłuba i modułów";
            var hull = Button("RepairHull", repairs, "Napraw kadłub", 0, 92, 820, 48);
            var modules = Button("RepairModules", repairs, "Napraw moduły", 0, 155, 820, 48);
            var all = Button("RepairAll", repairs, "Napraw cały statek", 0, 218, 820, 48);
            var cargo = Rect("CargoPanel", card); Place(cargo, 40, 190, 820, 310);
            var cargoSummary = Label("CargoSummary", cargo, 0, 0, 820, 36, 22);
            var scroll = Rect("CargoScroll", cargo); Place(scroll, 0, 46, 820, 260);
            var scrollRect = scroll.gameObject.AddComponent<ScrollRect>(); scrollRect.horizontal = false; scrollRect.movementType = ScrollRect.MovementType.Clamped;
            var viewport = Rect("Viewport", scroll); Stretch(viewport); Image(viewport, new Color(0.08f, 0.13f, 0.18f, 1f));
            viewport.gameObject.AddComponent<RectMask2D>();
            var list = Label("CargoList", viewport, 12, 10, 796, 260, 22);
            list.text = "Ładownia jest pusta."; scrollRect.viewport = viewport; scrollRect.content = list.rectTransform;
            var feedback = Label("Feedback", card, 40, 516, 820, 68, 20);
            feedback.text = "Wybierz usługę. E: odcumuj • Esc: pauza.";
            var undock = Button("Undock", card, "Odcumuj i wróć na morze", 40, 608, 820, 46);
            var view = root.AddComponent<PortMenuView>();
            view.Configure(window.gameObject, repairs.gameObject, cargo.gameObject, title, gold, repairSummary, cargoSummary, list, feedback,
                repairTab, cargoTab, hull, modules, all, undock);
            cargo.gameObject.SetActive(false); window.gameObject.SetActive(false);
            PortUpgradeLayout.Ensure(root);
            PortShipyardLayout.Ensure(root);
            PortShipBuildLayout.Ensure(root);
            return root;
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
            return obj.GetComponent<RectTransform>();
        }
        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        public static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(width, height);
        }
        private static Image Image(RectTransform rect, Color color)
        {
            rect.gameObject.AddComponent<CanvasRenderer>(); var image = rect.gameObject.AddComponent<Image>(); image.color = color; return image;
        }
        public static Text Label(string name, Transform parent, float x, float y, float width, float height, int fontSize)
        {
            var rect = Rect(name, parent); Place(rect, x, y, width, height);
            rect.gameObject.AddComponent<CanvasRenderer>(); var text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = fontSize; text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }
        public static Button Button(string name, Transform parent, string caption, float x, float y, float width, float height)
        {
            var rect = Rect(name, parent); Place(rect, x, y, width, height);
            var image = Image(rect, new Color(0.16f, 0.28f, 0.36f, 1f));
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.disabledColor = new Color(0.35f, 0.35f, 0.35f, 0.6f); button.colors = colors;
            var label = Label("Label", rect, 0, 0, width, height, 22); label.alignment = TextAnchor.MiddleCenter; label.text = caption;
            return button;
        }
    }
}
