using ThePirate.Economy;
using ThePirate.Ships;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ThePirate.Ports
{
    // Powiązania istniejących usług z edytowalnymi obiektami PortVisualKit.
    [DisallowMultipleComponent]
    public sealed class PortVisualBridge : MonoBehaviour
    {
        [SerializeField] private GameObject servicesCard;
        [SerializeField] private Text panelTitle, goldValue, shipValue, shipName, hullValue, hullCardValue, cargoValue, cargoCardValue, description;
        [SerializeField] private Slider hullProgress, hullCardProgress, cargoProgress, cargoCardProgress;
        [SerializeField] private Button home, cargo, upgrades, shipyard, repairs, depart;
        [SerializeField] private Sprite normal, selected;
        private PortMenuView view;
        private PortRepairService service;
        private UnityAction onHome, onCargo, onUpgrades, onShipyard, onRepairs;

        public void Configure(GameObject card, Transform shell, Sprite normalSprite, Sprite selectedSprite)
        {
            servicesCard = card; normal = normalSprite; selected = selectedSprite;
            panelTitle = Find<Text>(shell, "Header/PanelTitle");
            goldValue = Find<Text>(shell, "Header/GoldStatus/Value");
            shipValue = Find<Text>(shell, "Header/ShipStatus/Value");
            shipName = Find<Text>(shell, "ShipSummary/ShipName");
            hullValue = Find<Text>(shell, "Header/HullStatus/Value");
            hullCardValue = Find<Text>(shell, "ShipSummary/Hull/Value");
            cargoValue = Find<Text>(shell, "Header/CargoStatus/Value");
            cargoCardValue = Find<Text>(shell, "ShipSummary/Cargo/Value");
            description = Find<Text>(shell, "ShipSummary/Description");
            hullProgress = Find<Slider>(shell, "Header/HullStatus/Progress");
            hullCardProgress = Find<Slider>(shell, "ShipSummary/Hull/Progress");
            cargoProgress = Find<Slider>(shell, "Header/CargoStatus/Progress");
            cargoCardProgress = Find<Slider>(shell, "ShipSummary/Cargo/Progress");
            home = Find<Button>(shell, "Navigation/NavPort");
            cargo = Find<Button>(shell, "Navigation/NavEquipment");
            upgrades = Find<Button>(shell, "Navigation/NavWorkshop");
            shipyard = Find<Button>(shell, "Navigation/NavShipyard");
            repairs = Find<Button>(shell, "ShipSummary/RepairButton");
            depart = Find<Button>(shell, "Footer/DepartButton");
        }
        private static T Find<T>(Transform parent, string path) where T : Component
        {
            var child = parent.Find(path);
            if (child == null || child.GetComponent<T>() == null)
                throw new System.InvalidOperationException("PortVisualKit: brak " + path + " / " + typeof(T).Name);
            return child.GetComponent<T>();
        }
        public void Bind(PortMenuView owner, PortRepairService target)
        {
            Unbind(); view = owner; service = target;
            onHome = () => view.SelectVisualSection(0);
            onCargo = () => view.SelectVisualSection(1);
            onUpgrades = () => view.SelectVisualSection(2);
            onShipyard = () => view.SelectVisualSection(3);
            onRepairs = () => view.SelectVisualSection(4);
            home.onClick.AddListener(onHome); cargo.onClick.AddListener(onCargo);
            upgrades.onClick.AddListener(onUpgrades); shipyard.onClick.AddListener(onShipyard);
            repairs.onClick.AddListener(onRepairs); depart.onClick.AddListener(Depart);
            upgrades.interactable = view.HasUpgradePanel; shipyard.interactable = view.HasShipyardPanel;
            view.SelectVisualSection(0);
        }
        public void SetSection(int section)
        {
            servicesCard.SetActive(section != 0);
            panelTitle.text = section == 1 ? "ŁADOWNIA" : section == 2 ? "WARSZTAT" : section == 3 ? "STOCZNIA" : section == 4 ? "NAPRAWY" : "PORT";
            Highlight(home, section == 0 || section == 4); Highlight(cargo, section == 1);
            Highlight(upgrades, section == 2); Highlight(shipyard, section == 3);
        }
        private void Highlight(Button button, bool active) => button.GetComponent<Image>().sprite = active ? selected : normal;
        private void Depart()
        {
            if (service == null || global::Framework.Menu.PauseService.GameplayInputBlocked || !service.Available(out _)) return;
            service.Undock(); view.SetVisible(false);
        }
        public void Refresh()
        {
            if (service == null || service.Ship == null) return;
            var ship = service.Ship;
            goldValue.text = service.Gold.ToString();
            string currentName = ship.name.Replace("(Clone)", "").Trim();
            var identity = ship.GetComponent<ShipIdentity>();
            var catalog = ShipCatalog.Load();
            if (identity != null && catalog != null && catalog.TryFind(identity.TypeId, out var definition, out _)) currentName = definition.DisplayName;
            shipValue.text = shipName.text = currentName;
            float ratio = ship.Hull != null && ship.Hull.MaxHealth > 0 ? (float)ship.Hull.CurrentHealth / ship.Hull.MaxHealth : 0;
            hullProgress.value = hullCardProgress.value = Mathf.Clamp01(ratio);
            hullValue.text = hullCardValue.text = $"{Mathf.RoundToInt(ratio * 100)}%";
            var inventoryOwner = ShipCargo.Ensure(ship);
            if (inventoryOwner != null && inventoryOwner.TryGetInventory(out var inventory, out _))
            {
                cargoValue.text = cargoCardValue.text = $"{inventory.TotalWeight:0.#} / {inventory.Capacity:0.#} kg";
                cargoProgress.value = cargoCardProgress.value = inventory.Capacity > 0 ? Mathf.Clamp01(inventory.TotalWeight / inventory.Capacity) : 0;
            }
            else { cargoValue.text = cargoCardValue.text = "Niedostępna"; cargoProgress.value = cargoCardProgress.value = 0; }
            description.text = service.TryQuote(RepairTarget.All, out var quote, out _)
                ? quote.NeedsRepair ? $"Naprawa całego statku: {quote.Cost} złota. Wybierz NAPRAWY, aby sprawdzić zakres i cenę." : "Kadłub i moduły są sprawne. Statek jest gotowy do wypłynięcia."
                : "Usługi są dostępne podczas bezpiecznego cumowania.";
        }
        private void Unbind()
        {
            if (home != null && onHome != null) home.onClick.RemoveListener(onHome);
            if (cargo != null && onCargo != null) cargo.onClick.RemoveListener(onCargo);
            if (upgrades != null && onUpgrades != null) upgrades.onClick.RemoveListener(onUpgrades);
            if (shipyard != null && onShipyard != null) shipyard.onClick.RemoveListener(onShipyard);
            if (repairs != null && onRepairs != null) repairs.onClick.RemoveListener(onRepairs);
            if (depart != null) depart.onClick.RemoveListener(Depart);
        }
        private void OnDestroy() => Unbind();
    }
}
