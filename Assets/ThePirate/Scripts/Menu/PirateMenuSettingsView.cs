using Framework.Menu;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ThePirate.Menu
{
    public sealed class PirateMenuSettingsView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private CanvasGroup menuControls;
        [SerializeField] private Slider volume;
        [SerializeField] private Text volumeLabel;
        [SerializeField] private Text qualityLabel;
        [SerializeField] private Text soundLabel;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;

        private void Start()
        {
            volume.SetValueWithoutNotify(MenuPreferences.Volume);
            volume.onValueChanged.AddListener(ChangeVolume);
            RefreshLabels();
        }

        public void Open()
        {
            if (GameFlow.IsLoading) return;
            volume.SetValueWithoutNotify(MenuPreferences.Volume);
            RefreshLabels();
            panel.SetActive(true);
            menuControls.interactable = false;
            menuControls.blocksRaycasts = false;
            Select(closeButton);
        }

        public void Close()
        {
            panel.SetActive(false);
            menuControls.interactable = true;
            menuControls.blocksRaycasts = true;
            MenuPreferences.Flush();
            Select(openButton);
        }

        public void ToggleSound()
        {
            MenuPreferences.ToggleMute();
            RefreshLabels();
        }

        public void PreviousQuality() => ChangeQuality(-1);
        public void NextQuality() => ChangeQuality(1);

        private void ChangeQuality(int direction)
        {
            MenuPreferences.SetQuality(QualitySettings.GetQualityLevel() + direction);
            RefreshLabels();
        }

        private void ChangeVolume(float value)
        {
            MenuPreferences.SetVolume(value);
            RefreshLabels();
        }

        private void RefreshLabels()
        {
            if (volumeLabel != null)
                volumeLabel.text = "Głośność: " + Mathf.RoundToInt(MenuPreferences.Volume * 100f) + "%";
            string[] names = QualitySettings.names;
            if (qualityLabel != null) qualityLabel.text = names.Length == 0 ? "Domyślna"
                : names[Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, names.Length - 1)];
            if (soundLabel != null) soundLabel.text = MenuPreferences.Muted ? "DŹWIĘK: WYŁ." : "DŹWIĘK: WŁ.";
        }

        private void Update()
        {
            if (panel != null && panel.activeSelf && Keyboard.current != null
                && Keyboard.current.escapeKey.wasPressedThisFrame) Close();
        }

        private static void Select(Button button)
        {
            if (EventSystem.current != null && button != null)
                EventSystem.current.SetSelectedGameObject(button.gameObject);
        }

        private void OnDisable() => MenuPreferences.Flush();
        private void OnDestroy()
        {
            if (volume != null) volume.onValueChanged.RemoveListener(ChangeVolume);
        }
    }
}
