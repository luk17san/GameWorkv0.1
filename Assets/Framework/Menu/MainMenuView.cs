using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework.Menu
{
    public sealed class MainMenuView : MonoBehaviour
    {
        [SerializeField] private GameFlow flow;
        [SerializeField] private Button firstButton;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (EventSystem.current != null && firstButton != null)
                EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
        }

        public void NewGame() => flow.NewGame();
        public void QuitGame() => flow.QuitGame();
    }
}
