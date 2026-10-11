using System;
using System.Collections.Generic;
using System.Text;
using Framework.Inventory;
using Framework.Menu;
using ThePirate.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace ThePirate.Ports
{
    public sealed class PortInventoryPanel : MonoBehaviour
    {
        [SerializeField] private Text weight, details, empty;
        [SerializeField] private Image fill, portrait;
        [SerializeField] private RectTransform content;
        [SerializeField] private Button itemTemplate, discardOne, discardAll, confirm, cancel;
        [SerializeField] private Button[] filters;
        private readonly List<Button> slots = new List<Button>();
        private PortRepairService service;
        private PortMenuView owner;
        private ShipCargo cargo;
        private CargoInventory inventory;
        private string selected, signature;
        private int filter, pending;
        private string pendingId;
        public void Configure(Text mass, Text description, Text placeholder, Image bar, Image icon,
            RectTransform grid, Button template, Button one, Button all, Button yes, Button no, Button[] categories)
        { weight = mass; details = description; empty = placeholder; fill = bar; portrait = icon; content = grid;
          itemTemplate = template; discardOne = one; discardAll = all; confirm = yes; cancel = no; filters = categories; }
        public void Bind(PortRepairService target, PortMenuView view)
        {
            if (service != null) return;
            service = target; owner = view;
            for (int i = 0; i < filters.Length; i++) { int index = i; filters[i].onClick.AddListener(() => SelectFilter(index)); }
            discardOne.onClick.AddListener(() => AskDiscard(1));
            discardAll.onClick.AddListener(() => AskDiscard(inventory != null ? inventory.Quantity(selected) : 0));
            confirm.onClick.AddListener(ConfirmDiscard); cancel.onClick.AddListener(CancelDiscard);
            Refresh();
        }
        private bool Available() => service != null && !PauseService.GameplayInputBlocked && service.Available(out _);
        public void Refresh()
        {
            if (service == null) return;
            var nextCargo = ShipCargo.Ensure(service.Ship);
            if (nextCargo == null || !nextCargo.TryGetInventory(out var nextInventory, out _))
            { weight.text = "Ładownia niedostępna"; empty.text = "Brak danych ładowni."; empty.gameObject.SetActive(true); SetActions(false); return; }
            if (inventory != nextInventory) { inventory = nextInventory; cargo = nextCargo; selected = null; signature = null; CancelDiscard(); }
            weight.text = $"ŁADOWNOŚĆ   {inventory.TotalWeight:0.##} / {inventory.Capacity:0.##} kg";
            fill.fillAmount = Mathf.Clamp01(inventory.TotalWeight / inventory.Capacity);
            var stacks = inventory.Snapshot();
            var key = new StringBuilder().Append(filter);
            foreach (var stack in stacks) key.Append('|').Append(stack.resourceId).Append(':').Append(stack.quantity);
            if (signature != key.ToString()) { signature = key.ToString(); Rebuild(stacks); }
            if (pending > 0 && (!Available() || inventory.Quantity(pendingId) < pending || pendingId != selected)) CancelDiscard();
            UpdateDetails();
        }
        private void SelectFilter(int index) { filter = index; selected = null; CancelDiscard(); signature = null; Refresh(); }
        private bool Matches(ResourceDefinition resource) => filter == 0 || resource != null && (int)resource.Category == filter - 1;
        private void Rebuild(ResourceStack[] stacks)
        {
            foreach (var slot in slots) { slot.gameObject.SetActive(false); Destroy(slot.gameObject); }
            slots.Clear();
            Array.Sort(stacks, (a, b) => string.Compare(cargo.Catalog.Find(a.resourceId)?.DisplayName ?? a.resourceId,
                cargo.Catalog.Find(b.resourceId)?.DisplayName ?? b.resourceId, StringComparison.CurrentCulture));
            bool selectionPresent = false;
            foreach (var stack in stacks)
            {
                var resource = cargo.Catalog.Find(stack.resourceId); if (!Matches(resource)) continue;
                string id = stack.resourceId;
                var slot = Instantiate(itemTemplate, content); slot.name = "Item_" + id; slot.gameObject.SetActive(true);
                slot.transform.Find("Name").GetComponent<Text>().text = resource != null ? resource.DisplayName : id;
                slot.transform.Find("Quantity").GetComponent<Text>().text = "× " + stack.quantity;
                var icon = slot.transform.Find("Icon").GetComponent<Image>(); icon.sprite = resource != null ? resource.Icon : null;
                icon.enabled = icon.sprite != null;
                slot.onClick.AddListener(() => { selected = id; CancelDiscard(); UpdateDetails(); }); slots.Add(slot);
                if (id == selected) selectionPresent = true;
            }
            if (!selectionPresent) selected = slots.Count > 0 ? slots[0].name.Substring(5) : null;
            empty.gameObject.SetActive(slots.Count == 0);
            empty.text = stacks.Length == 0 ? "Ładownia jest pusta.\nZasoby zdobędziesz jako łup." : "Brak przedmiotów w tej kategorii.";
            for (int i = 0; i < filters.Length; i++) filters[i].GetComponent<Image>().color = i == filter ? new Color(.48f,.26f,.10f) : new Color(.10f,.15f,.19f);
        }
        private void UpdateDetails()
        {
            var resource = cargo != null ? cargo.Catalog.Find(selected) : null;
            int amount = inventory != null ? inventory.Quantity(selected) : 0;
            foreach (var slot in slots) slot.GetComponent<Image>().color = slot.name == "Item_" + selected ? new Color(.43f,.29f,.13f) : new Color(.08f,.13f,.17f);
            portrait.sprite = resource != null ? resource.Icon : null; portrait.enabled = portrait.sprite != null;
            details.text = resource == null || amount <= 0 ? "Wybierz przedmiot z ładowni." :
                $"{resource.DisplayName}\n\nIlość: {amount}\nMasa sztuki: {resource.UnitWeight:0.##} kg\nMasa stosu: {resource.UnitWeight * amount:0.##} kg\n\n{(string.IsNullOrWhiteSpace(resource.Description) ? "Brak opisu w katalogu zasobów." : resource.Description)}";
            if (inventory != null && inventory.LegacyWeight > 0) details.text += $"\n\nŁadunek bez rozpisanych przedmiotów: {inventory.LegacyWeight:0.##} kg";
            if (pending > 0) details.text += $"\n\nWyrzucić {pending} szt.? Ta czynność usuwa zasób.";
            SetActions(amount > 0 && Available());
        }
        private void SetActions(bool available)
        { discardOne.interactable = discardAll.interactable = available && pending == 0;
          confirm.gameObject.SetActive(pending > 0); cancel.gameObject.SetActive(pending > 0); confirm.interactable = available; }
        private void AskDiscard(int amount)
        { if (!Available() || inventory == null || amount <= 0 || inventory.Quantity(selected) < amount) return;
          pendingId = selected; pending = amount; UpdateDetails(); }
        private void CancelDiscard() { pending = 0; pendingId = null; if (confirm != null) { confirm.gameObject.SetActive(false); cancel.gameObject.SetActive(false); } }
        private void ConfirmDiscard()
        {
            if (!Available() || inventory == null || pending <= 0 || pendingId != selected) { CancelDiscard(); return; }
            string id = pendingId; int amount = pending; CancelDiscard();
            bool success = inventory.TryRemove(id, amount, out string error);
            owner.ShowFeedback(success ? $"Wyrzucono {amount} szt.: {cargo.Catalog.Find(id)?.DisplayName ?? id}." : error);
            Refresh(); owner.Refresh();
        }
        private void OnDisable() { CancelDiscard(); }
    }
}
