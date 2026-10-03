using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThePirate.Ports
{
    [DisallowMultipleComponent]
    public sealed class PortDock : MonoBehaviour
    {
        private static readonly HashSet<PortDock> ports = new HashSet<PortDock>();
        [SerializeField] private string portId = "sandbox-port";
        [SerializeField] private string displayName = "Port testowy";
        [SerializeField] private Transform berth;
        [SerializeField, Min(1f)] private float approachRadius = 12f;
        [SerializeField, Min(0f)] private float maximumDockSpeed = 1f;
        private ShipDocking occupant;
        public string Id => portId;
        public string DisplayName => displayName;
        public Transform Berth => berth != null ? berth : transform;
        public float MaximumDockSpeed => maximumDockSpeed;
        public bool AvailableFor(ShipDocking ship) => occupant == null || occupant == ship;
        public bool InRange(Vector3 position) => HorizontalDistance(position, Berth.position) <= approachRadius;
        public void Configure(string id, Transform point) { portId = id; berth = point; }
        public bool TryClaim(ShipDocking ship)
        {
            if (!isActiveAndEnabled || !AvailableFor(ship)) return false;
            occupant = ship; return true;
        }
        public void Release(ShipDocking ship) { if (occupant == ship) occupant = null; }
        private void OnEnable() => ports.Add(this);
        private void OnDisable() { ports.Remove(this); if (occupant != null) occupant.Undock(); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() => ports.Clear();
        public static PortDock Find(Scene scene, string id)
        {
            PortDock found = null;
            foreach (var port in ports)
                if (port != null && port.isActiveAndEnabled && port.gameObject.scene == scene && port.Id == id)
                {
                    if (found != null) return null; // Ambiguous identifiers must never restore to an arbitrary berth.
                    found = port;
                }
            return found;
        }
        public static PortDock Nearest(Scene scene, Vector3 position)
        {
            PortDock found = null; float best = float.PositiveInfinity;
            foreach (var port in ports)
            {
                if (port == null || !port.isActiveAndEnabled || port.gameObject.scene != scene || !port.InRange(position)) continue;
                float distance = HorizontalDistance(position, port.Berth.position);
                if (distance < best) { found = port; best = distance; }
            }
            return found;
        }
        private static float HorizontalDistance(Vector3 a, Vector3 b) { a.y = b.y = 0f; return Vector3.Distance(a, b); }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(Berth.position, approachRadius);
            Gizmos.color = Color.green; Gizmos.DrawRay(Berth.position, Berth.forward * 5f);
        }
    }
}
