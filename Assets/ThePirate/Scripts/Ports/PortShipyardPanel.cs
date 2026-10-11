using System.Collections.Generic;
using ThePirate.Ships;
using UnityEngine;
using UnityEngine.UI;

namespace ThePirate.Ports
{
    public sealed class PortShipyardPanel : MonoBehaviour
    {
        [SerializeField] private Dropdown selection;
        [SerializeField] private Text preview, confirmation;
        [SerializeField] private Button buy, confirm, cancel;
        [SerializeField] private Button build;
        [SerializeField] private Text buildSummary;
        public bool HasBuildButton => build != null && buildSummary != null;
        public void ConfigureBuildButton(Button button, Text summary) { build = button; buildSummary = summary; }
        private PortRepairService port;
        private ShipyardService service;
        private PortMenuView menu;
        private ShipCatalog catalog;
        private string pendingId;
        private int displayedPrice, pendingPrice;
        private ShipBuildQuote displayedBuild;
        private string pendingSignature;
        private bool pendingBuild;

        public void Configure(Dropdown dropdown, Text summary, Text warning, Button buyButton, Button confirmButton, Button cancelButton)
        { selection = dropdown; preview = summary; confirmation = warning; buy = buyButton; confirm = confirmButton; cancel = cancelButton; }
        public void Bind(PortRepairService owner, PortMenuView view)
        {
            port = owner; menu = view; service = ShipyardService.Ensure(owner.Ship); catalog = ShipCatalog.Load();
            selection.ClearOptions(); var names = new List<string>();
            if (catalog != null) foreach (var ship in catalog.Ships) names.Add(ship != null ? ship.DisplayName : "Brak projektu");
            selection.AddOptions(names); selection.onValueChanged.AddListener(SelectionChanged);
            buy.onClick.AddListener(RequestBuy); confirm.onClick.AddListener(ConfirmBuy); cancel.onClick.AddListener(CancelBuy);
            if (build != null) build.onClick.AddListener(RequestBuild);
            CancelBuy();
        }
        private void SelectionChanged(int value) { CancelBuy(); Refresh(); }
        private ShipDefinition Selected => catalog != null && selection.value >= 0 && selection.value < catalog.Ships.Count ? catalog.Ships[selection.value] : null;
        public void Refresh()
        {
            var selected = Selected;
            if (selected == null) { preview.text = "Brak projektów. Utwórz katalog statków."; buy.interactable = false; if (build != null) build.interactable = false; CancelBuy(); return; }
            displayedPrice = selected.Price;
            preview.text = $"{selected.DisplayName}\nKadłub: {selected.HullHealth} HP • Prędkość: {selected.Speed:0.##} m/s\nŁadownia: {selected.Capacity:0.##} kg • Zakup: {selected.Price} złota";
            bool valid = service.TryQuote(selected.Id, out _, out string error);
            buy.interactable = valid && port.Gold >= displayedPrice;
            buy.GetComponentInChildren<Text>().text = valid ? $"Kup i wymień — {displayedPrice} złota" + (port.Gold < displayedPrice ? " (brak złota)" : "") : error;
            bool buildValid = service.TryQuoteBuild(selected.Id, out displayedBuild, out string buildError);
            if (HasBuildButton)
            {
                // Koszt pokazujemy także wtedy, gdy brakuje materiałów albo to aktualny typ.
                if (displayedBuild == null) ShipBuildCatalog.Load()?.TryFind(selected.Id, out displayedBuild, out _);
                build.interactable = buildValid && displayedBuild != null && port.Gold >= displayedBuild.GoldCost;
                build.GetComponentInChildren<Text>().text = buildValid ? $"Zbuduj — {displayedBuild.GoldCost} złota" + (port.Gold < displayedBuild.GoldCost ? " (brak złota)" : "") : buildError;
                buildSummary.text = FormatBuild(displayedBuild);
                buildSummary.rectTransform.sizeDelta = new Vector2(buildSummary.rectTransform.sizeDelta.x, Mathf.Max(40f, buildSummary.preferredHeight));
            }
            if (pendingId != null && (pendingId != selected.Id || (pendingBuild
                ? !buildValid || displayedBuild == null || pendingSignature != displayedBuild.Signature || port.Gold < displayedBuild.GoldCost
                : pendingPrice != displayedPrice || !valid || port.Gold < displayedPrice))) CancelBuy();
        }
        private string FormatBuild(ShipBuildQuote quote)
        {
            if (quote == null) return "Brak receptury budowy.";
            var text = new System.Text.StringBuilder("Budowa: ").Append(quote.GoldCost).Append(" złota; materiały (posiadasz / potrzeba):");
            var resources = Resources.Load<Framework.Inventory.ResourceCatalog>("GameWork/ResourceCatalog");
            ThePirate.Economy.ShipCargo.Ensure(port.Ship).TryGetInventory(out var inventory, out _);
            foreach (var cost in quote.Materials())
                text.Append('\n').Append(resources?.Find(cost.resourceId)?.DisplayName ?? cost.resourceId)
                    .Append(": ").Append(inventory != null ? inventory.Quantity(cost.resourceId) : 0).Append(" / ").Append(cost.quantity);
            return text.ToString();
        }
        private void RequestBuy()
        {
            if (Selected == null) return;
            pendingBuild = false;
            pendingId = Selected.Id; pendingPrice = displayedPrice;
            confirmation.text = "Zastąpisz stary statek i jego ulepszenia bez zwrotu ceny. Złoto i ładownia przejdą na nowy.";
            confirm.gameObject.SetActive(true); cancel.gameObject.SetActive(true);
        }
        private void RequestBuild()
        {
            if (Selected == null || displayedBuild == null) return;
            pendingBuild = true; pendingId = Selected.Id; pendingSignature = displayedBuild.Signature;
            confirmation.text = "Zużyjesz materiały i złoto z receptury. Nowy statek zastąpi stary wraz z ulepszeniami, bez zwrotu ceny.";
            confirm.gameObject.SetActive(true); cancel.gameObject.SetActive(true);
        }
        private void ConfirmBuy()
        {
            if (pendingId == null) return;
            string message;
            if (pendingBuild) service.TryBuild(pendingId, pendingSignature, out message);
            else service.TryBuy(pendingId, pendingPrice, out message);
            CancelBuy(); menu.ShowFeedback(message); menu.Refresh();
        }
        private void CancelBuy() { pendingId = null; pendingSignature = null; pendingBuild = false; confirmation.text = ""; confirm.gameObject.SetActive(false); cancel.gameObject.SetActive(false); }
        private void OnDisable() { if (confirm != null && cancel != null && confirmation != null) CancelBuy(); }
        private void OnDestroy()
        {
            if (selection != null) selection.onValueChanged.RemoveListener(SelectionChanged);
            if (buy != null) buy.onClick.RemoveListener(RequestBuy);
            if (build != null) build.onClick.RemoveListener(RequestBuild);
            if (confirm != null) confirm.onClick.RemoveListener(ConfirmBuy);
            if (cancel != null) cancel.onClick.RemoveListener(CancelBuy);
        }
    }
}
