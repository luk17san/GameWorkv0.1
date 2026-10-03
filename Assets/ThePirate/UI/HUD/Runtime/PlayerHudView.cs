using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace ThePirate.UI.HUD
{
    public sealed class PlayerHudView : MonoBehaviour
    {
        [Header("Źródło danych; bootstrap przypisuje gracza")]
        public PlayerHudController source;
        [Header("Wytrzymałość")]
        public Text durabilityValue;
        public HudShapeGraphic durabilityFill;
        public Image avatar;
        [Header("Ruch i ładownia")]
        public Text speedValue;
        public Text driveValue;
        public string propulsionName = "ŻAGLE";
        public Text cargoValue;
        public HudShapeGraphic cargoFill;
        [Header("Punkty montażowe")]
        public HudBatteryWidget[] batteries;
        [Header("Minimapa i wiatr")]
        public HudMinimapView minimap;
        public Text windValue;
        public RectTransform windArrow;
        [Header("Zadanie i cel")]
        public Text objectiveValue;
        public GameObject targetPanel;
        public Text targetName, targetValue, targetDistance;
        public HudShapeGraphic targetFill;
        [Header("Komunikaty")]
        public GameObject messagePanel;
        public Text messageValue;
        private PlayerHudStateStore store;
        private float messageUntil;
        private static readonly CultureInfo Polish = CultureInfo.GetCultureInfo("pl-PL");
        public static string Number(float value) => value.ToString("0.0",Polish);

        public void Bind(PlayerHudController controller)
        {
            Unsubscribe(); source=controller;
            if(!isActiveAndEnabled) return;
            if(source==null) { Clear(); return; }
            store=source.State;
            store.DurabilityChanged+=RenderDurability; store.MovementChanged+=RenderMovement;
            store.CargoChanged+=RenderCargo; store.BatteriesChanged+=RenderBatteries;
            store.WindChanged+=RenderWind; store.ObjectiveChanged+=RenderObjective;
            store.TargetChanged+=RenderTarget; store.MarkersChanged+=RenderMarkers;
            store.MessageRaised+=ShowMessage;
            if(minimap!=null) minimap.SetPlayer(source.transform);
            RenderDurability(store.Durability); RenderMovement(store.Movement); RenderCargo(store.Cargo);
            RenderBatteries(store.Batteries); RenderWind(store.Wind); RenderObjective(store.Objective);
            RenderTarget(store.Target); RenderMarkers(store.Markers);
        }

        private void OnEnable() { if(Application.isPlaying) Bind(source); }
        private void OnDisable() => Unsubscribe();
        private void Update()
        {
            if(!Application.isPlaying) return;
            if(source == null && store != null) { Unsubscribe(); Clear(); }
            else if(source != null && store != source.State) Bind(source);
            if(messagePanel!=null && messagePanel.activeSelf && Time.unscaledTime>=messageUntil) messagePanel.SetActive(false);
        }
        private void Unsubscribe()
        {
            if(store==null) return;
            store.DurabilityChanged-=RenderDurability; store.MovementChanged-=RenderMovement;
            store.CargoChanged-=RenderCargo; store.BatteriesChanged-=RenderBatteries;
            store.WindChanged-=RenderWind; store.ObjectiveChanged-=RenderObjective;
            store.TargetChanged-=RenderTarget; store.MarkersChanged-=RenderMarkers;
            store.MessageRaised-=ShowMessage; store=null;
        }
        private void Clear()
        {
            RenderDurability(default); RenderMovement(default); RenderCargo(default); RenderWind(default);
            RenderObjective(default); RenderTarget(default); RenderBatteries(new HudBattery[0]); RenderMarkers(new HudMapMarker[0]);
        }

        public void RenderDurability(HudDurability value)
        {
            if(durabilityValue!=null) durabilityValue.text=value.Available ? value.Current+" / "+value.Maximum : "— / —";
            if(durabilityFill!=null) durabilityFill.SetFill(value.Fraction);
        }
        public void RenderMovement(HudMovement value)
        {
            if(speedValue!=null) speedValue.text=value.Available ? Number(value.Speed) : "—";
            if(driveValue!=null) driveValue.text="NAPĘD: "+propulsionName;
        }
        public void RenderCargo(HudCargo value)
        {
            if(cargoValue!=null) cargoValue.text=!value.Available ? "— / —" : value.Weight.ToString("0.#",Polish)+" / "+(value.HasCapacity ? value.Capacity.ToString("0.#",Polish) : "—");
            if(cargoFill!=null) cargoFill.SetFill(value.HasCapacity && value.Capacity>0 ? value.Weight/value.Capacity : 0);
        }
        public void RenderBatteries(IReadOnlyList<HudBattery> values)
        {
            if(batteries==null) return;
            foreach(var widget in batteries)
            {
                if(widget==null) continue;
                HudBattery? found=null; int index=0;
                foreach(var value in values) if(value.Side==widget.side && index++==widget.mountIndex) { found=value; break; }
                widget.Render(found);
            }
        }
        public void RenderWind(HudWind value)
        {
            if(windValue!=null) windValue.text=value.Available ? Number(new Vector2(value.Velocity.x,value.Velocity.z).magnitude) : "—";
            if(windArrow!=null)
            {
                windArrow.gameObject.SetActive(value.Available && value.Velocity.sqrMagnitude>0.001f);
                windArrow.localRotation=Quaternion.Euler(0,0,-Mathf.Atan2(value.Velocity.x,value.Velocity.z)*Mathf.Rad2Deg);
            }
        }
        public void RenderObjective(HudObjective value)
        { if(objectiveValue!=null) objectiveValue.text=value.Available ? value.Text+(value.Completed ? " • ukończone" : "") : "Brak aktywnego zadania"; }
        public void RenderTarget(HudTarget value)
        {
            if(targetPanel!=null) targetPanel.SetActive(value.Available);
            if(targetName!=null) targetName.text=value.Name ?? "";
            if(targetValue!=null) targetValue.text=value.Durability.Current+" / "+value.Durability.Maximum;
            if(targetDistance!=null) targetDistance.text=value.Distance.ToString("0",Polish)+" m";
            if(targetFill!=null) targetFill.SetFill(value.Durability.Fraction);
        }
        private void RenderMarkers(IReadOnlyList<HudMapMarker> values) { if(minimap!=null) minimap.Render(values); }
        private void ShowMessage(HudMessage value)
        {
            if(messageValue!=null) messageValue.text=value.Text;
            if(messagePanel!=null) messagePanel.SetActive(true);
            messageUntil=Time.unscaledTime+value.Duration;
        }

        // Wyłącznie do podglądu zasobów w edytorze; nigdy nie jest wywoływane w rozgrywce.
        public void ApplyPreviewValues()
        {
            RenderDurability(new HudDurability(420,500));
            RenderMovement(new HudMovement(8.4f,8.4f,HudDriveMode.Forward));
            RenderCargo(new HudCargo(320,1000,true));
            RenderWind(new HudWind(new Vector3(4.384f,0,4.384f)));
            RenderObjective(new HudObjective("Dopłyń do portu",false));
            RenderTarget(new HudTarget(1,"WROGI STATEK",65,true,new HudDurability(280,400)));
            RenderBatteries(new [] { new HudBattery(1,HudBatterySide.Bow,1,false,false,false,1.2f,6),
                new HudBattery(2,HudBatterySide.Left,4,false,false,false,2.4f,6),
                new HudBattery(3,HudBatterySide.Right,4,true,false,false,0,6),
                new HudBattery(4,HudBatterySide.Stern,1,true,false,false,0,6) });
        }
    }
}
