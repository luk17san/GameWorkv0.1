using System;
using System.Collections.Generic;

namespace Framework.Inventory
{
    [Serializable]
    public sealed class ResourceStack
    {
        public string resourceId;
        public int quantity;
    }

    // Własna kopia mas katalogu zapewnia spójność podczas jednej rozgrywki.
    public sealed class CargoInventory
    {
        private readonly Dictionary<string, float> weights;
        private Dictionary<string, int> items = new Dictionary<string, int>(StringComparer.Ordinal);
        public float Capacity { get; }
        public float TotalWeight { get; private set; }
        public float LegacyWeight { get; private set; }
        public event Action Changed;

        public CargoInventory(float capacity, Dictionary<string, float> resourceWeights)
        {
            if (!Finite(capacity) || capacity <= 0f) throw new ArgumentException("Nieprawidłowa pojemność.");
            if (resourceWeights == null || resourceWeights.Count > 1024) throw new ArgumentException("Nieprawidłowe masy zasobów.");
            weights = new Dictionary<string, float>(StringComparer.Ordinal);
            foreach (var pair in resourceWeights)
            {
                if (!ResourceDefinition.ValidId(pair.Key) || !Finite(pair.Value) || pair.Value <= 0f) throw new ArgumentException("Nieprawidłowy zasób.");
                weights.Add(pair.Key, pair.Value);
            }
            Capacity = capacity;
        }

        public int Quantity(string id) => id != null && items.TryGetValue(id, out int value) ? value : 0;

        public ResourceStack[] Snapshot()
        {
            var ids = new List<string>(items.Keys); ids.Sort(StringComparer.Ordinal);
            var result = new ResourceStack[ids.Count];
            for (int i = 0; i < ids.Count; i++) result[i] = new ResourceStack { resourceId = ids[i], quantity = items[ids[i]] };
            return result;
        }

        public bool TryAdd(string id, int quantity, out string error)
        {
            error = null;
            if (quantity <= 0 || id == null || !weights.ContainsKey(id)) { error = "Nieprawidłowy zasób lub ilość."; return false; }
            int previous = Quantity(id);
            if (quantity > int.MaxValue - previous) { error = "Przekroczono maksymalną ilość."; return false; }
            var candidate = new Dictionary<string, int>(items, StringComparer.Ordinal) { [id] = previous + quantity };
            return Commit(candidate, LegacyWeight, out error);
        }

        // Paczka nagrody: wszystkie wpisy mieszczą się albo zawartość pozostaje bez zmian.
        public bool TryAddBatch(ResourceStack[] stacks, out string error)
        {
            error = null;
            if (stacks == null || stacks.Length > 1024) { error = "Nieprawidłowa paczka zasobów."; return false; }
            var candidate = new Dictionary<string, int>(items, StringComparer.Ordinal);
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var stack in stacks)
            {
                if (stack == null || stack.resourceId == null || stack.quantity <= 0 || !weights.ContainsKey(stack.resourceId) || !ids.Add(stack.resourceId))
                { error = "Nieznany/powtórzony zasób lub nieprawidłowa ilość łupu."; return false; }
                int previous = Quantity(stack.resourceId);
                if (stack.quantity > int.MaxValue - previous) { error = "Przekroczono maksymalną ilość zasobu."; return false; }
                candidate[stack.resourceId] = previous + stack.quantity;
            }
            return Commit(candidate, LegacyWeight, out error);
        }

        public bool TryRemove(string id, int quantity, out string error)
        {
            error = null;
            int previous = Quantity(id);
            if (quantity <= 0 || previous < quantity) { error = "Brak wymaganej ilości zasobu."; return false; }
            var candidate = new Dictionary<string, int>(items, StringComparer.Ordinal);
            if (quantity == previous) candidate.Remove(id); else candidate[id] = previous - quantity;
            return Commit(candidate, LegacyWeight, out error);
        }

        // Plan kosztu bez zmiany bieżącego ładunku. Pojemność sprawdzana po zużyciu materiałów.
        public bool TryPlanConsumption(ResourceStack[] requirements, float targetCapacity, out ResourceStack[] remaining, out string error)
        {
            remaining = null; error = null;
            if (!Finite(targetCapacity) || targetCapacity <= 0f || requirements == null || requirements.Length > 1024)
            { error = "Nieprawidłowa receptura lub pojemność."; return false; }
            var candidate = new Dictionary<string, int>(items, StringComparer.Ordinal);
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var cost in requirements)
            {
                if (cost == null || cost.resourceId == null || !weights.ContainsKey(cost.resourceId) || cost.quantity <= 0 || !ids.Add(cost.resourceId))
                { error = "Nieznany/powtórzony materiał lub nieprawidłowa ilość w recepturze."; return false; }
                int available = Quantity(cost.resourceId);
                if (available < cost.quantity) { error = "Brakuje materiału: " + cost.resourceId + "."; return false; }
                if (available == cost.quantity) candidate.Remove(cost.resourceId);
                else candidate[cost.resourceId] = available - cost.quantity;
            }
            if (!Calculate(candidate, LegacyWeight, out float weight, out error)) return false;
            if (weight > targetCapacity) { error = "Pozostały ładunek przekracza pojemność nowego statku."; return false; }
            var sorted = new List<string>(candidate.Keys); sorted.Sort(StringComparer.Ordinal);
            remaining = new ResourceStack[sorted.Count];
            for (int i = 0; i < sorted.Count; i++) remaining[i] = new ResourceStack { resourceId = sorted[i], quantity = candidate[sorted[i]] };
            return true;
        }

        // Cała lista jest sprawdzana przed zastąpieniem zawartości.
        public bool CanRestore(ResourceStack[] stacks, float legacyWeight, out float weight, out string error)
            => Validate(stacks, legacyWeight, out _, out weight, out error);

        public bool TryRestore(ResourceStack[] stacks, float legacyWeight, out string error)
        {
            if (!Validate(stacks, legacyWeight, out var candidate, out _, out error)) return false;
            return Commit(candidate, legacyWeight, out error);
        }

        private bool Validate(ResourceStack[] stacks, float legacyWeight, out Dictionary<string, int> candidate, out float weight, out string error)
        {
            candidate = new Dictionary<string, int>(StringComparer.Ordinal); weight = 0f; error = null;
            if (stacks == null || stacks.Length > 1024) { error = "Nieprawidłowa lista ładunku."; return false; }
            foreach (var stack in stacks)
            {
                if (stack == null || stack.resourceId == null || stack.quantity <= 0 || !weights.ContainsKey(stack.resourceId) || candidate.ContainsKey(stack.resourceId))
                { error = "Nieznane lub powtórzone ID zasobu / nieprawidłowa ilość."; return false; }
                candidate.Add(stack.resourceId, stack.quantity);
            }
            return Calculate(candidate, legacyWeight, out weight, out error);
        }

        private bool Calculate(Dictionary<string, int> candidate, float legacyWeight, out float weight, out string error)
        {
            weight = 0f; error = null;
            if (!Finite(legacyWeight) || legacyWeight < 0f) { error = "Nieprawidłowa masa starego ładunku."; return false; }
            double total = legacyWeight;
            foreach (var pair in candidate) total += (double)weights[pair.Key] * pair.Value;
            if (total > Capacity) { error = "Brak miejsca w ładowni."; return false; }
            weight = (float)total; return true;
        }

        private bool Commit(Dictionary<string, int> candidate, float legacyWeight, out string error)
        {
            if (!Calculate(candidate, legacyWeight, out float weight, out error)) return false;
            bool changed = legacyWeight != LegacyWeight || candidate.Count != items.Count;
            if (!changed) foreach (var pair in candidate) if (Quantity(pair.Key) != pair.Value) { changed = true; break; }
            items = candidate; LegacyWeight = legacyWeight; TotalWeight = weight;
            if (changed) Changed?.Invoke();
            return true;
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
