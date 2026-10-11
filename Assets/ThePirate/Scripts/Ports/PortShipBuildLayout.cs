using UnityEngine;
using UnityEngine.UI;

namespace ThePirate.Ports
{
    public static class PortShipBuildLayout
    {
        public static bool Ensure(GameObject root)
        {
            var rect = root.transform.Find("Window/PortPanel/ShipyardPanel");
            var panel = rect != null ? rect.GetComponent<PortShipyardPanel>() : null;
            if (panel == null || panel.HasBuildButton) return false;
            var buy = rect.Find("BuyShip")?.GetComponent<RectTransform>();
            if (buy == null || rect.Find("BuildShip") != null) { Debug.LogError("PortMenu: sprawdź referencje sekcji budowy.", root); return false; }
            PortMenuLayout.Place(buy, 0, 175, 395, 42);
            var build = PortMenuLayout.Button("BuildShip", rect, "Zbuduj", 410, 175, 410, 42);
            var preview = rect.Find("ShipPreview")?.GetComponent<RectTransform>();
            if (preview != null) PortMenuLayout.Place(preview, 0, 50, 820, 76);
            var scroll = PortMenuLayout.Rect("BuildMaterials", rect); PortMenuLayout.Place(scroll, 0, 126, 820, 42);
            var scroller = scroll.gameObject.AddComponent<ScrollRect>(); scroller.horizontal = false; scroller.movementType = ScrollRect.MovementType.Clamped;
            var viewport = PortMenuLayout.Rect("Viewport", scroll); viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one; viewport.offsetMin = viewport.offsetMax = Vector2.zero;
            viewport.gameObject.AddComponent<CanvasRenderer>(); var image = viewport.gameObject.AddComponent<Image>(); image.color = new Color(.10f, .18f, .24f, 1f);
            viewport.gameObject.AddComponent<RectMask2D>();
            var summary = PortMenuLayout.Label("Recipe", viewport, 12, 0, 796, 42, 16);
            scroller.viewport = viewport; scroller.content = summary.rectTransform;
            panel.ConfigureBuildButton(build, summary); return true;
        }
    }
}
