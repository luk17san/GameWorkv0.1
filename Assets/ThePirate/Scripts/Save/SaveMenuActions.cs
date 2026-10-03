using Framework.Menu;
using Framework.Save;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ThePirate.Save
{
    [RequireComponent(typeof(GameFlow))]
    public sealed class SaveMenuActions : MonoBehaviour
    {
        [SerializeField] private Text feedback;
        [SerializeField] private Button loadButton;
        [SerializeField] private GameObject loadConfirmationPanel;
        [SerializeField] private Button cancelLoadButton;
        private GameFlow flow;

        private void Awake() => flow = GetComponent<GameFlow>();

        private void Start()
        {
            RefreshAvailability();
            if (!string.IsNullOrEmpty(VoyageSaveRuntime.LastLoadError) && feedback != null)
                feedback.text = VoyageSaveRuntime.LastLoadError;
            else if (SingleSlotSaveStore.HasAnyFile && !VoyageSaveRuntime.HasValidSave && feedback != null)
                feedback.text = "Zapis jest uszkodzony lub niezgodny z tą wersją gry.";
        }

        public void SaveGame()
        {
            VoyageSaveRuntime.TrySave(out string message);
            Report(message);
            RefreshAvailability();
        }

        public void LoadGame()
        {
            CancelLoad();
            VoyageSaveRuntime.TryLoad(flow.LoadSavedGame, out string message);
            Report(message);
        }

        public void RequestLoad()
        {
            if (loadConfirmationPanel == null) { LoadGame(); return; }
            loadConfirmationPanel.SetActive(true);
            if (EventSystem.current != null && cancelLoadButton != null)
                EventSystem.current.SetSelectedGameObject(cancelLoadButton.gameObject);
        }

        public void CancelLoad()
        {
            if (loadConfirmationPanel != null) loadConfirmationPanel.SetActive(false);
        }

        private void RefreshAvailability()
        {
            if (loadButton != null) loadButton.interactable = VoyageSaveRuntime.HasValidSave;
        }

        private void Report(string message)
        {
            if (feedback != null) feedback.text = message;
            else Debug.LogWarning(message, this);
        }
    }
}
