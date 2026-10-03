using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace ThePirate.Menu
{
    public static class PirateMenuBootstrap
    {
        public const string GameplayScene = "Assets/ThePirate/Scenes/ShipSandbox.unity";
        public const string MainMenuScene = "Assets/ThePirate/Scenes/MainMenu.unity";
        public const string PausePrefabResource = "GameWork/PauseMenu";

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
            if (scene.path != GameplayScene) return;
            // Kontrola w konkretnej scenie zapobiega duplikatom, także bez Scene Reload.
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.GetComponentInChildren<Framework.Menu.PauseMenuView>(true) != null) return;

            GameObject prefab = Resources.Load<GameObject>(PausePrefabResource);
            if (prefab == null)
            {
                Debug.LogError("Brak prefabu menu. Uruchom GameWork > Menu > Utwórz menu w edytorze.");
                return;
            }

            GameObject instance = Object.Instantiate(prefab);
            instance.name = "PauseMenu";
            SceneManager.MoveGameObjectToScene(instance, scene);
            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject events = new GameObject("Menu EventSystem", typeof(EventSystem));
                events.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
                SceneManager.MoveGameObjectToScene(events, scene);
            }
        }
    }
}
