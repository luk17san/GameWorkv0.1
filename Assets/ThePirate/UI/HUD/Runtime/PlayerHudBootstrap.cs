using ThePirate.Combat;
using ThePirate.Menu;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThePirate.UI.HUD
{
    public static class PlayerHudBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Subscribe()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            SceneManager.sceneLoaded += SceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeOpenScene() => Install(SceneManager.GetActiveScene());
        private static void SceneLoaded(Scene scene, LoadSceneMode mode) => Install(scene);

        private static void Install(Scene scene)
        {
            if (scene.path != PirateMenuBootstrap.GameplayScene) return;
            CombatShip player = null;
            foreach (var root in scene.GetRootGameObjects())
                foreach (var candidate in root.GetComponentsInChildren<CombatShip>())
                {
                    if (candidate.Team != 1 || !candidate.isActiveAndEnabled) continue;
                    if (player != null)
                    {
                        Debug.LogWarning("HUD: więcej niż jeden aktywny statek drużyny 1. Przypisz PlayerHudController ręcznie.");
                        return;
                    }
                    player = candidate;
                }
            if (player != null && player.GetComponent<PlayerHudController>() == null)
                player.gameObject.AddComponent<PlayerHudController>();
            if (player == null) return;
            PlayerHudView view = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                view = root.GetComponentInChildren<PlayerHudView>(true);
                if (view != null) break;
            }
            if (view == null)
            {
                var prefab = Resources.Load<GameObject>("GameWork/PlayerHud");
                if (prefab == null) return;
                var instance = Object.Instantiate(prefab);
                instance.name = "PlayerHUD";
                SceneManager.MoveGameObjectToScene(instance, scene);
                view = instance.GetComponent<PlayerHudView>();
            }
            if (view != null) view.Bind(player.GetComponent<PlayerHudController>());
        }
    }
}
