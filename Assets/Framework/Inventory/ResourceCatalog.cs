using System.Collections.Generic;
using UnityEngine;

namespace Framework.Inventory
{
    [CreateAssetMenu(fileName = "ResourceCatalog", menuName = "GameWork/Inventory/Resource Catalog")]
    public sealed class ResourceCatalog : ScriptableObject
    {
        [SerializeField] private ResourceDefinition[] resources = new ResourceDefinition[0];
        public IReadOnlyList<ResourceDefinition> Resources => System.Array.AsReadOnly(resources ?? new ResourceDefinition[0]);

        public bool TryBuildWeights(out Dictionary<string, float> weights, out string error)
        {
            weights = new Dictionary<string, float>(System.StringComparer.Ordinal);
            error = null;
            if (resources == null || resources.Length > 1024) { error = "Nieprawidłowy katalog zasobów (limit 1024)."; return false; }
            foreach (var resource in resources)
            {
                if (resource == null) { error = "Katalog zawiera pusty wpis."; return false; }
                if (!resource.Validate(out error)) { error = resource.name + ": " + error; return false; }
                if (weights.ContainsKey(resource.Id)) { error = "Powtórzone ID: " + resource.Id; return false; }
                weights.Add(resource.Id, resource.UnitWeight);
            }
            return true;
        }

        public ResourceDefinition Find(string id)
        {
            if (resources != null) foreach (var resource in resources)
                if (resource != null && resource.Id == id) return resource;
            return null;
        }
    }
}
