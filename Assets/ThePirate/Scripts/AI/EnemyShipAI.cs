using GameWork.Framework.Ships.Movement;
using ThePirate.Combat;
using UnityEngine;

namespace ThePirate.AI
{
    public enum EnemyShipState { Waiting, Approaching, HoldingDistance, Returning, Destroyed, PositioningBroadside, Circling, GainingDistance }

    [DefaultExecutionOrder(-50)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatShip), typeof(ShipMovementController))]
    public sealed class EnemyShipAI : MonoBehaviour
    {
        [Header("Detection and territory (open water)")]
        [SerializeField, Min(1f)] private float detectionRange = 80f;
        [SerializeField, Min(1f)] private float loseTargetRange = 110f;
        [SerializeField, Min(1f)] private float territoryRadius = 140f;
        [Header("Approach")]
        [SerializeField, Min(1f)] private float approachDistance = 30f;
        [SerializeField, Min(0.1f)] private float resumeMargin = 5f;
        [SerializeField, Min(0.1f)] private float cruiseSpeed = 8f;
        [SerializeField, Min(0.1f)] private float homeTolerance = 3f;
        [Header("Broadside manoeuvres (stage 2)")]
        [SerializeField] private bool enableManeuvers = true;
        [SerializeField, Min(1f)] private float minimumDistance = 18f;
        [SerializeField, Min(0.1f)] private float maneuverSpeed = 4.5f;
        [SerializeField, Min(1f)] private float turnInDistance = 15f;
        [Header("Broadside fire (stage 3)")]
        [SerializeField] private bool enableWeapons = true;
        [SerializeField, Min(0.05f)] private float fireCheckInterval = 0.2f;
        [Header("Runtime (diagnostics)")]
        [SerializeField] private int salvosStarted;
        [SerializeField] private WeaponBattery lastFiredBattery;
        [SerializeField] private bool conflictingFireController;
        private float nextFireCheck;
        private ShipAutoFire autoFire;
        private CannonPlayerInput playerFire;
        [SerializeField] private int selectedSide;
        [SerializeField] private bool targetInBroadsideSector;
        private WeaponBattery[] batteries;
        [SerializeField] private EnemyShipState currentState;
        [SerializeField] private CombatShip target;
        private CombatShip ship;
        private ShipMovementController movement;
        private Rigidbody body;
        private Vector3 home;
        public EnemyShipState CurrentState => currentState;
        public CombatShip Target => target;
        public int SelectedSide => selectedSide; // -1 = left, +1 = right, 0 = none
        public bool TargetInBroadsideSector => targetInBroadsideSector;

        private void Awake()
        {
            ship = GetComponent<CombatShip>();
            movement = GetComponent<ShipMovementController>();
            body = GetComponent<Rigidbody>();
            home = transform.position;
            batteries = GetComponentsInChildren<WeaponBattery>();
            autoFire = GetComponent<ShipAutoFire>();
            playerFire = GetComponent<CannonPlayerInput>();
            salvosStarted = 0;
            lastFiredBattery = null;
        }

        private void Start()
        {
            if (!movement.ExternalControl || !movement.enabled || !movement.HasMovementConfiguration)
            {
                Debug.LogError("EnemyShipAI requires an enabled movement controller, Movement Stats and External Control.", this);
                enabled = false;
            }
        }

        private void FixedUpdate()
        {
            if (global::Framework.Menu.PauseService.GameplayInputBlocked) return;
            targetInBroadsideSector = false;
            if (!ship.Alive)
            {
                target = null;
                selectedSide = 0;
                currentState = EnemyShipState.Destroyed;
                Stop();
                return;
            }
            if (currentState == EnemyShipState.Destroyed) currentState = EnemyShipState.Returning;

            if (currentState == EnemyShipState.Returning)
            {
                ReturnHome();
                return;
            }
            if (target != null && (!ship.IsEnemy(target) ||
                Distance(body.position, target.transform.position) > loseTargetRange ||
                Distance(home, target.transform.position) > territoryRadius ||
                Distance(home, body.position) > territoryRadius))
            {
                target = null;
                selectedSide = 0;
                currentState = EnemyShipState.Returning;
                ReturnHome();
                return;
            }
            // A destroyed Unity object compares equal to null: leave combat instead of waiting offshore.
            if (target == null && (currentState != EnemyShipState.Waiting))
            {
                selectedSide = 0;
                currentState = EnemyShipState.Returning;
                ReturnHome();
                return;
            }
            if (target == null) FindTarget();
            if (target == null)
            {
                currentState = EnemyShipState.Waiting;
                Stop();
                return;
            }

            if (enableManeuvers)
            {
                Maneuver();
                TryBroadsideFire();
                return;
            }
            float distance = Distance(body.position, target.transform.position);
            bool hold = EnemyNavigationMath.ShouldHold(currentState == EnemyShipState.HoldingDistance, distance, approachDistance, resumeMargin);
            currentState = hold ? EnemyShipState.HoldingDistance : EnemyShipState.Approaching;
            if (hold) Stop();
            else Navigate(target.transform.position, approachDistance);
        }

        private void FindTarget()
        {
            float nearest = detectionRange;
            foreach (var candidate in CombatShip.ActiveShips)
            {
                if (!ship.IsEnemy(candidate) || candidate.gameObject.scene != gameObject.scene ||
                    Distance(home, candidate.transform.position) > territoryRadius) continue;
                float distance = Distance(body.position, candidate.transform.position);
                if (distance > nearest) continue;
                nearest = distance;
                target = candidate;
            }
        }

        private void Maneuver()
        {
            Vector3 delta = target.transform.position - body.position;
            delta.y = 0f;
            float distance = delta.magnitude;
            Vector3 toTarget = distance > 0.01f ? delta / distance : body.rotation * Vector3.forward;
            if (selectedSide == 0)
            {
                float bearing = Vector3.SignedAngle(body.rotation * Vector3.forward, toTarget, Vector3.up);
                selectedSide = EnemyNavigationMath.ChooseSide(bearing);
            }
            foreach (var battery in batteries)
            {
                if (!IsSelectedBroadside(battery)) continue;
                if (battery.Contains(target.transform.position)) targetInBroadsideSector = true;
            }

            bool escaping = EnemyNavigationMath.ShouldGainDistance(
                currentState == EnemyShipState.GainingDistance, distance, minimumDistance, resumeMargin);
            bool alreadyManeuvering = currentState == EnemyShipState.PositioningBroadside ||
                currentState == EnemyShipState.Circling || currentState == EnemyShipState.GainingDistance;
            bool closeEnough = distance <= approachDistance + turnInDistance + (alreadyManeuvering ? resumeMargin : 0f);
            if (!escaping && !closeEnough)
            {
                currentState = EnemyShipState.Approaching;
                Navigate(target.transform.position, approachDistance + turnInDistance);
                return;
            }

            float radial = EnemyNavigationMath.RadialCorrection(distance, approachDistance, turnInDistance);
            EnemyNavigationMath.BroadsideDirection(toTarget.x, toTarget.z, selectedSide, radial, escaping,
                out float directionX, out float directionZ);
            Vector3 direction = new Vector3(directionX, 0f, directionZ);
            float angle = Vector3.SignedAngle(body.rotation * Vector3.forward, direction, Vector3.up);
            float speed = EnemyNavigationMath.ManeuverSpeed(maneuverSpeed, angle);
            movement.SetNavigationCommand(speed, Mathf.Clamp(angle / 15f, -1f, 1f));
            currentState = escaping ? EnemyShipState.GainingDistance :
                targetInBroadsideSector ? EnemyShipState.Circling : EnemyShipState.PositioningBroadside;
        }

        private void ReturnHome()
        {
            if (Distance(body.position, home) <= homeTolerance)
            {
                Stop();
                if (Mathf.Abs(movement.State.CurrentPropulsionSpeed) < 0.1f)
                    currentState = EnemyShipState.Waiting;
            }
            else Navigate(home, homeTolerance * 0.5f);
        }

        private bool IsSelectedBroadside(WeaponBattery battery)
        {
            if (selectedSide == 0 || battery == null || !battery.isActiveAndEnabled) return false;
            Vector3 localDirection = transform.InverseTransformDirection(battery.transform.forward);
            return localDirection.x * selectedSide >= 0.7f;
        }

        private void TryBroadsideFire()
        {
            // The AI owns fire requests; never compete with generic auto-fire or player input.
            conflictingFireController = (autoFire != null && autoFire.isActiveAndEnabled) ||
                (playerFire != null && playerFire.isActiveAndEnabled);
            if (!enableWeapons || conflictingFireController ||
                global::Framework.Menu.PauseService.GameplayInputBlocked ||
                !ship.Alive || !ship.IsEnemy(target) || currentState != EnemyShipState.Circling ||
                Time.time < nextFireCheck) return;

            nextFireCheck = Time.time + fireCheckInterval;
            foreach (var battery in batteries)
            {
                if (!IsSelectedBroadside(battery)) continue;
                // TryFire checks readiness, live firing sector/range, damage and each muzzle's path.
                // Keep automatic=true: this also rejects friendly/neutral obstructions.
                if (!battery.TryFire(target.transform.position, true, target)) continue;
                lastFiredBattery = battery;
                salvosStarted++;
            }
        }

        private void Navigate(Vector3 destination, float stoppingDistance)
        {
            Vector3 delta = destination - body.position;
            delta.y = 0f;
            float angle = Vector3.SignedAngle(body.rotation * Vector3.forward, delta, Vector3.up);
            float steering = Mathf.Clamp(angle / 30f, -1f, 1f);
            float speed = EnemyNavigationMath.ApproachSpeed(delta.magnitude - stoppingDistance,
                movement.BrakingDeceleration, cruiseSpeed, angle);
            movement.SetNavigationCommand(speed, steering);
        }

        private static float Distance(Vector3 a, Vector3 b)
        {
            a.y = b.y = 0f;
            return Vector3.Distance(a, b);
        }

        private void Stop() => movement.SetNavigationCommand(0f, 0f);
        private void OnDisable() { if (movement != null) Stop(); }

        private void OnValidate()
        {
            approachDistance = Mathf.Max(1f, approachDistance);
            resumeMargin = Mathf.Max(0.1f, resumeMargin);
            detectionRange = Mathf.Max(approachDistance + resumeMargin, detectionRange);
            loseTargetRange = Mathf.Max(detectionRange + resumeMargin, loseTargetRange);
            territoryRadius = Mathf.Max(loseTargetRange, territoryRadius);
            cruiseSpeed = Mathf.Max(0.1f, cruiseSpeed);
            homeTolerance = Mathf.Max(0.1f, homeTolerance);
            minimumDistance = Mathf.Clamp(minimumDistance, 1f, Mathf.Max(1f, approachDistance - resumeMargin));
            maneuverSpeed = Mathf.Clamp(maneuverSpeed, 0.1f, cruiseSpeed);
            turnInDistance = Mathf.Max(1f, turnInDistance);
            fireCheckInterval = Mathf.Max(0.05f, fireCheckInterval);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, approachDistance);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, minimumDistance);
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(Application.isPlaying ? home : transform.position, territoryRadius);
            if (target != null) { Gizmos.color = Color.red; Gizmos.DrawLine(transform.position, target.transform.position); }
        }
    }
}
