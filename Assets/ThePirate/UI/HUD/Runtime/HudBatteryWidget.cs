using UnityEngine;
using UnityEngine.UI;

namespace ThePirate.UI.HUD
{
    public sealed class HudBatteryWidget : MonoBehaviour
    {
        public HudBatterySide side;
        [Tooltip("Indeks punktu w danym kierunku; umożliwia dodanie kilku niezależnych punktów na burcie.")]
        [Min(0)] public int mountIndex;
        public Text countdown;
        public HudShapeGraphic progress;
        public HudShapeGraphic selection;
        public GameObject readyCheck;
        public Color normalColor = new Color(0.2f,0.8f,0.7f);
        public Color damagedColor = new Color(0.85f,0.25f,0.2f);

        public void Render(HudBattery? value)
        {
            bool ready = value.HasValue && value.Value.Ready;
            if(readyCheck != null) readyCheck.SetActive(ready);
            if(selection != null) selection.gameObject.SetActive(value.HasValue && value.Value.Selected);
            if(countdown != null)
            {
                countdown.gameObject.SetActive(!ready);
                countdown.text = !value.HasValue ? "—" : value.Value.Destroyed ? "×"
                    : value.Value.ReloadRemaining > 0 ? PlayerHudView.Number(value.Value.ReloadRemaining)+" s" : "…";
            }
            if(progress != null)
            {
                progress.color = value.HasValue && value.Value.Destroyed ? damagedColor : normalColor;
                progress.SetFill(value.HasValue && !value.Value.Destroyed ? value.Value.ReloadProgress : 0);
            }
        }
    }
}
