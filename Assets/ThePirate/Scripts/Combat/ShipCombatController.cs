using UnityEngine;

namespace ThePirate.Combat
{
    [RequireComponent(typeof(CombatShip))]
    public sealed class ShipCombatController : MonoBehaviour
    {
        [SerializeField] private Camera aimCamera;
        [SerializeField] private float waterHeight;
        public WeaponBattery[] Batteries { get; private set; }
        public Vector3 AimPoint { get; private set; }
        public bool HasAim { get; private set; }
        public WeaponBattery Selected { get; private set; }
        private void Awake() => Batteries = GetComponentsInChildren<WeaponBattery>();
        public void Aim(Vector2 screenPoint)
        {
            if (aimCamera == null) aimCamera = Camera.main;
            HasAim = false; Selected = null;
            if (aimCamera == null) return;
            Ray ray = aimCamera.ScreenPointToRay(screenPoint);
            if (!new Plane(Vector3.up, new Vector3(0, waterHeight, 0)).Raycast(ray, out float distance)) return;
            AimPoint = ray.GetPoint(distance); HasAim = true;
            float best = float.MaxValue;
            foreach (var battery in Batteries)
            {
                if (!battery.isActiveAndEnabled || !battery.Contains(AimPoint)) continue;
                float angle = battery.Alignment(AimPoint);
                if (angle < best) { best = angle; Selected = battery; }
            }
        }
        public bool Fire() => HasAim && Selected != null && Selected.TryFire(AimPoint);
    }
}
