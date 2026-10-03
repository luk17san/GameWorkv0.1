using ThePirate.Combat;
using ThePirate.Menu;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThePirate.UI.HUD
{
    public sealed class EnemyHudBootstrap : MonoBehaviour
    {
        private float nextScan;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Subscribe()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            SceneManager.sceneLoaded += SceneLoaded;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize() => Install(SceneManager.GetActiveScene());
        private static void SceneLoaded(Scene scene, LoadSceneMode mode) => Install(scene);
        private static void Install(Scene scene)
        {
            if (scene.path != PirateMenuBootstrap.GameplayScene) return;
            foreach (var root in scene.GetRootGameObjects())
                if (root.GetComponentInChildren<EnemyHudBootstrap>(true) != null) return;
            var obj = new GameObject("EnemyHudBootstrap");
            SceneManager.MoveGameObjectToScene(obj, scene);
            obj.AddComponent<EnemyHudBootstrap>();
        }
        private void Update()
        {
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + .5f;
            foreach (var ship in CombatShip.ActiveShips)
            {
                if (ship == null || ship.gameObject.scene != gameObject.scene || ship.Team == 0 || ship.Team == 1) continue;
                if (ship.GetComponent<EnemyHealthHud>() == null) ship.gameObject.AddComponent<EnemyHealthHud>();
            }
        }
    }
}
