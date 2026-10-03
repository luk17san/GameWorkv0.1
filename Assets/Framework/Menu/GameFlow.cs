using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Framework.Menu
{
    [RequireComponent(typeof(PauseService))]
    [DisallowMultipleComponent]
    public sealed class GameFlow : MonoBehaviour
    {
        [SerializeField] private string mainMenuScene;
        [SerializeField] private string gameplayScene;
        [SerializeField] private Text feedback;
        [SerializeField] private CanvasGroup controls;
        private static GameFlow loadingOwner;

        public static bool IsLoading => loadingOwner != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => loadingOwner = null;

        public void NewGame() => LoadScene(gameplayScene);
        public void ReturnToMenu() => LoadScene(mainMenuScene);
        public bool LoadSavedGame() => LoadScene(gameplayScene);

        private bool LoadScene(string scenePath)
        {
            if (IsLoading) return false;
            if (string.IsNullOrWhiteSpace(scenePath) || !Application.CanStreamedLevelBeLoaded(scenePath))
            {
                Report("Nie można otworzyć sceny. Sprawdź listę scen w Build Profiles.");
                return false;
            }

            bool wasPaused = PauseService.IsPaused;
            loadingOwner = this;
            if (controls != null) controls.interactable = false;
            Report("Ładowanie…");
            GetComponent<PauseService>().SetPaused(true);
            try
            {
                // Single usuwa poprzednią rozgrywkę; Nowa gra zawsze zaczyna od stanu sceny.
                if (SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single) == null)
                    throw new InvalidOperationException("Unity nie rozpoczęło ładowania sceny.");
                return true;
            }
            catch (Exception exception)
            {
                loadingOwner = null;
                if (controls != null) controls.interactable = true;
                if (!wasPaused) GetComponent<PauseService>().SetPaused(false);
                Report("Nie udało się otworzyć sceny. Spróbuj ponownie.");
                Debug.LogException(exception, this);
                return false;
            }
        }

        public void QuitGame()
        {
            if (IsLoading) return;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void Report(string message)
        {
            if (feedback != null) feedback.text = message;
            else Debug.LogWarning(message, this);
        }

        private void OnDestroy()
        {
            if (loadingOwner == this) loadingOwner = null;
        }
    }
}
