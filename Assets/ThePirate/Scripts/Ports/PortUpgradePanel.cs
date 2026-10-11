using ThePirate.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace ThePirate.Ports
{
    public sealed class PortUpgradePanel : MonoBehaviour
    {
        [SerializeField] private Text hullPreview, drivePreview, batteryPreview;
        [SerializeField] private Button hullButton, driveButton, batteryButton;
        private ShipUpgrades upgrades;
        private PortRepairService service;
        private PortMenuView menu;
        private int hullPrice, drivePrice, batteryPrice;

        public void Configure(Text hull, Text drive, Text batteries, Button hullBuy, Button driveBuy, Button batteryBuy)
        { hullPreview = hull; drivePreview = drive; batteryPreview = batteries; hullButton = hullBuy; driveButton = driveBuy; batteryButton = batteryBuy; }
        public void Bind(PortRepairService port, PortMenuView view)
        {
            service = port; menu = view; upgrades = ShipUpgrades.Ensure(port.Ship);
            hullButton.onClick.AddListener(BuyHull); driveButton.onClick.AddListener(BuyDrive); batteryButton.onClick.AddListener(BuyBatteries);
        }
        public void Refresh()
        {
            UpdateQuote(ShipUpgradeKind.Hull, hullPreview, hullButton, out hullPrice);
            UpdateQuote(ShipUpgradeKind.Drive, drivePreview, driveButton, out drivePrice);
            UpdateQuote(ShipUpgradeKind.Batteries, batteryPreview, batteryButton, out batteryPrice);
        }
        private void UpdateQuote(ShipUpgradeKind kind, Text preview, Button button, out int price)
        {
            price = 0;
            if (upgrades == null) { preview.text = "Brak systemu ulepszeń."; button.interactable = false; return; }
            if (!upgrades.TryQuote(kind, out price, out string text, out bool purchased, out string error))
            { preview.text = error; button.interactable = false; button.GetComponentInChildren<Text>().text = "Ulepszenie niedostępne"; return; }
            preview.text = text;
            button.interactable = !purchased && service.Gold >= price;
            button.GetComponentInChildren<Text>().text = purchased ? "Poziom 1 — kupiony" : $"Kup poziom 1 — {price} złota" + (service.Gold < price ? " (brak złota)" : "");
        }
        private void BuyHull() => Buy(ShipUpgradeKind.Hull, hullPrice);
        private void BuyDrive() => Buy(ShipUpgradeKind.Drive, drivePrice);
        private void BuyBatteries() => Buy(ShipUpgradeKind.Batteries, batteryPrice);
        private void Buy(ShipUpgradeKind kind, int price)
        {
            upgrades.TryBuy(kind, price, out string message);
            menu.ShowFeedback(message); menu.Refresh();
        }
        private void OnDestroy()
        {
            if (hullButton != null) hullButton.onClick.RemoveListener(BuyHull);
            if (driveButton != null) driveButton.onClick.RemoveListener(BuyDrive);
            if (batteryButton != null) batteryButton.onClick.RemoveListener(BuyBatteries);
        }
    }
}
