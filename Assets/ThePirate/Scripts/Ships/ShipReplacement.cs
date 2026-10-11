using ThePirate.Cameras;
using ThePirate.Combat;
using ThePirate.Ports;
using ThePirate.UI.HUD;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThePirate.Ships
{
    public static class ShipReplacement
    {
        // Awake nowego statku wykonujemy w tej samej operacji, potem wyłączamy go do czasu zatwierdzenia.
        public static bool TryPrepare(ShipDefinition definition, Scene scene, Vector3 position, Quaternion rotation,
            out CombatShip prepared, out string error)
        {
            prepared = null; error = null;
            if (definition == null || !definition.Validate(out error)) return false;
            GameObject root = null;
            try
            {
                root = Object.Instantiate(definition.Prefab, position, rotation);
                SceneManager.MoveGameObjectToScene(root, scene);
                prepared = root.GetComponent<CombatShip>();
                ShipIdentity.Ensure(prepared).Configure(definition.Id);
                root.SetActive(false);
                if (prepared.Hull == null || prepared.Hull.BaseMaxHealth != definition.HullHealth)
                    throw new System.InvalidOperationException("Prefab nie zainicjalizował kadłuba.");
                return true;
            }
            catch (System.Exception exception)
            {
                if (root != null) { root.SetActive(false); Object.Destroy(root); }
                prepared = null; error = "Nie udało się przygotować statku: " + exception.Message; return false;
            }
        }
        public static void Discard(CombatShip ship)
        { if (ship != null) { ship.gameObject.SetActive(false); Object.Destroy(ship.gameObject); } }

        public static bool TryCommit(CombatShip previous, CombatShip prepared, PortDock port, out string error)
        {
            error = null;
            if (previous == null || prepared == null || previous == prepared) { error = "Nieprawidłowa wymiana statku."; return false; }
            var oldDocking = previous.GetComponent<ShipDocking>();
            PortDock previousPort = oldDocking != null && oldDocking.IsDocked ? PortDock.Find(previous.gameObject.scene, oldDocking.PortId) : null;
            bool bindingsChanged = false;
            try
            {
                oldDocking?.Undock();
                previous.gameObject.SetActive(false);
                prepared.gameObject.SetActive(true);
                if (port != null && !prepared.GetComponent<ShipDocking>().RestoreDock(port))
                    throw new System.InvalidOperationException("Port nie przyjął nowego statku.");
                Framework.Menu.PauseService.SuppressGameplayInputThisFrame();
                bindingsChanged = true;
                BindPlayer(previous, prepared);
            }
            catch (System.Exception exception)
            {
                prepared.GetComponent<ShipDocking>()?.Undock();
                prepared.gameObject.SetActive(false); previous.gameObject.SetActive(true);
                if (previousPort != null) oldDocking.RestoreDock(previousPort);
                if (bindingsChanged)
                {
                    try { BindPlayer(prepared, previous); }
                    catch (System.Exception rollbackError) { Debug.LogError("Nie udało się ponownie przypiąć widoku: " + rollbackError.Message); }
                }
                error = "Wymiana przerwana. Zachowano poprzedni statek: " + exception.Message;
                return false;
            }
            Object.Destroy(previous.gameObject);
            return true;
        }
        private static void BindPlayer(CombatShip previous, CombatShip current)
        {
            foreach (var root in current.gameObject.scene.GetRootGameObjects())
                foreach (var camera in root.GetComponentsInChildren<TopDownCamera>(true))
                    camera.RetargetIfFollowing(previous.transform, current.transform);
            PlayerHudBootstrap.Install(current.gameObject.scene);
        }
    }
}
