using UnityEngine;
using UnityEngine.InputSystem;

namespace ThePirate.Combat
{
    [DisallowMultipleComponent]
    public sealed class CannonPlayerInput : MonoBehaviour
    {
        private ShipCombatController combat;
        private SpecialWeaponController special;
        private void Awake() { combat = GetComponentInParent<ShipCombatController>(); special = GetComponentInParent<SpecialWeaponController>(); }
        private void Update()
        {
            var ship = GetComponentInParent<CombatShip>();
            if (ship != null && !ship.WeaponsAllowed) { special?.Cancel(); return; }
            if (!Application.isFocused || Framework.Menu.PauseService.GameplayInputBlocked) { if (special != null) special.Cancel(); return; }
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) { if (special != null) special.Cancel(); return; }
            if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
            Mouse mouse = Mouse.current;
            if (mouse == null || combat == null) return;
            combat.Aim(mouse.position.ReadValue());
            if (special != null && mouse.rightButton.wasPressedThisFrame) special.ToggleAim();
            if (special != null && special.Aiming) special.SetAim(combat.AimPoint, combat.HasAim);
            if (!mouse.leftButton.wasPressedThisFrame) return;
            if (special != null && special.Aiming) special.TryFire();
            else combat.Fire();
        }
    }
}
