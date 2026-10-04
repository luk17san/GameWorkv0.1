using UnityEngine;

namespace ThePirate.Combat
{
    [RequireComponent(typeof(ShipCombatController))]
    public sealed class FireRangeVisualizer : MonoBehaviour
    {
        [SerializeField] private bool showSectors = true;
        [Tooltip("Wysokość znaczników ponad płaszczyzną wody; ustaw powyżej maksymalnej wysokości fal.")]
        [SerializeField, Min(0f)] private float surfaceOffset = 0.65f;
        [Tooltip("Kolejka renderowania znaczników; powinna być późniejsza niż kolejka materiału wody.")]
        [SerializeField, Range(3001, 3999)] private int indicatorRenderQueue = 3100;
        private ShipCombatController combat;
        private SpecialWeaponController special;
        private LineRenderer[] lines;
        private LineRenderer circle;
        private Material material;
        private void Start()
        {
            combat = GetComponent<ShipCombatController>(); special = GetComponent<SpecialWeaponController>();
            var shader = Shader.Find("Sprites/Default");
            if (shader == null) { enabled = false; return; }
            material = new Material(shader);
            // Rysuj po przezroczystej wodzie. Shader nadal sprawdza głębokość sceny.
            material.renderQueue = indicatorRenderQueue;
            lines = new LineRenderer[combat.Batteries.Length];
            for (int i = 0; i < lines.Length; i++) lines[i] = MakeLine("Sector " + i);
            circle = MakeLine("Mortar target");
        }
        private LineRenderer MakeLine(string label)
        {
            var go = new GameObject(label); go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>(); line.sharedMaterial = material;
            line.useWorldSpace = true; line.widthMultiplier = 0.08f; line.loop = true; line.positionCount = 66;
            return line;
        }
        private void LateUpdate()
        {
            if (lines == null) return;
            for (int i = 0; i < lines.Length; i++)
            {
                var battery = combat.Batteries[i]; var line = lines[i];
                line.enabled = showSectors && !(special != null && special.Aiming);
                Color color = !battery.Ready ? Color.gray : combat.Selected == battery ? battery.CanFire(combat.AimPoint) ? Color.green : Color.red : new Color(1, 1, 1, 0.3f);
                line.startColor = line.endColor = color;
                for (int j = 0; j <= 32; j++)
                {
                    float angle = -battery.FiringArc / 2 + battery.FiringArc * j / 32;
                    Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * battery.transform.forward;
                    Vector3 center = battery.transform.position; center.y = combat.WaterHeight + surfaceOffset;
                    line.SetPosition(j, center + direction * battery.MaximumRange);
                    line.SetPosition(65 - j, center + direction * battery.MinimumRange);
                }
            }
            circle.enabled = special != null && special.Aiming;
            if (!circle.enabled) return;
            circle.startColor = circle.endColor = special.ValidTarget ? Color.green : Color.red;
            for (int i = 0; i < 66; i++)
            {
                float angle = i * Mathf.PI * 2 / 66;
                Vector3 center = special.Target; center.y = combat.WaterHeight + surfaceOffset;
                circle.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * special.Radius, 0f, Mathf.Sin(angle) * special.Radius));
            }
        }
        private void OnDisable() { if (lines != null) foreach (var line in lines) if (line != null) line.enabled = false; if (circle != null) circle.enabled = false; }
        private void OnDestroy() { if (material != null) Destroy(material); }
    }
}
