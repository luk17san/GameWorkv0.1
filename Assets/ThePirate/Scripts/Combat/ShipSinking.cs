using Framework.Health;
using GameWork.Framework.Ships.Movement;
using ThePirate.AI;
using UnityEngine;

namespace ThePirate.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatShip))]
    public sealed class ShipSinking : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float duration = 3f;
        [SerializeField, Min(0f)] private float depth = 6f;
        [SerializeField, Range(-60f, 60f)] private float roll = 18f;
        private Health hull;
        private CombatShip ship;
        private bool sinking;
        private float elapsed;
        private Vector3 origin;
        private Quaternion rotation;

        // Existing enemies work without rebuilding their prefabs. A serialized
        // component on the prefab can override the defaults in the Inspector.
        public static ShipSinking Ensure(CombatShip target)
        {
            if (target.Team == 0 || target.Team == 1) return null;
            var result = target.GetComponent<ShipSinking>();
            return result != null ? result : target.gameObject.AddComponent<ShipSinking>();
        }

        private void Awake()
        {
            hull = GetComponent<Health>();
            ship = GetComponent<CombatShip>();
        }
        private void OnEnable() => hull.Depleted += Begin;
        private void OnDisable() => hull.Depleted -= Begin;
        private void Start() { if (hull.IsDepleted) Begin(); }

        private void Begin()
        {
            if (sinking || !hull.IsDepleted || ship.Team == 0 || ship.Team == 1) return;
            sinking = true;
            origin = transform.position;
            rotation = transform.rotation;
            foreach (var collider in GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            Disable<EnemyShipAI>();
            Disable<ShipMovementController>();
            Disable<ShipConditionController>();
            Disable<ShipAutoFire>();
            Disable<ShipCombatController>();
            Disable<FireRangeVisualizer>();
            Disable<CannonPlayerInput>();
            Disable<CrewAutoFireSystem>();
            Disable<SpecialWeaponController>();
            Disable<WeaponBattery>();
            Disable<Cannon>();
            Disable<ShipCollisionDamage>();
            foreach (var body in GetComponentsInChildren<Rigidbody>(true))
            {
                if (!body.isKinematic)
                {
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
                body.detectCollisions = false;
                body.isKinematic = true;
                body.interpolation = RigidbodyInterpolation.None;
            }
        }

        private void Disable<T>() where T : Behaviour
        {
            foreach (var component in GetComponentsInChildren<T>(true)) component.enabled = false;
        }

        private void Update()
        {
            if (!sinking || global::Framework.Menu.PauseService.GameplayInputBlocked) return;
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / Mathf.Max(0.1f, duration));
            transform.SetPositionAndRotation(origin + Vector3.down * (depth * progress),
                rotation * Quaternion.Euler(0f, 0f, roll * progress));
            if (progress >= 1f) Finish();
        }

        // Saves taken during sinking restore the defeated state, not a second animation.
        public void FinishLoadedDefeat()
        {
            Begin();
            if (sinking) Finish();
        }

        private void Finish()
        {
            // Remove from ActiveShips immediately, before deferred Destroy runs.
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
