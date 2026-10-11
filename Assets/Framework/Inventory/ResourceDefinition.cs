using UnityEngine;

namespace Framework.Inventory
{
    public enum ResourceCategory { Material, TradeGoods, Ammunition }

    [CreateAssetMenu(fileName = "Resource", menuName = "GameWork/Inventory/Resource")]
    public sealed class ResourceDefinition : ScriptableObject
    {
        [SerializeField, Tooltip("Stałe ID używane w zapisach. Po wydaniu zasobu nie zmieniaj go.")]
        private string resourceId;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField, Min(0.001f)] private float unitWeight = 1f;
        [SerializeField, Min(0)] private int basePrice;
        [SerializeField] private ResourceCategory category;

        public string Id => resourceId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public float UnitWeight => unitWeight;
        public int BasePrice => basePrice;
        public ResourceCategory Category => category;

        public static bool ValidId(string id)
        {
            if (string.IsNullOrEmpty(id) || id.Length > 64) return false;
            foreach (char c in id)
                if (!(c >= 'a' && c <= 'z') && !(c >= '0' && c <= '9') && c != '_' && c != '-') return false;
            return true;
        }

        public bool Validate(out string error)
        {
            error = null;
            if (!ValidId(resourceId)) error = "ID: użyj 1–64 znaków a-z, 0-9, _ lub -.";
            else if (string.IsNullOrWhiteSpace(displayName)) error = "Zasób wymaga nazwy.";
            else if (float.IsNaN(unitWeight) || float.IsInfinity(unitWeight) || unitWeight <= 0f) error = "Masa musi być skończona i dodatnia.";
            else if (basePrice < 0) error = "Cena nie może być ujemna.";
            else if (!System.Enum.IsDefined(typeof(ResourceCategory), category)) error = "Nieprawidłowa kategoria.";
            return error == null;
        }
    }
}
