using ThePirate.Combat;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace ThePirate.Ports
{
    [DisallowMultipleComponent]
    public sealed class PortMenuController : MonoBehaviour
    {
        private PortRepairService service;
        private PortMenuView view;
        private float nextRefresh;

        public static void Ensure(CombatShip ship)
        {
            if (ship == null || ship.Team != 1 || ship.GetComponent<ShipDocking>() == null) return;
            PortRepairService.Ensure(ship);
            if (ship.GetComponent<PortMenuController>() == null) ship.gameObject.AddComponent<PortMenuController>();
        }
        private void Start()
        {
            service = GetComponent<PortRepairService>();
            var prefab = Resources.Load<GameObject>("GameWork/PortMenu");
            var root = prefab != null ? Instantiate(prefab) : PortMenuLayout.Create();
            PortUpgradeLayout.Ensure(root);
            PortShipyardLayout.Ensure(root);
            PortShipBuildLayout.Ensure(root);
            root.name = "PortMenu";
            SceneManager.MoveGameObjectToScene(root, gameObject.scene);
            view = root.GetComponent<PortMenuView>();
            if (view == null) { Debug.LogError("Prefab PortMenu wymaga PortMenuView.", root); Destroy(root); return; }
            view.Bind(service);
            view.SetVisible(false);
            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                var events = new GameObject("Port EventSystem", typeof(EventSystem));
                events.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
                SceneManager.MoveGameObjectToScene(events, gameObject.scene);
            }
        }
        private void LateUpdate()
        {
            if (view == null || service == null) return;
            // Usługi nie zatrzymują symulacji; ponowne zagrożenie nadal zwalnia cumy.
            bool visible = service.Available(out _) && !global::Framework.Menu.PauseService.GameplayInputBlocked;
            bool opened = visible && !view.Visible;
            view.SetVisible(visible);
            if (visible && (opened || Time.unscaledTime >= nextRefresh))
            { nextRefresh = Time.unscaledTime + 0.15f; view.Refresh(); }
        }
        private void OnDisable() { if (view != null) view.SetVisible(false); }
        private void OnDestroy() { if (view != null) Destroy(view.gameObject); }
    }
}
