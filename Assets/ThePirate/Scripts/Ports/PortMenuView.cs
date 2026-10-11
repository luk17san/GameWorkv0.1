using System.Text;
using ThePirate.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace ThePirate.Ports
{
    public sealed class PortMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject window, repairPanel, cargoPanel;
        [SerializeField] private Text title, gold, repairSummary, cargoSummary, cargoList, feedback;
        [SerializeField] private Button repairTab, cargoTab, repairHull, repairModules, repairAll, undock;
        private PortRepairService service;
        private int hullCost, moduleCost, allCost;
        private bool cargoSelected;
        private PortInventoryPanel inventoryPanel;
        [SerializeField] private PortVisualBridge visual;
        public bool HasVisual => visual != null;
        public void ConfigureVisual(PortVisualBridge bridge, Text portTitle, Text messages)
        { visual = bridge; title = portTitle; feedback = messages; }
        public void SelectVisualSection(int section)
        {
            if (service == null) return;
            if (section != 0 && (!service.Available(out _) || global::Framework.Menu.PauseService.GameplayInputBlocked)) return;
            if (section == 0)
            {
                repairPanel.SetActive(false); cargoPanel.SetActive(false);
                if (upgradesPanel != null) upgradesPanel.gameObject.SetActive(false);
                if (shipyardPanel != null) shipyardPanel.gameObject.SetActive(false);
                cargoSelected = false; visual?.SetSection(0); Refresh();
            }
            else if (section == 1) ShowCargo();
            else if (section == 2 && HasUpgradePanel) ShowUpgrades();
            else if (section == 3 && HasShipyardPanel) ShowShipyard();
            else if (section == 4) { ShowRepairs(); Refresh(); }
        }
        [SerializeField] private PortShipyardPanel shipyardPanel;
        [SerializeField] private UnityEngine.UI.Button shipyardTab;
        public bool HasShipyardPanel => shipyardPanel != null && shipyardTab != null;
        public void ConfigureShipyardPanel(PortShipyardPanel panel, UnityEngine.UI.Button tab) { shipyardPanel = panel; shipyardTab = tab; }
        private void ShowShipyard()
        {
            visual?.SetSection(3);
            cargoSelected = false; repairPanel.SetActive(false); cargoPanel.SetActive(false);
            if (upgradesPanel != null) upgradesPanel.gameObject.SetActive(false);
            shipyardPanel.gameObject.SetActive(true); Refresh();
        }
        [SerializeField] private PortUpgradePanel upgradesPanel;
        [SerializeField] private Button upgradeTab;
        public bool HasUpgradePanel => upgradesPanel != null && upgradeTab != null;
        public void ConfigureUpgradePanel(PortUpgradePanel panel, Button tab) { upgradesPanel = panel; upgradeTab = tab; }
        public void ShowFeedback(string text) => feedback.text = text;
        private void ShowUpgrades()
        {
            visual?.SetSection(2);
            if (shipyardPanel != null) shipyardPanel.gameObject.SetActive(false);
            cargoSelected = false; repairPanel.SetActive(false); cargoPanel.SetActive(false);
            upgradesPanel.gameObject.SetActive(true); Refresh();
        }
        public bool Visible => window != null && window.activeSelf;

        public void Configure(GameObject windowObject, GameObject repairs, GameObject cargo,
            Text titleText, Text goldText, Text repairsText, Text cargoText, Text listText, Text feedbackText,
            Button repairsButton, Button cargoButton, Button hullButton, Button modulesButton, Button allButton, Button undockButton)
        {
            window = windowObject; repairPanel = repairs; cargoPanel = cargo;
            title = titleText; gold = goldText; repairSummary = repairsText; cargoSummary = cargoText; cargoList = listText; feedback = feedbackText;
            repairTab = repairsButton; cargoTab = cargoButton; repairHull = hullButton; repairModules = modulesButton; repairAll = allButton; undock = undockButton;
        }

        public void Bind(PortRepairService target)
        {
            service = target;
            inventoryPanel = PortInventoryLayout.Ensure(gameObject);
            if (inventoryPanel != null) inventoryPanel.Bind(service, this);
            repairTab.onClick.AddListener(ShowRepairs); cargoTab.onClick.AddListener(ShowCargo);
            repairHull.onClick.AddListener(RepairHull); repairModules.onClick.AddListener(RepairModules); repairAll.onClick.AddListener(RepairAll);
            undock.onClick.AddListener(Undock);
            if (HasShipyardPanel) { shipyardPanel.Bind(service, this); shipyardTab.onClick.AddListener(ShowShipyard); }
            if (HasUpgradePanel) { upgradesPanel.Bind(service, this); upgradeTab.onClick.AddListener(ShowUpgrades); }
            ShowRepairs();
            if (visual != null) visual.Bind(this, service);
        }
        public void SetVisible(bool value)
        {
            if (window != null && window.activeSelf != value)
            {
                window.SetActive(value);
                if (value) feedback.text = "Wybierz usługę. E: odcumuj • Esc: pauza.";
            }
        }
        private void ShowRepairs() { visual?.SetSection(4); if (shipyardPanel != null) shipyardPanel.gameObject.SetActive(false); if (upgradesPanel != null) upgradesPanel.gameObject.SetActive(false); cargoSelected = false; repairPanel.SetActive(true); cargoPanel.SetActive(false); }
        private void ShowCargo() { visual?.SetSection(1); if (shipyardPanel != null) shipyardPanel.gameObject.SetActive(false); if (upgradesPanel != null) upgradesPanel.gameObject.SetActive(false); cargoSelected = true; repairPanel.SetActive(false); cargoPanel.SetActive(true); Refresh(); }
        private void RepairHull() => Repair(RepairTarget.Hull, hullCost);
        private void RepairModules() => Repair(RepairTarget.Modules, moduleCost);
        private void RepairAll() => Repair(RepairTarget.All, allCost);
        private void Repair(RepairTarget target, int displayedCost)
        {
            service.TryRepair(target, displayedCost, out string message);
            feedback.text = message; Refresh();
        }
        private void Undock() { service.Undock(); SetVisible(false); }

        public void Refresh()
        {
            if (service == null || !service.Available(out _)) { SetVisible(false); return; }
            title.text = service.Port.DisplayName;
            gold.text = $"Złoto: {service.Gold} • Usługi dostępne po zacumowaniu";
            if (service.TryQuote(RepairTarget.All, out var quote, out string error))
                repairSummary.text = $"Kadłub: {service.Ship.Hull.CurrentHealth}/{service.Ship.Hull.MaxHealth} HP\nBrakujące HP modułów: {quote.ModuleMissing}. Cena: kadłub {service.Port.HullRepairGoldPerHealth}, moduły {service.Port.ModuleRepairGoldPerHealth} zł/HP.";
            else repairSummary.text = error;
            UpdateButton(repairHull, RepairTarget.Hull, "Napraw kadłub", out hullCost);
            UpdateButton(repairModules, RepairTarget.Modules, "Napraw moduły", out moduleCost);
            UpdateButton(repairAll, RepairTarget.All, "Napraw cały statek", out allCost);
            if (cargoSelected) RefreshCargo();
            if (upgradesPanel != null && upgradesPanel.gameObject.activeSelf) upgradesPanel.Refresh();
            if (shipyardPanel != null && shipyardPanel.gameObject.activeSelf) shipyardPanel.Refresh();
            if (visual != null) visual.Refresh();
        }
        private void UpdateButton(Button button, RepairTarget target, string label, out int cost)
        {
            bool valid = service.TryQuote(target, out var quote, out _);
            cost = valid ? quote.Cost : 0;
            button.interactable = valid && quote.NeedsRepair && service.Gold >= cost;
            button.GetComponentInChildren<Text>().text = !valid ? label + " — niedostępna" : !quote.NeedsRepair ? label + " — sprawny" :
                $"{label} — {cost} złota" + (service.Gold < cost ? " (brak złota)" : "");
        }
        private void RefreshCargo()
        {
            if (inventoryPanel != null) { inventoryPanel.Refresh(); return; }
            var cargo = ShipCargo.Ensure(service.Ship);
            if (cargo == null || !cargo.TryGetInventory(out var inventory, out string error))
            { cargoSummary.text = "Ładownia niedostępna"; cargoList.text = cargo != null ? cargo.LastError : "Brak ładowni."; return; }
            cargoSummary.text = $"Ładownia: {inventory.TotalWeight:0.###} / {inventory.Capacity:0.###} kg";
            var text = new StringBuilder();
            foreach (var stack in inventory.Snapshot())
            {
                var resource = cargo.Catalog.Find(stack.resourceId);
                string name = resource != null ? resource.DisplayName : stack.resourceId;
                text.AppendLine($"{name} — {stack.quantity} szt.");
            }
            if (inventory.LegacyWeight > 0f) text.AppendLine($"Stary ładunek bez zawartości — {inventory.LegacyWeight:0.###} kg");
            if (text.Length == 0) text.Append("Ładownia jest pusta.");
            cargoList.text = text.ToString();
            var rect = cargoList.rectTransform;
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, Mathf.Max(260f, cargoList.preferredHeight + 20f));
        }
        private void OnDestroy()
        {
            if (shipyardTab != null) shipyardTab.onClick.RemoveListener(ShowShipyard);
            if (upgradeTab != null) upgradeTab.onClick.RemoveListener(ShowUpgrades);
            if (repairTab != null) repairTab.onClick.RemoveListener(ShowRepairs);
            if (cargoTab != null) cargoTab.onClick.RemoveListener(ShowCargo);
            if (repairHull != null) repairHull.onClick.RemoveListener(RepairHull);
            if (repairModules != null) repairModules.onClick.RemoveListener(RepairModules);
            if (repairAll != null) repairAll.onClick.RemoveListener(RepairAll);
            if (undock != null) undock.onClick.RemoveListener(Undock);
        }
    }
}
