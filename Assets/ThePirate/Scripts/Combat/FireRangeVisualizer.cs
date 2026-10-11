using UnityEngine;
using UnityEngine.UI;

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
        [Header("Linie ekranowe")]
        [SerializeField] private Camera indicatorCamera;
        [SerializeField, Range(0.5f, 6f)] private float desktopWidthPixels = 1.25f;
        [SerializeField, Range(0.5f, 6f)] private float mobileWidthPixels = 2f;
        [SerializeField, Range(0f, 8f)] private float glowPixels = 2f;
        private GameObject overlay;
        internal Camera ViewCamera => indicatorCamera != null ? indicatorCamera : Camera.main;
        internal float WidthPixels => Application.isMobilePlatform ? mobileWidthPixels : desktopWidthPixels;
        internal float GlowPixels => glowPixels;
        private ShipCombatController combat;
        private SpecialWeaponController special;
        private LineRenderer[] lines;
        private LineRenderer[][] batteryTrajectories;
        private LineRenderer circle;
        private LineRenderer trajectory;
        private Material material;
        private void Start()
        {
            combat = GetComponent<ShipCombatController>(); special = GetComponent<SpecialWeaponController>();
            var shader = Shader.Find("Sprites/Default");
            if (shader == null) { enabled = false; return; }
            material = new Material(shader);
            overlay = new GameObject("Aim lines overlay", typeof(RectTransform), typeof(Canvas));
            var canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = -10;
            // Rysuj po przezroczystej wodzie. Shader nadal sprawdza głębokość sceny.
            material.renderQueue = indicatorRenderQueue;
            lines = new LineRenderer[combat.Batteries.Length];
            batteryTrajectories = new LineRenderer[lines.Length][];
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = MakeLine("Sector " + i);
                batteryTrajectories[i] = new LineRenderer[combat.Batteries[i].CannonCount];
                for (int j = 0; j < batteryTrajectories[i].Length; j++)
                {
                    var path = MakeLine("Battery " + i + " trajectory " + j);
                    path.loop = false; path.widthMultiplier = 0.12f; path.enabled = false;
                    batteryTrajectories[i][j] = path;
                }
            }
            circle = MakeLine("Mortar target");
            trajectory = MakeLine("Mortar trajectory");
            trajectory.loop = false;
            trajectory.widthMultiplier = 0.16f;
            trajectory.enabled = false;
        }
        private LineRenderer MakeLine(string label)
        {
            var go = new GameObject(label); go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>(); line.sharedMaterial = material;
            line.useWorldSpace = true; line.widthMultiplier = 0.08f; line.loop = true; line.positionCount = 258;
            line.forceRenderingOff = true;
            var graphicObject = new GameObject(label, typeof(RectTransform), typeof(AimLineGraphic));
            graphicObject.transform.SetParent(overlay.transform, false);
            var rect = (RectTransform)graphicObject.transform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var graphic = graphicObject.GetComponent<AimLineGraphic>();
            graphic.Source = line; graphic.Owner = this; graphic.raycastTarget = false;
            return line;
        }
        private void LateUpdate()
        {
            if (lines == null) return;
            for (int i = 0; i < lines.Length; i++)
            {
                var battery = combat.Batteries[i]; var line = lines[i];
                bool selectedReady = combat.HasAim && combat.Selected == battery && battery.Ready
                    && !(special != null && special.Aiming);
                line.enabled = showSectors && combat.HasAim && !(special != null && special.Aiming)
                    && (selectedReady || (combat.Selected != null && battery.Ready));
                Color color = selectedReady ? new Color(1f, 0.78f, 0.36f, 0.48f)
                    : new Color(0.78f, 0.85f, 0.9f, 0.08f);
                line.startColor = line.endColor = color;
                for (int c = 0; c < batteryTrajectories[i].Length; c++)
                {
                    var path = batteryTrajectories[i][c];
                    path.enabled = false;
                    if (!selectedReady || !battery.TryGetTrajectory(c, combat.AimPoint, out var shotOrigin, out var shotVelocity, out var shotDuration)) continue;
                    path.enabled = true;
                    path.startColor = path.endColor = new Color(1f, 0.94f, 0.73f, 0.85f);
                    for (int p = 0; p < path.positionCount; p++)
                    {
                        float time = shotDuration * p / (path.positionCount - 1);
                        path.SetPosition(p, shotOrigin + shotVelocity * time + 0.5f * Physics.gravity * time * time);
                    }
                }
                if (!line.enabled) continue;
                for (int j = 0; j <= 128; j++)
                {
                    float angle = -battery.FiringArc / 2 + battery.FiringArc * j / 128;
                    Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * battery.transform.forward;
                    Vector3 center = battery.transform.position; center.y = combat.WaterHeight + surfaceOffset;
                    line.SetPosition(j, center + direction * battery.MaximumRange);
                    line.SetPosition(257 - j, center + direction * battery.MinimumRange);
                }
            }
            circle.enabled = special != null && special.Aiming;
            trajectory.enabled = false;
            if (!circle.enabled) return;
            circle.startColor = circle.endColor = special.ValidTarget
                ? new Color(1f, 0.85f, 0.46f, 0.85f) : new Color(1f, 0.3f, 0.22f, 0.85f);
            if (special.TryGetTrajectory(out Vector3 origin, out Vector3 velocity, out float duration))
            {
                trajectory.enabled = true;
                trajectory.startColor = trajectory.endColor = circle.startColor;
                for (int i = 0; i < trajectory.positionCount; i++)
                {
                    float time = duration * i / (trajectory.positionCount - 1);
                    trajectory.SetPosition(i, origin + velocity * time + 0.5f * Physics.gravity * time * time);
                }
            }
            for (int i = 0; i < circle.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2 / circle.positionCount;
                Vector3 center = special.Target; center.y = combat.WaterHeight + surfaceOffset;
                circle.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * special.Radius, 0f, Mathf.Sin(angle) * special.Radius));
            }
        }
        private void OnEnable() { if (overlay != null) overlay.SetActive(true); }
        private void OnDisable()
        {
            if (overlay != null) overlay.SetActive(false);
            if (lines != null) foreach (var line in lines) if (line != null) line.enabled = false;
            if (batteryTrajectories != null) foreach (var paths in batteryTrajectories) foreach (var path in paths) if (path != null) path.enabled = false;
            if (circle != null) circle.enabled = false;
            if (trajectory != null) trajectory.enabled = false;
        }
        private void OnDestroy() { if (material != null) Destroy(material); if (overlay != null) Destroy(overlay); }
    }
}

namespace ThePirate.Combat
{
    // Projekcja podczas budowania UI uwzględnia końcową pozycję kamery.
    public sealed class AimLineGraphic : MaskableGraphic
    {
        internal LineRenderer Source;
        internal FireRangeVisualizer Owner;
        private Vector2[] points = new Vector2[0];
        private bool[] visible = new bool[0];
        private void LateUpdate() { SetVerticesDirty(); }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (Owner == null || !Owner.isActiveAndEnabled || Source == null || !Source.enabled) return;
            Camera camera = Owner.ViewCamera;
            if (camera == null || camera.pixelHeight <= 0) return;
            int count = Source.positionCount;
            if (count < 2) return;
            if (points.Length != count) { points = new Vector2[count]; visible = new bool[count]; }
            for (int i = 0; i < count; i++)
            {
                Vector3 screen = camera.WorldToScreenPoint(Source.GetPosition(i));
                visible[i] = screen.z >= camera.nearClipPlane && screen.z <= camera.farClipPlane;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screen, null, out points[i]);
            }
            Color tint = Source.startColor;
            if (Owner.GlowPixels > 0f)
            {
                Color glow = tint; glow.a *= 0.15f;
                Draw(vh, count, Owner.WidthPixels * 0.5f, Owner.GlowPixels, glow);
            }
            Draw(vh, count, Owner.WidthPixels * 0.5f, 1f, tint);
        }

        private Vector2 Normal(int index, int count)
        {
            int previous = index == 0 ? (Source.loop ? count - 1 : 0) : index - 1;
            int next = index == count - 1 ? (Source.loop ? 0 : index) : index + 1;
            Vector2 incoming = (points[index] - points[previous]).normalized;
            Vector2 outgoing = (points[next] - points[index]).normalized;
            if (incoming.sqrMagnitude < 0.01f) incoming = outgoing;
            if (outgoing.sqrMagnitude < 0.01f) outgoing = incoming;
            Vector2 tangent = (incoming + outgoing).normalized;
            Vector2 normal = new Vector2(-tangent.y, tangent.x);
            Vector2 edgeNormal = new Vector2(-outgoing.y, outgoing.x);
            return normal / Mathf.Max(0.5f, Mathf.Abs(Vector2.Dot(normal, edgeNormal)));
        }

        private void Draw(VertexHelper vh, int count, float halfWidth, float feather, Color tint)
        {
            int segments = Source.loop ? count : count - 1;
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % count;
                if (!visible[i] || !visible[next] || (points[next] - points[i]).sqrMagnitude < 0.001f) continue;
                Vector2 a = Normal(i, count), b = Normal(next, count);
                Quad(vh, points[i] - a * halfWidth, points[i] + a * halfWidth,
                    points[next] + b * halfWidth, points[next] - b * halfWidth, tint, tint, tint, tint);
                Color clear = tint; clear.a = 0f;
                Quad(vh, points[i] + a * halfWidth, points[i] + a * (halfWidth + feather),
                    points[next] + b * (halfWidth + feather), points[next] + b * halfWidth, tint, clear, clear, tint);
                Quad(vh, points[i] - a * (halfWidth + feather), points[i] - a * halfWidth,
                    points[next] - b * halfWidth, points[next] - b * (halfWidth + feather), clear, tint, tint, clear);
            }
        }

        private static void Quad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d,
            Color ca, Color cb, Color cc, Color cd)
        {
            int start = vh.currentVertCount;
            vh.AddVert(a, ca, Vector2.zero); vh.AddVert(b, cb, Vector2.zero);
            vh.AddVert(c, cc, Vector2.zero); vh.AddVert(d, cd, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2); vh.AddTriangle(start, start + 2, start + 3);
        }
    }
}

