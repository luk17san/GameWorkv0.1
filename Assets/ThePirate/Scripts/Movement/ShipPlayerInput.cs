using UnityEngine;
using UnityEngine.InputSystem;

namespace ThePirate.Ships
{
    // Ten komponent zna klawiaturę. ShipMovement otrzymuje tylko liczby.
    [RequireComponent(typeof(ShipMovement))]
    [DisallowMultipleComponent]
    public sealed class ShipPlayerInput : MonoBehaviour
    {
        private ShipMovement movement;

        private void Awake()
        {
            movement = GetComponent<ShipMovement>();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !Application.isFocused)
            {
                movement.SetInput(0f, 0f);
                return;
            }

            float throttle = (keyboard.wKey.isPressed ? 1f : 0f)
                - (keyboard.sKey.isPressed ? 1f : 0f);
            float steering = (keyboard.dKey.isPressed ? 1f : 0f)
                - (keyboard.aKey.isPressed ? 1f : 0f);

            movement.SetInput(throttle, steering);
        }

        private void OnDisable()
        {
            if (movement != null)
                movement.SetInput(0f, 0f);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && movement != null)
                movement.SetInput(0f, 0f);
        }
    }
}
