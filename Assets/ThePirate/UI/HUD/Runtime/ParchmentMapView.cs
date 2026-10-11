using Framework.Menu;
using ThePirate.Combat;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ThePirate.UI.HUD
{
    [DefaultExecutionOrder(-950)]
    public sealed class ParchmentMapView : MonoBehaviour
    {
        public Transform player;
        public GameObject panel;
        public RectTransform parchment;
        public RectTransform marker;
        public Text status;
        public Vector2 worldMinimum = new Vector2(-1500, -1000);
        public Vector2 worldSize = new Vector2(3000, 2000);
        [Tooltip("Obszar terenu na oryginalnym obrazie, piksele od lewego dolnego rogu.")]
        public Rect imageWorldRect = new Rect(40, 54, 1455, 930);
        public Vector2 imageSize = new Vector2(1536, 1024);
        public bool IsOpen => panel != null && panel.activeSelf;

        void Awake() { if (panel != null) panel.SetActive(false); }
        void Update()
        {
            if (!Application.isFocused || GameFlow.IsLoading) { Close(); return; }
            if (player == null || !player.gameObject.activeInHierarchy)
            {
                player = null;
                foreach (var ship in CombatShip.ActiveShips)
                    if (ship != null && ship.Team == 1 && ship.isActiveAndEnabled && ship.gameObject.scene == gameObject.scene)
                    { player = ship.transform; break; }
            }
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.mKey.wasPressedThisFrame && !PauseService.IsPaused)
                    panel.SetActive(!IsOpen);
                else if (IsOpen && keyboard.escapeKey.wasPressedThisFrame) Close();
            }
            if (IsOpen) PauseService.SuppressGameplayInputThisFrame();
        }
        void LateUpdate()
        {
            if (!IsOpen || parchment == null || marker == null) return;
            bool available = player != null && player.gameObject.activeInHierarchy;
            Vector2 uv = available ? WorldToImage(player.position) : Vector2.zero;
            bool inside = available && player.position.x >= worldMinimum.x && player.position.x <= worldMinimum.x + worldSize.x
                && player.position.z >= worldMinimum.y && player.position.z <= worldMinimum.y + worldSize.y;
            marker.gameObject.SetActive(inside);
            if (inside)
            {
                marker.anchorMin = marker.anchorMax = uv;
                marker.anchoredPosition = Vector2.zero;
                marker.localRotation = Quaternion.Euler(0, 0, -player.eulerAngles.y);
            }
            if (status != null) status.text = !available ? "Brak aktywnego statku" : !inside ? "Statek poza obszarem mapy"
                : "Twoja pozycja • M / Esc — zamknij mapę";
        }
        public Vector2 WorldToImage(Vector3 position)
        {
            float x = (position.x - worldMinimum.x) / Mathf.Max(.001f, worldSize.x);
            float z = (position.z - worldMinimum.y) / Mathf.Max(.001f, worldSize.y);
            return new Vector2((imageWorldRect.x + x * imageWorldRect.width) / imageSize.x,
                (imageWorldRect.y + z * imageWorldRect.height) / imageSize.y);
        }
        public void Close()
        {
            if (!IsOpen) return;
            panel.SetActive(false);
            PauseService.SuppressGameplayInputThisFrame();
        }
    }
}
