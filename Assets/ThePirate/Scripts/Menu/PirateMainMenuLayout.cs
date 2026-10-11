using UnityEngine;
using UnityEngine.UI;

namespace ThePirate.Menu
{
    [ExecuteAlways]
    public sealed class PirateMainMenuLayout : MonoBehaviour
    {
        [SerializeField] private RectTransform safeArea;
        [SerializeField] private RectTransform column;
        [SerializeField] private RectTransform logo;
        [SerializeField] private RectTransform[] buttons;
        [SerializeField] private RectTransform feedback;
        [SerializeField] private RectTransform settingsCard;
        [SerializeField] private GameObject desktopExit;
        private Rect lastSafe;
        private Vector2 lastSize;
        private int lastWidth;
        private int lastHeight;

        private void OnEnable()
        {
            if (Application.isPlaying && desktopExit != null)
                desktopExit.SetActive(Application.platform != RuntimePlatform.Android
                    && Application.platform != RuntimePlatform.IPhonePlayer);
            Refresh(true);
        }
        private void LateUpdate() => Refresh(false);

        public void Refresh(bool force)
        {
            if (safeArea == null || column == null || logo == null || buttons == null) return;
            int width = Mathf.Max(1, Screen.width);
            int height = Mathf.Max(1, Screen.height);
            Rect area = Screen.safeArea;
            if (area.width <= 0 || area.height <= 0) area = new Rect(0, 0, width, height);
            if (force || area != lastSafe || width != lastWidth || height != lastHeight)
            {
                safeArea.anchorMin = new Vector2(area.xMin / width, area.yMin / height);
                safeArea.anchorMax = new Vector2(area.xMax / width, area.yMax / height);
                safeArea.offsetMin = safeArea.offsetMax = Vector2.zero;
                lastSafe = area;
                lastWidth = width;
                lastHeight = height;
            }
            Vector2 size = safeArea.rect.size;
            if (!force && size == lastSize) return;
            if (size.x < 1 || size.y < 1) return;
            lastSize = size;
            // Reserve space for a two-line save/load error below all four buttons.
            float w = Mathf.Max(1f, Mathf.Min(610f, size.x * 0.40f, (size.y - 130f) / 1.409f));
            float inset = Mathf.Clamp(size.x * 0.025f, 18f, 40f);
            column.anchorMin = new Vector2(0, 0);
            column.anchorMax = new Vector2(0, 1);
            column.pivot = new Vector2(0, 0.5f);
            column.sizeDelta = new Vector2(w, -40);
            column.anchoredPosition = new Vector2(inset, 0);
            float logoHeight = w * 0.46f;
            Place(logo, w, logoHeight, -logoHeight * 0.5f);
            float buttonHeight = w * 0.213f;
            float gap = w * 0.018f;
            float y = -logoHeight - w * 0.025f;
            foreach (RectTransform button in buttons)
            {
                if (button == null || !button.gameObject.activeSelf) continue;
                Place(button, w, buttonHeight, y - buttonHeight * 0.5f);
                y -= buttonHeight + gap;
            }
            if (feedback != null) Place(feedback, w, 76f, y - 40f);
            if (settingsCard != null)
            {
                settingsCard.localScale = Vector3.one * Mathf.Min(1f,
                    (size.x - 36f) / 720f, (size.y - 36f) / 520f);
            }
        }

        private static void Place(RectTransform item, float width, float height, float y)
        {
            item.anchorMin = item.anchorMax = new Vector2(0.5f, 1);
            item.pivot = new Vector2(0.5f, 0.5f);
            item.sizeDelta = new Vector2(width, height);
            item.anchoredPosition = new Vector2(0, y);
        }
    }
}
