using UnityEngine;
using UnityEngine.InputSystem;

namespace GameWork.Framework.Ships.Movement
{
    public sealed class ShipMovementInput : MonoBehaviour
    {
        [SerializeField] private InputActionReference movementAction;

        private bool enabledByThisComponent;

        public Vector2 ReadMovement()
        {
            return global::Framework.Menu.PauseService.GameplayInputBlocked
                || !isActiveAndEnabled || movementAction == null || movementAction.action == null
                ? Vector2.zero
                : Vector2.ClampMagnitude(movementAction.action.ReadValue<Vector2>(), 1f);
        }

        private void OnEnable()
        {
            if (movementAction == null ||
                movementAction.action == null ||
                movementAction.action.enabled)
            {
                return;
            }

            movementAction.action.Enable();
            enabledByThisComponent = true;
        }

        private void OnDisable()
        {
            if (!enabledByThisComponent ||
                movementAction == null ||
                movementAction.action == null)
            {
                return;
            }

            movementAction.action.Disable();
            enabledByThisComponent = false;
        }
    }
}
