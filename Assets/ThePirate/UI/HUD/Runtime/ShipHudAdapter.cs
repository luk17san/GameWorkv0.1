using System.Collections.Generic;
using Framework.Health;
using GameWork.Framework.Ships.Movement;
using ThePirate.Combat;
using ThePirate.Ships;
using UnityEngine;

namespace ThePirate.UI.HUD
{
    [DisallowMultipleComponent]
    public sealed class ShipHudAdapter : MonoBehaviour
    {
        private Health health;
        private CombatShip ship, target;
        private ShipMovementController movement;
        private ShipMovement legacyMovement;
        private ShipCombatController combat;
        private Rigidbody body;
        private readonly List<HudBattery> batteries = new List<HudBattery>();
        private readonly List<HudMapMarker> markers = new List<HudMapMarker>();

        public void ResolveSources()
        {
            health = GetComponent<Health>(); ship = GetComponent<CombatShip>();
            movement = GetComponent<ShipMovementController>(); legacyMovement = GetComponent<ShipMovement>();
            combat = GetComponent<ShipCombatController>(); body = GetComponent<Rigidbody>();
        }

        // Podłączenie przyszłego systemu wyboru celu; punkt celowania nie oznacza wybranego statku.
        public void SetTarget(CombatShip value) => target = value;

        public void ReadInto(PlayerHudStateStore store)
        {
            store.SetDurability(health != null ? new HudDurability(health.CurrentHealth, health.MaxHealth) : default);
            float speed = body != null ? Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude : 0;
            if (movement != null && movement.isActiveAndEnabled)
            {
                var state = movement.State;
                HudDriveMode drive = state.DriveState == ShipDriveState.Reverse ? HudDriveMode.Reverse
                    : state.DriveState == ShipDriveState.Braking ? HudDriveMode.Braking
                    : state.DriveState == ShipDriveState.Forward ? HudDriveMode.Forward : HudDriveMode.Stopped;
                store.SetMovement(new HudMovement(speed, state.RequestedTargetSpeed, drive));
            }
            else if (legacyMovement != null && legacyMovement.isActiveAndEnabled)
                store.SetMovement(new HudMovement(speed, legacyMovement.TargetSpeed,
                    legacyMovement.CurrentSpeed > legacyMovement.TargetSpeed + 0.01f ? HudDriveMode.Braking
                    : legacyMovement.TargetSpeed > 0 ? HudDriveMode.Forward : HudDriveMode.Stopped));
            else store.SetMovement(default);

            store.SetCargo(movement != null ? new HudCargo(movement.CurrentCargoWeight,
                movement.CargoCapacity, movement.HasCargoCapacity) : default);
            batteries.Clear();
            if (combat != null && combat.Batteries != null)
                foreach (var battery in combat.Batteries)
                {
                    if (battery == null) continue;
                    Vector3 forward = transform.InverseTransformDirection(battery.transform.forward);
                    HudBatterySide side = Mathf.Abs(forward.x) > Mathf.Abs(forward.z)
                        ? (forward.x < 0 ? HudBatterySide.Left : HudBatterySide.Right)
                        : (forward.z < 0 ? HudBatterySide.Stern : HudBatterySide.Bow);
                    var condition = battery.GetComponent<ShipDamageModule>();
                    batteries.Add(new HudBattery(EntityId.ToULong(battery.GetEntityId()), side, battery.CannonCount,
                        battery.Ready, combat.Selected == battery, condition != null && condition.Destroyed,
                        battery.ReloadRemaining, battery.ReloadDuration));
                }
            batteries.Sort((a, b) => a.Side != b.Side ? a.Side.CompareTo(b.Side) : a.Id.CompareTo(b.Id));
            store.SetBatteries(batteries);

            bool hasAim = combat != null && combat.isActiveAndEnabled && combat.HasAim;
            var selected = hasAim ? combat.Selected : null;
            store.SetAim(hasAim ? new HudAim(combat.AimPoint, selected != null ? EntityId.ToULong(selected.GetEntityId()) : 0UL,
                selected != null && selected.Contains(combat.AimPoint),
                selected != null && Time.timeScale > 0 && !Framework.Menu.PauseService.GameplayInputBlocked
                    && selected.CanFire(combat.AimPoint)) : default);

            if (target != null && target.Alive && target.gameObject.scene == gameObject.scene)
                store.SetTarget(new HudTarget(EntityId.ToULong(target.GetEntityId()), target.name,
                    Vector3.Distance(transform.position, target.transform.position), ship != null && ship.IsEnemy(target),
                    new HudDurability(target.Hull.CurrentHealth, target.Hull.MaxHealth)));
            else store.SetTarget(default);

            markers.Clear();
            foreach (var other in CombatShip.ActiveShips)
            {
                if (other == null || !other.Alive || other.gameObject.scene != gameObject.scene) continue;
                var kind = other == ship ? HudMarkerKind.Player
                    : ship != null && ship.IsEnemy(other) ? HudMarkerKind.Enemy
                    : ship != null && ship.Team != 0 && ship.Team == other.Team ? HudMarkerKind.Friendly : HudMarkerKind.Neutral;
                markers.Add(new HudMapMarker(EntityId.ToULong(other.GetEntityId()), other.name,
                    other.transform.position, other.transform.eulerAngles.y, kind));
            }
            markers.Sort((a, b) => a.Id.CompareTo(b.Id));
            store.SetMarkers(markers);
        }
    }
}
