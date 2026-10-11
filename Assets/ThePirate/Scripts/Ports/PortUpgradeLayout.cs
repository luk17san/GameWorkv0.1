using UnityEngine;
using UnityEngine.UI;

namespace ThePirate.Ports
{
    public static class PortUpgradeLayout
    {
        // Rozszerzamy istniejącą hierarchię, zachowując wszystkie dotychczasowe obiekty.
        public static bool Ensure(GameObject root)
        {
            var view = root.GetComponent<PortMenuView>();
            if (view == null || view.HasUpgradePanel) return false;
            var card = root.transform.Find("Window/PortPanel");
            if (card == null) { Debug.LogError("PortMenu: brak Window/PortPanel do dodania ulepszeń.", root); return false; }
            if (card.Find("UpgradePanel") != null || card.Find("UpgradeTab") != null)
            { Debug.LogError("PortMenu: sekcja ulepszeń istnieje, ale brakuje referencji na PortMenuView.", root); return false; }
            var tab = PortMenuLayout.Button("UpgradeTab", card, "Ulepszenia", 450, 127, 190, 44);
            var rect = PortMenuLayout.Rect("UpgradePanel", card); PortMenuLayout.Place(rect, 40, 190, 820, 310);
            var hull = PortMenuLayout.Label("HullPreview", rect, 0, 0, 820, 42, 19);
            var hullBuy = PortMenuLayout.Button("BuyHull", rect, "Kup kadłub", 0, 44, 820, 42);
            var drive = PortMenuLayout.Label("DrivePreview", rect, 0, 100, 820, 42, 19);
            var driveBuy = PortMenuLayout.Button("BuyDrive", rect, "Kup napęd", 0, 144, 820, 42);
            var batteries = PortMenuLayout.Label("BatteryPreview", rect, 0, 200, 820, 42, 19);
            var batteryBuy = PortMenuLayout.Button("BuyBatteries", rect, "Kup baterie", 0, 244, 820, 42);
            var panel = rect.gameObject.AddComponent<PortUpgradePanel>(); panel.Configure(hull, drive, batteries, hullBuy, driveBuy, batteryBuy);
            view.ConfigureUpgradePanel(panel, tab); rect.gameObject.SetActive(false);
            return true;
        }
    }
}
