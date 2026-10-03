using System;
using System.Collections.Generic;
using System.IO;
using Framework.Menu;
using ThePirate.Menu;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThePirate.Editor.Menu
{
    public static class MenuSetupBuilder
    {
        private const string PrefabPath = "Assets/ThePirate/Resources/GameWork/PauseMenu.prefab";
        private const string RequestPath = "Temp/GameWorkMenuSetup.request";
        private const string ResultPath = "Temp/GameWorkMenuSetup.result";
        private static readonly Color Navy = new Color(0.035f, 0.07f, 0.105f, 1f);
        private static readonly Color Gold = new Color(0.86f, 0.71f, 0.43f, 1f);

        // Jednorazowe żądanie instalacji jest tworzone osobno, po zatwierdzeniu zmian.
        // Zwykły import skryptu bez tego pliku niczego nie tworzy i nie zapisuje.
        [InitializeOnLoadMethod]
        private static void CheckRequestedSetup()
        {
            if (File.Exists(RequestPath)) EditorApplication.delayCall += RunRequestedSetup;
        }

        private static void RunRequestedSetup()
        {
            if (!File.Exists(RequestPath)) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating
                || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += RunRequestedSetup;
                return;
            }
            File.Delete(RequestPath);
            try
            {
                Build();
                File.WriteAllText(ResultPath, "OK: MainMenu, PauseMenu prefab, EditorBuildSettings.");
            }
            catch (Exception exception)
            {
                File.WriteAllText(ResultPath, "ERROR: " + exception);
                Debug.LogException(exception);
            }
        }

        [MenuItem("GameWork/Menu/Utwórz menu")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Zatrzymaj Play Mode przed utworzeniem menu.");
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(PirateMenuBootstrap.GameplayScene) == null)
                throw new InvalidOperationException("Nie znaleziono sceny ShipSandbox.");

            EnsureFolder("Assets/ThePirate/Resources/GameWork");
            EnsureFolder("Assets/ThePirate/Scenes");
            Scene previous = SceneManager.GetActiveScene();
            Scene scratch = default;
            try
            {
                // Osobna scena robocza chroni otwartą, również niezapisaną scenę użytkownika.
                scratch = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                SceneManager.SetActiveScene(scratch);
                if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null)
                {
                    GameObject root = BuildPauseMenu();
                    if (PrefabUtility.SaveAsPrefabAsset(root, PrefabPath) == null)
                        throw new IOException("Nie udało się zapisać prefabu menu pauzy.");
                    UnityEngine.Object.DestroyImmediate(root);
                }

                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(PirateMenuBootstrap.MainMenuScene) == null)
                {
                    BuildMainMenu();
                    if (!EditorSceneManager.SaveScene(scratch, PirateMenuBootstrap.MainMenuScene))
                        throw new IOException("Nie udało się zapisać sceny MainMenu.");
                }

                AssetDatabase.ImportAsset(PirateMenuBootstrap.MainMenuScene,
                    ImportAssetOptions.ForceSynchronousImport);
                AssetDatabase.ImportAsset(PrefabPath, ImportAssetOptions.ForceSynchronousImport);
                ConfigureBuildScenes();
                Debug.Log("Menu gotowe. Otwórz Assets/ThePirate/Scenes/MainMenu.unity i uruchom Play. "
                    + "Istniejące menu nie jest nadpisywane przy ponownym uruchomieniu narzędzia.");
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                if (scratch.IsValid() && scratch.isLoaded) EditorSceneManager.CloseScene(scratch, true);
            }
        }

        private static GameObject BuildPauseMenu()
        {
            GameObject root = new GameObject("PauseMenu");
            PauseService pause = root.AddComponent<PauseService>();
            GameFlow flow = root.AddComponent<GameFlow>();
            PauseMenuView view = root.AddComponent<PauseMenuView>();
            RectTransform canvas = CreateCanvas(root.transform);
            CanvasGroup controls = canvas.GetComponent<CanvasGroup>();

            RectTransform panel = Backdrop("PausePanel", canvas);
            RectTransform card = Card(panel);
            Label("Title", card, "PAUZA", 42, 175f, 500f, 64f, Gold);
            Button resume = MakeButton("ResumeButton", card, "Wznów", 55f, view.Resume);
            MakeButton("ReturnButton", card, "Powrót do menu", -35f, view.RequestReturnToMenu);
            MakeButton("QuitButton", card, "Wyjdź z gry", -125f, view.RequestQuit);

            RectTransform confirmation = Backdrop("ConfirmationPanel", canvas);
            RectTransform confirmationCard = Card(confirmation);
            Text question = Label("Question", confirmationCard, "Wrócić do menu?", 26, 100f, 520f, 150f, Color.white);
            MakeButton("AcceptButton", confirmationCard, "Tak, zakończ rozgrywkę", -45f, view.AcceptConfirmation);
            Button cancel = MakeButton("CancelButton", confirmationCard, "Anuluj", -135f, view.CancelConfirmation);
            Text feedback = Label("Feedback", canvas, "", 22, -330f, 1000f, 70f, Gold);

            ConfigureFlow(flow, feedback, controls);
            SetObject(view, "flow", flow);
            SetObject(view, "pause", pause);
            SetObject(view, "pausePanel", panel.gameObject);
            SetObject(view, "confirmationPanel", confirmation.gameObject);
            SetObject(view, "confirmationText", question);
            SetObject(view, "resumeButton", resume);
            SetObject(view, "cancelButton", cancel);
            panel.gameObject.SetActive(false);
            confirmation.gameObject.SetActive(false);
            return root;
        }

        private static void BuildMainMenu()
        {
            GameObject root = new GameObject("MainMenu");
            root.AddComponent<PauseService>();
            GameFlow flow = root.AddComponent<GameFlow>();
            MainMenuView view = root.AddComponent<MainMenuView>();
            RectTransform canvas = CreateCanvas(root.transform);
            RectTransform background = Backdrop("Background", canvas);
            background.GetComponent<Image>().color = Navy;
            RectTransform card = Card(background);
            Label("Title", card, "THE PIRATE", 48, 155f, 540f, 90f, Gold);
            Label("Subtitle", card, "Twoja podróż zaczyna się tutaj", 22, 85f, 520f, 45f, Color.white);
            Button first = MakeButton("NewGameButton", card, "Nowa gra", -25f, view.NewGame);
            MakeButton("QuitButton", card, "Wyjdź z gry", -120f, view.QuitGame);
            Text feedback = Label("Feedback", canvas, "", 22, -330f, 1000f, 70f, Gold);
            ConfigureFlow(flow, feedback, canvas.GetComponent<CanvasGroup>());
            SetObject(view, "flow", flow);
            SetObject(view, "firstButton", first);

            GameObject cameraObject = new GameObject("Menu Camera", typeof(Camera), typeof(AudioListener));
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Navy;
            camera.cullingMask = 0;
            GameObject events = new GameObject("EventSystem", typeof(EventSystem));
            events.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static RectTransform CreateCanvas(Transform parent)
        {
            GameObject obj = new GameObject("MenuCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            obj.transform.SetParent(parent, false);
            Canvas canvas = obj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            CanvasScaler scaler = obj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.matchWidthOrHeight = 0.5f;
            return obj.GetComponent<RectTransform>();
        }

        private static RectTransform Backdrop(string name, RectTransform parent)
        {
            RectTransform rect = Rect(name, parent, Vector2.zero, Vector2.zero);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.gameObject.AddComponent<Image>().color = new Color(0f, 0.02f, 0.04f, 0.78f);
            return rect;
        }

        private static RectTransform Card(RectTransform parent)
        {
            RectTransform card = Rect("Card", parent, new Vector2(620f, 520f), Vector2.zero);
            card.gameObject.AddComponent<Image>().color = Navy;
            RectTransform line = Rect("GoldLine", card, new Vector2(540f, 3f), new Vector2(0f, 230f));
            Image image = line.gameObject.AddComponent<Image>();
            image.color = Gold;
            image.raycastTarget = false;
            return card;
        }

        private static Button MakeButton(string name, RectTransform parent, string title, float y, UnityAction action)
        {
            RectTransform rect = Rect(name, parent, new Vector2(440f, 66f), new Vector2(0f, y));
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.11f, 0.20f, 0.26f);
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1.25f, 1.25f, 1.25f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            button.colors = colors;
            Label("Label", rect, title, 27, 0f, 420f, 60f, Color.white);
            UnityEventTools.AddPersistentListener(button.onClick, action);
            return button;
        }

        private static Text Label(string name, RectTransform parent, string value, int size,
            float y, float width, float height, Color color)
        {
            RectTransform rect = Rect(name, parent, new Vector2(width, height), new Vector2(0f, y));
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.text = value;
            text.raycastTarget = false;
            return text;
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 position)
        {
            RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return rect;
        }

        private static void ConfigureFlow(GameFlow flow, Text feedback, CanvasGroup controls)
        {
            SerializedObject serialized = new SerializedObject(flow);
            serialized.FindProperty("mainMenuScene").stringValue = PirateMenuBootstrap.MainMenuScene;
            serialized.FindProperty("gameplayScene").stringValue = PirateMenuBootstrap.GameplayScene;
            serialized.FindProperty("feedback").objectReferenceValue = feedback;
            serialized.FindProperty("controls").objectReferenceValue = controls;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObject(UnityEngine.Object target, string property, UnityEngine.Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(property).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureBuildScenes()
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(PirateMenuBootstrap.MainMenuScene, true),
                new EditorBuildSettingsScene(PirateMenuBootstrap.GameplayScene, true)
            };
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.path == PirateMenuBootstrap.MainMenuScene || scene.path == PirateMenuBootstrap.GameplayScene)
                    continue;
                // Zachowaj inne istniejące sceny, usuń tylko martwe wpisy.
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path) != null) scenes.Add(scene);
            }
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            EditorBuildSettingsScene[] saved = EditorBuildSettings.scenes;
            if (saved.Length < 2 || saved[0].path != PirateMenuBootstrap.MainMenuScene
                || saved[1].path != PirateMenuBootstrap.GameplayScene)
                throw new IOException("Unity nie przyjęło kolejności scen w Build Settings.");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = path.Substring(0, path.LastIndexOf('/'));
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(path.LastIndexOf('/') + 1));
        }
    }
}
