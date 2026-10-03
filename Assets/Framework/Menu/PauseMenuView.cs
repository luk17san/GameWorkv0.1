using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Framework.Menu
{
    [DefaultExecutionOrder(-900)]
    public sealed class PauseMenuView : MonoBehaviour
    {
        [SerializeField] private GameFlow flow;
        [SerializeField] private PauseService pause;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject confirmationPanel;
        [SerializeField] private GameObject secondaryConfirmationPanel;
        [SerializeField] private Text confirmationText;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button cancelButton;
        private bool open;
        private bool quitRequested;

        private void Awake()
        {
            pausePanel.SetActive(false);
            confirmationPanel.SetActive(false);
        }

        private void Update()
        {
            if (!Application.isFocused || GameFlow.IsLoading || Keyboard.current == null
                || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

            if (secondaryConfirmationPanel != null && secondaryConfirmationPanel.activeSelf)
            {
                secondaryConfirmationPanel.SetActive(false);
                Select(resumeButton);
            }
            else if (confirmationPanel.activeSelf) CancelConfirmation();
            else if (open) Resume();
            else Open();
        }

        public void Open()
        {
            if (GameFlow.IsLoading || open) return;
            open = true;
            pause.SetPaused(true);
            pausePanel.SetActive(true);
            Select(resumeButton);
        }

        public void Resume()
        {
            if (GameFlow.IsLoading) return;
            open = false;
            confirmationPanel.SetActive(false);
            if (secondaryConfirmationPanel != null) secondaryConfirmationPanel.SetActive(false);
            pausePanel.SetActive(false);
            pause.SetPaused(false);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        public void RequestReturnToMenu() => Confirm(false);
        public void RequestQuit() => Confirm(true);

        private void Confirm(bool quit)
        {
            if (GameFlow.IsLoading) return;
            quitRequested = quit;
            confirmationText.text = quit
                ? "Wyjść z gry?\nPostęp tej rozgrywki zostanie utracony."
                : "Wrócić do menu?\nPostęp tej rozgrywki zostanie utracony.";
            pausePanel.SetActive(false);
            confirmationPanel.SetActive(true);
            Select(cancelButton);
        }

        public void AcceptConfirmation()
        {
            if (quitRequested) flow.QuitGame();
            else flow.ReturnToMenu();
        }

        public void CancelConfirmation()
        {
            if (GameFlow.IsLoading) return;
            confirmationPanel.SetActive(false);
            pausePanel.SetActive(true);
            Select(resumeButton);
        }

        private static void Select(Button button)
        {
            if (EventSystem.current != null && button != null)
                EventSystem.current.SetSelectedGameObject(button.gameObject);
        }

        private void OnDisable()
        {
            open = false;
            if (pausePanel != null) pausePanel.SetActive(false);
            if (confirmationPanel != null) confirmationPanel.SetActive(false);
            if (secondaryConfirmationPanel != null) secondaryConfirmationPanel.SetActive(false);
            if (pause != null) pause.SetPaused(false);
        }
    }
}
