using UnityEngine;
using UnityEngine.UI;

namespace ThePirate.Ports
{
    public static class PortInventoryLayout
    {
        public static PortInventoryPanel Ensure(GameObject root)
        {
            var parent = root.transform.Find("Window/PortPanel/CargoPanel");
            if (parent == null) return null;
            var existing = parent.GetComponentInChildren<PortInventoryPanel>(true);
            if (existing != null) return existing;
            foreach (Transform child in parent) child.gameObject.SetActive(false);
            var panel = Rect("Inventory", parent, 0, 0, 862, 530);
            var weight = Label("Weight", panel, "ŁADOWNOŚĆ", 0, 0, 850, 32, 22);
            var track = Rect("WeightBar", panel, 0, 37, 862, 10); Background(track, new Color(.03f,.06f,.08f));
            var fillRect = Rect("Fill", track, 0, 0, 862, 10); var fill = Background(fillRect, new Color(.82f,.59f,.20f));
            fill.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal;
            var filters = new Button[4]; string[] names = { "Wszystko", "Materiały", "Towary", "Amunicja" };
            for (int i = 0; i < 4; i++) filters[i] = Button("Filter" + i, panel, names[i], i * 139, 60, 131, 40);
            var scroll = Rect("ItemsScroll", panel, 0, 112, 552, 410);
            var viewport = Rect("Viewport", scroll, 0, 0, 552, 410); Background(viewport, new Color(.04f,.07f,.10f)); viewport.gameObject.AddComponent<RectMask2D>();
            var content = Rect("Content", viewport, 0, 0, 544, 410);
            var grid = content.gameObject.AddComponent<GridLayoutGroup>(); grid.cellSize = new Vector2(170, 136); grid.spacing = new Vector2(10, 10);
            grid.padding = new RectOffset(6, 6, 6, 6); grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 3;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>(); fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroller = scroll.gameObject.AddComponent<ScrollRect>(); scroller.viewport = viewport; scroller.content = content;
            scroller.horizontal = false; scroller.movementType = ScrollRect.MovementType.Clamped;
            var template = Button("ItemTemplate", panel, "", 0, 0, 170, 136);
            template.transform.Find("Label").gameObject.SetActive(false);
            Label("Name", template.transform, "Przedmiot", 8, 83, 154, 30, 17);
            Label("Quantity", template.transform, "× 0", 8, 113, 154, 21, 17);
            var itemIcon = Background(Rect("Icon", template.transform, 53, 8, 64, 64), Color.white); itemIcon.preserveAspect = true;
            template.gameObject.SetActive(false);
            var empty = Label("Empty", viewport, "Ładownia jest pusta.", 20, 140, 512, 90, 22); empty.alignment = TextAnchor.MiddleCenter;
            var parchment = Rect("DetailsCard", panel, 568, 60, 294, 462); Background(parchment, new Color(.79f,.69f,.51f));
            var icon = Background(Rect("Portrait", parchment, 106, 12, 82, 82), Color.white); icon.preserveAspect = true;
            var details = Label("Description", parchment, "Wybierz przedmiot.", 14, 101, 266, 261, 17); details.color = new Color(.12f,.08f,.04f);
            var one = Button("DiscardOne", parchment, "Wyrzuć 1", 14, 368, 127, 38);
            var all = Button("DiscardAll", parchment, "Wyrzuć stos", 151, 368, 129, 38);
            var yes = Button("Confirm", parchment, "Potwierdź", 14, 412, 127, 38);
            var no = Button("Cancel", parchment, "Anuluj", 151, 412, 129, 38);
            yes.gameObject.SetActive(false); no.gameObject.SetActive(false);
            var result = panel.gameObject.AddComponent<PortInventoryPanel>();
            result.Configure(weight, details, empty, fill, icon, content, template, one, all, yes, no, filters);
            return result;
        }
        private static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
        {
            var rect = PortMenuLayout.Rect(name, parent); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(w, h); return rect;
        }
        private static Image Background(RectTransform rect, Color color)
        { rect.gameObject.AddComponent<CanvasRenderer>(); var image = rect.gameObject.AddComponent<Image>(); image.color = color; return image; }
        private static Text Label(string name, Transform parent, string value, float x, float y, float w, float h, int size)
        {
            var rect = Rect(name, parent, x, y, w, h); rect.gameObject.AddComponent<CanvasRenderer>();
            var text = rect.gameObject.AddComponent<Text>(); text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.color = new Color(.94f,.86f,.69f);
            text.raycastTarget = false; text.verticalOverflow = VerticalWrapMode.Truncate; text.horizontalOverflow = HorizontalWrapMode.Wrap;
            return text;
        }
        private static Button Button(string name, Transform parent, string value, float x, float y, float w, float h)
        {
            var rect = Rect(name, parent, x, y, w, h); var image = Background(rect, new Color(.10f,.15f,.19f));
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var label = Label("Label", rect, value, 4, 0, w - 8, h, 17); label.alignment = TextAnchor.MiddleCenter; return button;
        }
    }
}
