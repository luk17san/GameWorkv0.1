using UnityEngine;

namespace Framework.Menu
{
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public sealed class PauseService : MonoBehaviour
    {
        private static PauseService owner;
        private static int blockedThroughFrame = -1;
        private float previousTimeScale;
        private bool previousAudioPause;
        private bool previousCursorVisible;
        private CursorLockMode previousCursorLock;

        public static bool IsPaused => owner != null;
        public static bool GameplayInputBlocked => IsPaused || GameFlow.IsLoading
            || Time.frameCount <= blockedThroughFrame || Time.timeScale <= 0f
            || !Application.isFocused;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            owner = null;
            blockedThroughFrame = -1;
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }

        public void SetPaused(bool paused)
        {
            if (paused)
            {
                if (owner != null) return;
                owner = this;
                previousTimeScale = Time.timeScale;
                previousAudioPause = AudioListener.pause;
                previousCursorVisible = Cursor.visible;
                previousCursorLock = Cursor.lockState;
                Time.timeScale = 0f;
                AudioListener.pause = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                if (owner != this) return;
                Time.timeScale = previousTimeScale;
                AudioListener.pause = previousAudioPause;
                Cursor.lockState = previousCursorLock;
                Cursor.visible = previousCursorVisible;
                owner = null;
            }

            // Kliknięcie Wznów nie może stać się strzałem w tej samej klatce.
            blockedThroughFrame = Time.frameCount;
        }

        private void OnDisable() => SetPaused(false);
    }
}
