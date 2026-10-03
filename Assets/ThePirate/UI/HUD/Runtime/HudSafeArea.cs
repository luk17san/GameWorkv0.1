using UnityEngine;

namespace ThePirate.UI.HUD
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class HudSafeArea : MonoBehaviour
    {
        private Rect previous;
        private Vector2 resolution;
        private void OnEnable() => Apply();
        private void Update()
        { if (previous != Screen.safeArea || resolution != new Vector2(Screen.width, Screen.height)) Apply(); }
        private void Apply()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;
            previous = Screen.safeArea; resolution = new Vector2(Screen.width, Screen.height);
            var rect = (RectTransform)transform;
            rect.anchorMin = previous.position / resolution;
            rect.anchorMax = (previous.position + previous.size) / resolution;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
