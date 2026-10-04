using UnityEngine;

namespace ThePirate.Combat
{
    [RequireComponent(typeof(CombatShip))]
    public sealed class SpecialWeaponController : MonoBehaviour
    {
        [SerializeField] private Cannon mortar;
        [SerializeField] private ShipDamageModule condition;
        [SerializeField, Min(1)] private float minimumRange = 20f;
        [SerializeField, Min(1)] private float maximumRange = 100f;
        [SerializeField, Min(0.1f)] private float flightTime = 3f;
        [SerializeField, Min(0.1f)] private float reloadTime = 15f;
        [SerializeField, Min(1)] private int damage = 100;
        [SerializeField, Min(1f)] private float minimumArcHeight = 25f;
        [SerializeField] private bool damageAllies;
        [SerializeField] private bool damageOwner;
        [SerializeField, Min(0.1f)] private float blastRadius = 6f;
        private CombatShip ship;
        private Vector3 target;
        private bool hasTarget;
        private float nextFire;
        public bool Aiming { get; private set; }
        public Vector3 Target => target;
        public float Radius => blastRadius;
        public float ReloadRemaining => Mathf.Max(0f, nextFire - Time.time);
        public float ReloadDuration => Mathf.Max(0.1f, reloadTime);
        public bool Destroyed => condition != null && condition.Destroyed;
        public bool Ready => isActiveAndEnabled && mortar != null && mortar.Ready && ship != null
            && ship.WeaponsAllowed && !Destroyed && ReloadRemaining <= 0f;
        public void RestoreReload(float seconds)
        {
            Cancel();
            nextFire = Time.time + Mathf.Max(0f, seconds);
        }
        public bool ValidTarget => hasTarget && mortar != null && mortar.Ready && ship.WeaponsAllowed && (condition == null || !condition.Destroyed)
            && Time.time >= nextFire && Vector3.Distance(transform.position, target) >= minimumRange && Vector3.Distance(transform.position, target) <= maximumRange;
        private void Awake() => ship = GetComponent<CombatShip>();
        public void Configure(Cannon cannon, ShipDamageModule module) { mortar = cannon; condition = module; }
        public void ToggleAim() { Aiming = !Aiming && mortar != null && ship.WeaponsAllowed; hasTarget = false; }
        public void Cancel() { Aiming = false; hasTarget = false; }
        public void SetAim(Vector3 point, bool valid) { target = point; hasTarget = valid; }
        private void OnDisable() => Cancel();
        public bool TryFire()
        {
            if (!Aiming || !ValidTarget || Time.timeScale <= 0) return false;
            if (Physics.gravity.y >= -0.01f) return false;
            // Dłuższy lot zapewnia wysoki łuk także przy większym zasięgu.
            float duration = Mathf.Max(Mathf.Max(0.1f, flightTime), Mathf.Sqrt(8f * minimumArcHeight / -Physics.gravity.y));
            Vector3 velocity = (target - mortar.Muzzle.position - 0.5f * Physics.gravity * duration * duration) / duration;
            if (!mortar.Emit(velocity.normalized, damage, velocity.magnitude, duration + 5f, AmmunitionKind.Mortar, true, blastRadius, damageAllies, damageOwner, target.y)) return false;
            nextFire = Time.time + reloadTime; Cancel(); return true;
        }
    }
}
