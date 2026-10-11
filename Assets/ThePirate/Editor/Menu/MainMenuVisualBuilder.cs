using System;
using System.IO;
using System.Linq;
using Framework.Menu;
using ThePirate.Menu;
using ThePirate.Save;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThePirate.Editor.Menu
{
    public static class MainMenuVisualBuilder
    {
        private const string Kit = "Assets/ThePirate/UI/MainMenuVisualKit";
        private const string ScenePath = PirateMenuBootstrap.MainMenuScene;
        private static readonly Color Ink = new Color(0.10f, 0.075f, 0.045f);
        private static readonly Color Gold = new Color(0.90f, 0.79f, 0.57f);
        private static Sprite parchment;
        private static Font font;

        [MenuItem("Tools/GameWork/Menu/Apply illustrated main menu")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before building the menu.");
            Scene existing = SceneManager.GetSceneByPath(ScenePath);
            if (existing.IsValid() && existing.isDirty)
                throw new InvalidOperationException("MainMenu has unsaved changes. Save it before applying the menu.");
            string backup = "Documentation/MenuVisualBackups/" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            Directory.CreateDirectory(backup);
            File.Copy(ScenePath, backup + "/MainMenu.unity", false);
            File.Copy(ScenePath + ".meta", backup + "/MainMenu.unity.meta", false);
            EnsureFolder(Kit + "/Prefabs");
            Sprite background = ImportSprite("harbor-background", false);
            Sprite logoSprite = ImportSprite("logo", true);
            parchment = ImportSprite("button-parchment", true);
            Sprite settingsPanel = ImportSprite("settings-panel", true);
            Sprite[] icons = { ImportSprite("icon-ship", true), ImportSprite("icon-compass", true),
                ImportSprite("icon-gear", true), ImportSprite("icon-door", true) };
            font = AssetDatabase.LoadAssetAtPath<Font>(Kit + "/Fonts/Cinzel.ttf");
            if (font == null) throw new InvalidOperationException("Missing Cinzel font.");
            string[] buttonNames = { "NewGameButton", "LoadButton", "SettingsButton", "QuitButton" };
            string[] titles = { "Nowa gra", "Wczytaj grę", "Ustawienia", "Wyjście" };
            for (int i = 0; i < buttonNames.Length; i++)
            {
                GameObject template = CreateButton(buttonNames[i], null, titles[i], icons[i]).gameObject;
                PrefabUtility.SaveAsPrefabAsset(template, Kit + "/Prefabs/" + buttonNames[i] + ".prefab");
                UnityEngine.Object.DestroyImmediate(template);
            }

            Scene previous = SceneManager.GetActiveScene();
            bool alreadyOpen = existing.IsValid() && existing.isLoaded;
            Scene scene = alreadyOpen ? existing : EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                GameObject root = scene.GetRootGameObjects().Single(x => x.name == "MainMenu");
                GameFlow flow = root.GetComponent<GameFlow>();
                MainMenuView view = root.GetComponent<MainMenuView>();
                SaveMenuActions save = root.GetComponent<SaveMenuActions>();
                if (flow == null || view == null || save == null)
                    throw new InvalidOperationException("Existing menu services are missing.");
                RectTransform canvas = root.transform.Find("MenuCanvas") as RectTransform;
                if (canvas == null) throw new InvalidOperationException("MenuCanvas is missing.");
                for (int i = canvas.childCount - 1; i >= 0; i--)
                    UnityEngine.Object.DestroyImmediate(canvas.GetChild(i).gameObject);
                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                scaler.referenceResolution = new Vector2(1600, 900);
                scaler.matchWidthOrHeight = 1;
                Image art = Image("BackgroundArt", canvas, background, Color.white);
                Stretch(art.rectTransform);
                AspectRatioFitter cover = art.gameObject.AddComponent<AspectRatioFitter>();
                cover.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                cover.aspectRatio = background.rect.width / background.rect.height;
                RectTransform safe = Rect("Background", canvas);
                Stretch(safe);
                CanvasGroup controls = safe.gameObject.AddComponent<CanvasGroup>();
                RectTransform card = Rect("Card", safe);
                Image logo = Image("Logo", card, logoSprite, Color.white);
                logo.preserveAspect = true;
                RectTransform[] buttons = new RectTransform[4];
                Button[] actions = new Button[4];
                for (int i = 0; i < actions.Length; i++)
                {
                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(
                        AssetDatabase.LoadAssetAtPath<GameObject>(Kit + "/Prefabs/" + buttonNames[i] + ".prefab"), card);
                    instance.name = buttonNames[i];
                    buttons[i] = instance.GetComponent<RectTransform>();
                    actions[i] = instance.GetComponent<Button>();
                }
                // Scene-specific actions stay on instances; reusable prefabs have no scene references.
                Bind(actions[0], view.NewGame);
                Bind(actions[1], save.LoadGame);
                Bind(actions[3], view.QuitGame);
                Text feedback = Text("Feedback", card, "", 21, Gold);
                feedback.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(1, -1);
                Set(flow, "feedback", feedback);
                Set(flow, "controls", canvas.GetComponent<CanvasGroup>());
                Set(view, "flow", flow);
                Set(view, "firstButton", actions[0]);
                Set(save, "feedback", feedback);
                Set(save, "loadButton", actions[1]);
                Set(save, "loadConfirmationPanel", null);
                Set(save, "cancelLoadButton", null);

                PirateMenuSettingsView settings = root.GetComponent<PirateMenuSettingsView>();
                if (settings == null) settings = root.AddComponent<PirateMenuSettingsView>();
                Bind(actions[2], settings.Open);
                RectTransform top = Rect("TopRightControls", safe);
                top.anchorMin = top.anchorMax = new Vector2(1, 1);
                top.pivot = new Vector2(1, 1);
                top.sizeDelta = new Vector2(215, 105);
                top.anchoredPosition = new Vector2(-24, -22);
                Text language = Text("Language", top, "PL", 24, Gold);
                Place(language.rectTransform, new Vector2(215, 38), new Vector2(0, 30));
                Button sound = CreateButton("SoundButton", top, "DŹWIĘK: WŁ.", null);
                Place(sound.GetComponent<RectTransform>(), new Vector2(215, 57), new Vector2(0, -20));
                sound.GetComponentInChildren<Text>().fontSize = 17;
                Bind(sound, settings.ToggleSound);

                // Modal is a sibling of controls, so disabling the main menu does not disable the modal.
                RectTransform modal = Rect("SettingsPanel", canvas);
                Stretch(modal);
                Image blocker = modal.gameObject.AddComponent<Image>();
                blocker.color = new Color(0.015f, 0.025f, 0.035f, 0.86f);
                blocker.raycastTarget = true;
                RectTransform settingsCard = Rect("SettingsCard", modal);
                Place(settingsCard, new Vector2(720, 520), Vector2.zero);
                Image panel = Image("Panel", settingsCard, settingsPanel, new Color(0.95f, 0.88f, 0.75f));
                Stretch(panel.rectTransform);
                Text title = Text("Title", settingsCard, "Ustawienia", 36, Ink);
                Place(title.rectTransform, new Vector2(560, 64), new Vector2(0, 173));
                Text volumeLabel = Text("VolumeLabel", settingsCard, "Głośność", 25, Ink);
                Place(volumeLabel.rectTransform, new Vector2(500, 48), new Vector2(0, 91));
                Slider slider = CreateSlider(settingsCard);
                Text qualityTitle = Text("QualityTitle", settingsCard, "Jakość grafiki", 25, Ink);
                Place(qualityTitle.rectTransform, new Vector2(500, 48), new Vector2(0, -20));
                Text quality = Text("QualityLabel", settingsCard, "", 23, Ink);
                Place(quality.rectTransform, new Vector2(285, 52), new Vector2(0, -76));
                Button left = CreateButton("PreviousQuality", settingsCard, "‹", null);
                Place(left.GetComponent<RectTransform>(), new Vector2(95, 64), new Vector2(-205, -76));
                Bind(left, settings.PreviousQuality);
                Button right = CreateButton("NextQuality", settingsCard, "›", null);
                Place(right.GetComponent<RectTransform>(), new Vector2(95, 64), new Vector2(205, -76));
                Bind(right, settings.NextQuality);
                Button close = CreateButton("CloseButton", settingsCard, "Powrót", null);
                Place(close.GetComponent<RectTransform>(), new Vector2(330, 80), new Vector2(0, -167));
                Bind(close, settings.Close);
                Set(settings, "panel", modal.gameObject);
                Set(settings, "menuControls", controls);
                Set(settings, "volume", slider);
                Set(settings, "volumeLabel", volumeLabel);
                Set(settings, "qualityLabel", quality);
                Set(settings, "soundLabel", sound.GetComponentInChildren<Text>());
                Set(settings, "openButton", actions[2]);
                Set(settings, "closeButton", close);
                modal.gameObject.SetActive(false);

                PirateMainMenuLayout layout = canvas.GetComponent<PirateMainMenuLayout>();
                if (layout == null) layout = canvas.gameObject.AddComponent<PirateMainMenuLayout>();
                Set(layout, "safeArea", safe);
                Set(layout, "column", card);
                Set(layout, "logo", logo.rectTransform);
                Set(layout, "feedback", feedback.rectTransform);
                Set(layout, "settingsCard", settingsCard);
                Set(layout, "desktopExit", actions[3].gameObject);
                SerializedObject serialized = new SerializedObject(layout);
                SerializedProperty list = serialized.FindProperty("buttons");
                list.arraySize = buttons.Length;
                for (int i = 0; i < buttons.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
                serialized.ApplyModifiedPropertiesWithoutUndo();
                layout.Refresh(true);
                Canvas.ForceUpdateCanvases();
                layout.Refresh(true);
                EditorSceneManager.MarkSceneDirty(scene);
                Validate(root);
                if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save MainMenu.");
                AssetDatabase.SaveAssets();
                File.WriteAllText("Temp/MainMenuVisual.result", "PASS: MainMenu migrated; 4 editable button prefabs; settings, safe area, bindings validated. Backup: " + backup);
                Debug.Log("Illustrated MainMenu ready. Backup: " + backup);
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                if (!alreadyOpen && scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static Sprite ImportSprite(string name, bool trim)
        {
            string path = Kit + "/Sprites/" + name + ".png";
            if (!File.Exists(path)) throw new FileNotFoundException(path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = trim;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.maxTextureSize = name.StartsWith("icon-") ? 256 : 2048;
            importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (!trim) return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            Texture2D source = new Texture2D(2, 2);
            source.LoadImage(File.ReadAllBytes(path));
            Color32[] pixels = source.GetPixels32();
            int minX = source.width, minY = source.height, maxX = 0, maxY = 0;
            for (int y = 0; y < source.height; y++)
                for (int x = 0; x < source.width; x++)
                    if (pixels[y * source.width + x].a > 8)
                    { minX = Math.Min(minX, x); minY = Math.Min(minY, y); maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, y); }
            float sx = (float)texture.width / source.width, sy = (float)texture.height / source.height;
            Rect bounds = new Rect(Math.Max(0, minX - 2) * sx, Math.Max(0, minY - 2) * sy,
                (Math.Min(source.width - 1, maxX + 2) - Math.Max(0, minX - 2) + 1) * sx,
                (Math.Min(source.height - 1, maxY + 2) - Math.Max(0, minY - 2) + 1) * sy);
            UnityEngine.Object.DestroyImmediate(source);
            Sprite sprite = Sprite.Create(texture, bounds, new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
            sprite.name = name;
            string spritePath = Kit + "/Sprites/" + name + ".sprite.asset";
            Sprite old = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (old == null) AssetDatabase.CreateAsset(sprite, spritePath);
            else { EditorUtility.CopySerialized(sprite, old); UnityEngine.Object.DestroyImmediate(sprite); sprite = old; }
            return sprite;
        }

        private static Button CreateButton(string name, Transform parent, string label, Sprite icon)
        {
            RectTransform rect = Rect(name, parent);
            rect.sizeDelta = new Vector2(610, 130);
            Image surface = rect.gameObject.AddComponent<Image>();
            surface.sprite = parchment;
            surface.color = Color.white;
            surface.raycastTarget = true;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = surface;
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.90f, 0.84f, 0.74f);
            colors.highlightedColor = new Color(1f, 0.97f, 0.83f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color(0.70f, 0.59f, 0.43f);
            colors.disabledColor = new Color(0.43f, 0.43f, 0.43f, 0.78f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            Image emblem = Image("Icon", rect, icon, Ink);
            emblem.preserveAspect = true;
            emblem.rectTransform.anchorMin = new Vector2(0.13f, 0.18f);
            emblem.rectTransform.anchorMax = new Vector2(0.27f, 0.82f);
            emblem.rectTransform.offsetMin = emblem.rectTransform.offsetMax = Vector2.zero;
            emblem.gameObject.SetActive(icon != null);
            Text text = Text("Label", rect, label, 34, Ink);
            text.fontStyle = FontStyle.Bold;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 16;
            text.resizeTextMaxSize = 34;
            text.rectTransform.anchorMin = new Vector2(icon == null ? 0.12f : 0.29f, 0.17f);
            text.rectTransform.anchorMax = new Vector2(0.81f, 0.83f);
            text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
            return button;
        }

        private static Slider CreateSlider(Transform parent)
        {
            RectTransform root = Rect("VolumeSlider", parent);
            Place(root, new Vector2(440, 46), new Vector2(0, 38));
            Slider slider = root.gameObject.AddComponent<Slider>();
            Image hit = root.gameObject.AddComponent<Image>();
            hit.color = Color.clear;
            Image track = Image("Track", root, null, new Color(0.20f, 0.13f, 0.07f, 0.65f));
            Place(track.rectTransform, new Vector2(440, 12), Vector2.zero);
            RectTransform fillArea = Rect("FillArea", root);
            Stretch(fillArea);
            fillArea.offsetMin = new Vector2(12, 15); fillArea.offsetMax = new Vector2(-12, -15);
            Image fill = Image("Fill", fillArea, null, new Color(0.62f, 0.24f, 0.15f));
            Stretch(fill.rectTransform);
            RectTransform handleArea = Rect("HandleArea", root);
            Stretch(handleArea);
            handleArea.offsetMin = new Vector2(12, 0); handleArea.offsetMax = new Vector2(-12, 0);
            Image handle = Image("Handle", handleArea, null, Gold);
            handle.rectTransform.sizeDelta = new Vector2(28, 42);
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.minValue = 0; slider.maxValue = 1;
            return slider;
        }

        private static RectTransform Rect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            return rect;
        }
        private static Image Image(string name, Transform parent, Sprite sprite, Color color)
        {
            Image image = Rect(name, parent).gameObject.AddComponent<Image>();
            image.sprite = sprite; image.color = color; image.raycastTarget = false;
            return image;
        }
        private static Text Text(string name, Transform parent, string value, int size, Color color)
        {
            Text text = Rect(name, parent).gameObject.AddComponent<Text>();
            text.font = font; text.fontSize = size; text.color = color; text.text = value;
            text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false;
            return text;
        }
        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        private static void Place(RectTransform rect, Vector2 size, Vector2 position)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size; rect.anchoredPosition = position;
        }
        private static void Bind(Button button, UnityAction action)
        {
            while (button.onClick.GetPersistentEventCount() > 0) UnityEventTools.RemovePersistentListener(button.onClick, 0);
            UnityEventTools.AddPersistentListener(button.onClick, action);
        }
        private static void Set(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = path.Substring(0, path.LastIndexOf('/'));
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(path.LastIndexOf('/') + 1));
        }
        private static void Validate(GameObject root)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) != 0)
                    throw new InvalidOperationException("Missing script: " + child.name);
            foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(true))
                if (graphic.GetComponent<CanvasRenderer>() == null)
                    throw new InvalidOperationException("CanvasRenderer missing: " + graphic.name);
            foreach (string name in new[] { "NewGameButton", "LoadButton", "SettingsButton", "QuitButton" })
            {
                Button button = root.transform.Find("MenuCanvas/Background/Card/" + name).GetComponent<Button>();
                if (button.onClick.GetPersistentEventCount() != 1 || button.onClick.GetPersistentTarget(0) == null)
                    throw new InvalidOperationException("Invalid action: " + name);
            }
        }
    }
}
