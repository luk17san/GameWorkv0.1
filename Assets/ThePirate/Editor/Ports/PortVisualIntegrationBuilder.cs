using System;
using System.IO;
using System.Linq;
using ThePirate.Ports;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ThePirate.EditorTools
{
    public static class PortVisualIntegrationBuilder
    {
        private const string Target = "Assets/ThePirate/Resources/GameWork/PortMenu.prefab";
        private const string Kit = "Assets/ThePirate/UI/PortVisualKit/Generated";
        [InitializeOnLoadMethod]
        private static void Schedule() => EditorApplication.delayCall += Ensure;
        private static void Ensure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            { EditorApplication.delayCall += Ensure; return; }
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(Target);
            if (existing != null && existing.GetComponent<PortMenuView>()?.HasVisual == false) Integrate();
        }
        [MenuItem("Tools/GameWork/Ports/Visual kit/Connect port to game logic")]
        public static void Integrate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(Target);
            if (existing == null) { Debug.LogError("Najpierw utwórz istniejące menu portu: " + Target); return; }
            if (existing.GetComponent<PortMenuView>()?.HasVisual == true)
            { Selection.activeObject = existing; return; }
            if (!AssetDatabase.IsValidFolder(Kit)) { Debug.LogError("Najpierw wygeneruj PortVisualCanvas."); return; }
            var sourcePath = AssetDatabase.FindAssets("PortVisualCanvas t:Prefab", new[] { Kit })
                .Select(AssetDatabase.GUIDToAssetPath).OrderByDescending(path => path, StringComparer.Ordinal).FirstOrDefault();
            var source = sourcePath != null ? AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath) : null;
            if (source == null) { Debug.LogError("Brak wygenerowanego PortVisualCanvas."); return; }
            var root = PrefabUtility.LoadPrefabContents(Target);
            try
            {
                PortUpgradeLayout.Ensure(root); PortShipyardLayout.Ensure(root); PortShipBuildLayout.Ensure(root);
                var view = root.GetComponent<PortMenuView>();
                var window = root.transform.Find("Window");
                var card = root.transform.Find("Window/PortPanel") as RectTransform;
                if (view == null || window == null || card == null || !view.HasUpgradePanel || !view.HasShipyardPanel)
                    throw new InvalidOperationException("Menu wymaga poprawnych powiązań usług przed integracją.");
                if (window.Find("VisualShell") != null) throw new InvalidOperationException("VisualShell istnieje bez referencji. Sprawdź prefab.");

                var clone = Object.Instantiate(source);
                var shell = clone.transform.Find("Window");
                if (shell == null) { Object.DestroyImmediate(clone); throw new InvalidOperationException("PortVisualCanvas nie zawiera Window."); }
                shell.SetParent(window, false); shell.name = "VisualShell";
                Object.DestroyImmediate(clone);
                window.Find("Shade")?.gameObject.SetActive(false);
                var blocker = window.GetComponent<Image>() ?? window.gameObject.AddComponent<Image>();
                blocker.color = Color.clear; blocker.raycastTarget = true;
                var scaler = root.GetComponent<CanvasScaler>();
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

                card.SetAsLastSibling();
                card.anchorMin = card.anchorMax = card.pivot = new Vector2(.5f, .5f);
                card.anchoredPosition = new Vector2(-99, -48); card.sizeDelta = new Vector2(952, 694);
                var navy = Required<Image>(shell, "Navigation/NavEquipment").sprite;
                var red = Required<Image>(shell, "Navigation/NavPort").sprite;
                var cardImage = card.GetComponent<Image>();
                cardImage.sprite = navy; cardImage.type = Image.Type.Sliced;
                cardImage.pixelsPerUnitMultiplier = 6; cardImage.color = Color.white; cardImage.raycastTarget = true;
                foreach (var name in new[] { "PortName", "Gold", "RepairTab", "CargoTab", "UpgradeTab", "ShipyardTab", "Feedback", "Undock" })
                    card.Find(name)?.gameObject.SetActive(false);
                foreach (var name in new[] { "RepairPanel", "CargoPanel", "UpgradePanel", "ShipyardPanel" })
                {
                    var panel = card.Find(name) as RectTransform;
                    if (panel != null) PortMenuLayout.Place(panel, 45, 92, 862, 530);
                }
                var heading = PortMenuLayout.Label("ServicesHeading", card, 45, 28, 862, 44, 28);
                heading.text = "USŁUGI PORTOWE";
                foreach (var text in card.GetComponentsInChildren<Text>(true))
                {
                    text.color = new Color(.96f, .89f, .72f);
                    if (text.transform.parent.GetComponent<Button>() != null)
                    { text.resizeTextForBestFit = true; text.resizeTextMinSize = 13; text.resizeTextMaxSize = 22; }
                }
                foreach (var button in card.GetComponentsInChildren<Button>(true))
                {
                    var image = button.GetComponent<Image>();
                    if (image != null) { image.sprite = navy; image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = 6; image.color = Color.white; }
                }

                foreach (var name in new[] { "NavFleet", "NavQuests", "NavTrade", "NavWarehouse", "NavTavern" })
                {
                    var button = Required<Button>(shell, "Navigation/" + name);
                    button.interactable = false;
                    button.GetComponent<Image>().color = new Color(.55f, .55f, .55f, 1);
                    Required<Text>(button.transform, "Label").color = new Color(.60f, .56f, .47f);
                }
                Required<Text>(shell, "ShipSummary/Crew/Value").text = "—";
                Required<Text>(shell, "ShipSummary/RepairButton/Label").text = "NAPRAWY";
                var quest = shell.Find("Footer/ActiveQuest");
                foreach (var name in new[] { "ResourceIcon", "Progress", "ProgressValue", "Description", "QuestsButton" })
                    quest.Find(name)?.gameObject.SetActive(false);
                Required<Text>(quest, "Title").text = "USŁUGI PORTOWE";
                var messages = Required<Text>(quest, "QuestName");
                PortMenuLayout.Place(messages.rectTransform, 68, 31, 1165, 44);
                messages.fontSize = 18; messages.alignment = TextAnchor.MiddleLeft;
                messages.text = "Wybierz usługę. E: odcumuj • Esc: pauza.";

                var bridge = root.AddComponent<PortVisualBridge>(); bridge.Configure(card.gameObject, shell, navy, red);
                view.ConfigureVisual(bridge, Required<Text>(shell, "Header/PortName"), messages);
                bridge.SetSection(0);
                var serialized = new SerializedObject(bridge);
                foreach (var field in new[] { "servicesCard", "panelTitle", "goldValue", "shipValue", "shipName", "hullValue", "hullCardValue",
                    "cargoValue", "cargoCardValue", "description", "hullProgress", "hullCardProgress", "cargoProgress", "cargoCardProgress",
                    "home", "cargo", "upgrades", "shipyard", "repairs", "depart", "normal", "selected" })
                    if (serialized.FindProperty(field)?.objectReferenceValue == null)
                        throw new InvalidOperationException("Brak referencji mostka: " + field);
                // Zapisujemy ten sam asset, aby zachować GUID istniejącego PortMenu.
                var backup = "Temp/PortVisualIntegrationBackup";
                Directory.CreateDirectory(backup);
                File.Copy(Target, backup + "/PortMenu-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".prefab", false);
                var saved = PrefabUtility.SaveAsPrefabAsset(root, Target);
                if (saved == null) throw new InvalidOperationException("Nie udało się zapisać prefabu.");
                File.WriteAllText("Temp/PortVisualIntegration.result", "PASS: prefab migrated, 22 bridge references validated, existing service bindings preserved. Source: " + sourcePath);
                Selection.activeObject = saved;
                Debug.Log("PortVisualKit połączony z naprawami, ładownią, ulepszeniami, stocznią i odcumowaniem. GUID PortMenu zachowany.");
            }
            catch (Exception error) { Debug.LogError("Integracja portu przerwana bez zapisu prefabu: " + error); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static T Required<T>(Transform root, string path) where T : Component
        {
            var node = root.Find(path);
            if (node == null || node.GetComponent<T>() == null) throw new InvalidOperationException("Brak " + path + " / " + typeof(T).Name);
            return node.GetComponent<T>();
        }
    }
}
