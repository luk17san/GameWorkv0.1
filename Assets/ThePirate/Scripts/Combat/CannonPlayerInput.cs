using UnityEngine;
using UnityEngine.InputSystem;

namespace ThePirate.Combat
{
    [RequireComponent(typeof(Cannon))]
    [DisallowMultipleComponent]
    public sealed class CannonPlayerInput : MonoBehaviour
    {
        private Cannon cannon;

        private void Awake()
        {
            cannon = GetComponent<Cannon>();
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (Application.isFocused && mouse != null && mouse.leftButton.wasPressedThisFrame)
                cannon.TryFire();
        }
    }
}
