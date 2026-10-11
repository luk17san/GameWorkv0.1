using UnityEngine;
using UnityEngine.UI;

namespace ThePirate.Ports
{
    public static class PortShipyardLayout
    {
        public static bool Ensure(GameObject root)
        {
            var view = root.GetComponent<PortMenuView>();
            if (view == null || view.HasShipyardPanel) return false;
            var card = root.transform.Find("Window/PortPanel");
            if (card == null || card.Find("ShipyardPanel") != null) { Debug.LogError("PortMenu: nie można dodać sekcji stoczni; sprawdź hierarchię/referencje.", root); return false; }
            var tab = PortMenuLayout.Button("ShipyardTab", card, "Stocznia", 655, 127, 205, 44);
            var rect = PortMenuLayout.Rect("ShipyardPanel", card); PortMenuLayout.Place(rect, 40, 190, 820, 310);
            var selector = PortMenuLayout.Rect("ShipSelection", rect); PortMenuLayout.Place(selector, 0, 0, 820, 40);
            selector.gameObject.AddComponent<CanvasRenderer>(); var selectorImage = selector.gameObject.AddComponent<Image>(); selectorImage.color = new Color(.16f, .28f, .36f, 1f);
            var dropdown = selector.gameObject.AddComponent<Dropdown>(); dropdown.targetGraphic = selectorImage;
            dropdown.captionText = PortMenuLayout.Label("Caption", selector, 12, 0, 790, 40, 20); dropdown.captionText.alignment = TextAnchor.MiddleLeft;
            var template = PortMenuLayout.Rect("Template", dropdown.transform); PortMenuLayout.Place(template, 0, 40, 820, 180);
            template.gameObject.AddComponent<CanvasRenderer>(); var image = template.gameObject.AddComponent<Image>(); image.color = new Color(.10f, .18f, .24f, 1f);
            var scroll = template.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
            var viewport = PortMenuLayout.Rect("Viewport", template); viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one; viewport.offsetMin = viewport.offsetMax = Vector2.zero;
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = PortMenuLayout.Rect("Content", viewport); PortMenuLayout.Place(content, 0, 0, 820, 40);
            var item = PortMenuLayout.Rect("Item", content); PortMenuLayout.Place(item, 0, 0, 820, 40);
            item.gameObject.AddComponent<CanvasRenderer>(); var itemImage = item.gameObject.AddComponent<Image>(); itemImage.color = new Color(.15f, .25f, .32f, 1f);
            var toggle = item.gameObject.AddComponent<Toggle>(); toggle.targetGraphic = itemImage;
            var itemText = PortMenuLayout.Label("ItemLabel", item, 12, 0, 780, 40, 20); itemText.alignment = TextAnchor.MiddleLeft;
            dropdown.template = template; dropdown.itemText = itemText; scroll.viewport = viewport; scroll.content = content;
            template.gameObject.SetActive(false);
            var preview = PortMenuLayout.Label("ShipPreview", rect, 0, 50, 820, 122, 20);
            var buy = PortMenuLayout.Button("BuyShip", rect, "Kup i wymień", 0, 175, 820, 42);
            var warning = PortMenuLayout.Label("Confirmation", rect, 0, 224, 820, 43, 17);
            var confirm = PortMenuLayout.Button("ConfirmShip", rect, "Potwierdź wymianę", 0, 270, 395, 40);
            var cancel = PortMenuLayout.Button("CancelShip", rect, "Anuluj", 410, 270, 410, 40);
            var panel = rect.gameObject.AddComponent<PortShipyardPanel>(); panel.Configure(dropdown, preview, warning, buy, confirm, cancel);
            view.ConfigureShipyardPanel(panel, tab); rect.gameObject.SetActive(false);
            return true;
        }
    }
}
