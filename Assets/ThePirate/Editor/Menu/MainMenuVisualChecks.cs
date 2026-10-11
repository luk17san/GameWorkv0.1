using System;
using System.IO;
using System.Reflection;
using Framework.Menu;
using ThePirate.Menu;
using ThePirate.Save;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThePirate.Editor.Menu
{
    [InitializeOnLoad]
    public static class MainMenuVisualChecks
    {
        private const string SessionKey = "GameWork.MenuPreview.PreviousStartScene";
        private const string SizeKey = "GameWork.MenuPreview.PreviousGameViewIndex";
        private const string CountKey = "GameWork.MenuPreview.PreviousCustomCount";
        static MainMenuVisualChecks() => EditorApplication.playModeStateChanged += RestoreStartScene;

        [MenuItem("Tools/GameWork/Menu/Start menu preview")]
        public static void StartPreview()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Already playing.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save open scene edits before preview.");
            SceneAsset previous = EditorSceneManager.playModeStartScene;
            SessionState.SetString(SessionKey, previous == null ? "none" : AssetDatabase.GetAssetPath(previous));
            SessionState.SetBool(SessionKey + ".Background", Application.runInBackground);
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(PirateMenuBootstrap.MainMenuScene);
            EditorApplication.isPlaying = true;
        }

        private static void RestoreStartScene(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetString(SessionKey, "").Length > 0)
            {
                Application.runInBackground = true;
                return;
            }
            if (state != PlayModeStateChange.EnteredEditMode) return;
            string previous = SessionState.GetString(SessionKey, "");
            if (previous.Length == 0) return;
            EditorSceneManager.playModeStartScene = previous == "none" ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(previous);
            SessionState.EraseString(SessionKey);
            Application.runInBackground = SessionState.GetBool(SessionKey + ".Background", false);
            SessionState.EraseBool(SessionKey + ".Background");
            RestoreGameView();
        }

        [MenuItem("Tools/GameWork/Menu/Check menu in Play Mode")]
        public static void CheckRuntime()
        {
            Require(EditorApplication.isPlaying, "Play Mode required.");
            Require(SceneManager.GetActiveScene().path == PirateMenuBootstrap.MainMenuScene, "MainMenu required.");
            GameObject root = GameObject.Find("MainMenu");
            Transform card = root.transform.Find("MenuCanvas/Background/Card");
            PirateMenuSettingsView settings = root.GetComponent<PirateMenuSettingsView>();
            Button open = card.Find("SettingsButton").GetComponent<Button>();
            Require(card.Find("CreatorsButton") == null, "Creators must be omitted.");
            Require(card.Find("LoadButton").GetComponent<Button>().interactable == VoyageSaveRuntime.HasValidSave,
                "Load availability must reflect actual save.");
            Font font = card.Find("NewGameButton/Label").GetComponent<Text>().font;
            foreach (char glyph in "ąćęłńóśźżĄĆĘŁŃÓŚŹŻ") Require(font.HasCharacter(glyph), "Missing Polish glyph.");
            float originalVolume = MenuPreferences.Volume;
            bool hadVolume = PlayerPrefs.HasKey("GameWork.Audio.MasterVolume");
            bool hadMute = PlayerPrefs.HasKey("GameWork.Audio.Muted");
            bool muted = MenuPreferences.Muted;
            try
            {
                open.onClick.Invoke();
                GameObject panel = root.transform.Find("MenuCanvas/SettingsPanel").gameObject;
                Require(panel.activeSelf, "Settings did not open.");
                CanvasGroup controls = root.transform.Find("MenuCanvas/Background").GetComponent<CanvasGroup>();
                Require(!controls.interactable && !controls.blocksRaycasts, "Menu input must be blocked by modal.");
                Slider slider = panel.GetComponentInChildren<Slider>();
                slider.value = 0.37f;
                Require(Mathf.Abs(MenuPreferences.Volume - 0.37f) < 0.001f, "Slider not connected.");
                Require(Mathf.Abs(AudioListener.volume - (muted ? 0f : 0.37f)) < 0.001f, "Audio volume not applied.");
                settings.ToggleSound();
                Require(MenuPreferences.Muted != muted, "Sound toggle not connected.");
                settings.ToggleSound();
                settings.Close();
                Require(!panel.activeSelf && controls.interactable && controls.blocksRaycasts, "Settings did not close cleanly.");
                root.GetComponentInChildren<PirateMainMenuLayout>().Refresh(true);
                RectTransform safe = root.transform.Find("MenuCanvas/Background") as RectTransform;
                foreach (Button button in card.GetComponentsInChildren<Button>())
                {
                    Vector3[] corners = new Vector3[4];
                    button.GetComponent<RectTransform>().GetWorldCorners(corners);
                    foreach (Vector3 corner in corners)
                        Require(safe.rect.Contains(safe.InverseTransformPoint(corner)), "Button outside safe area: " + button.name);
                }
                File.WriteAllText("Temp/MainMenuVisual.PlayMode.result", "PASS: real menu button actions, save availability, Polish glyphs, modal input block, volume slider, mute, return, button bounds. Screen: " + Screen.width + "x" + Screen.height);
                Debug.Log("MainMenu Play Mode smoke checks PASS.");
            }
            finally
            {
                MenuPreferences.SetVolume(originalVolume);
                if (!hadVolume) PlayerPrefs.DeleteKey("GameWork.Audio.MasterVolume");
                if (MenuPreferences.Muted != muted) MenuPreferences.ToggleMute();
                if (!hadMute) PlayerPrefs.DeleteKey("GameWork.Audio.Muted");
                MenuPreferences.Flush();
                settings.Close();
            }
        }

        [MenuItem("Tools/GameWork/Menu/Preview settings panel")]
        public static void PreviewSettings()
        {
            Require(EditorApplication.isPlaying, "Play Mode required.");
            GameObject.Find("MainMenu").GetComponent<PirateMenuSettingsView>().Open();
        }

        [MenuItem("Tools/GameWork/Menu/Check New Game action")]
        public static void CheckNewGame()
        {
            Require(EditorApplication.isPlaying, "Play Mode required.");
            GameObject.Find("MainMenu").transform.Find("MenuCanvas/Background/Card/NewGameButton")
                .GetComponent<Button>().onClick.Invoke();
        }

        [MenuItem("Tools/GameWork/Menu/Preview 16x9")]
        public static void PreviewWide() => SetGameView(1600, 900);
        [MenuItem("Tools/GameWork/Menu/Preview mobile 20x9")]
        public static void PreviewMobile() => SetGameView(2000, 900);
        [MenuItem("Tools/GameWork/Menu/Preview 4x3")]
        public static void PreviewTablet() => SetGameView(1200, 900);

        private static object SizeGroup()
        {
            Assembly assembly = typeof(UnityEditor.Editor).Assembly;
            Type sizesType = assembly.GetType("UnityEditor.GameViewSizes");
            object sizes = sizesType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetValue(null);
            return sizesType.GetMethod("GetGroup").Invoke(sizes, new[] { Enum.ToObject(assembly.GetType("UnityEditor.GameViewSizeGroupType"), 0) });
        }

        private static void SetGameView(int width, int height)
        {
            Assembly assembly = typeof(UnityEditor.Editor).Assembly;
            Type viewType = assembly.GetType("UnityEditor.GameView");
            EditorWindow view = EditorWindow.GetWindow(viewType);
            PropertyInfo index = viewType.GetProperty("selectedSizeIndex", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            object group = SizeGroup();
            Type groupType = group.GetType();
            int builtIn = (int)groupType.GetMethod("GetBuiltinCount").Invoke(group, null);
            int custom = (int)groupType.GetMethod("GetCustomCount").Invoke(group, null);
            if (SessionState.GetInt(SizeKey, -1) < 0)
            {
                SessionState.SetInt(SizeKey, (int)index.GetValue(view));
                SessionState.SetInt(CountKey, custom);
            }
            Type sizeType = assembly.GetType("UnityEditor.GameViewSize");
            object size = Activator.CreateInstance(sizeType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null, new object[] { Enum.ToObject(assembly.GetType("UnityEditor.GameViewSizeType"), 1), width, height, "Menu verification " + width + "x" + height }, null);
            groupType.GetMethod("AddCustomSize").Invoke(group, new[] { size });
            index.SetValue(view, builtIn + custom);
            view.Show(); view.Focus(); view.Repaint();
            EditorApplication.QueuePlayerLoopUpdate();
        }

        private static void RestoreGameView()
        {
            int oldIndex = SessionState.GetInt(SizeKey, -1);
            if (oldIndex < 0) return;
            Type viewType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
            EditorWindow view = EditorWindow.GetWindow(viewType);
            viewType.GetProperty("selectedSizeIndex", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(view, oldIndex);
            object group = SizeGroup();
            Type type = group.GetType();
            int originalCount = SessionState.GetInt(CountKey, 0);
            int count = (int)type.GetMethod("GetCustomCount").Invoke(group, null);
            int builtIn = (int)type.GetMethod("GetBuiltinCount").Invoke(group, null);
            while (count > originalCount) { type.GetMethod("RemoveCustomSize").Invoke(group, new object[] { builtIn + count - 1 }); count--; }
            SessionState.EraseInt(SizeKey); SessionState.EraseInt(CountKey);
        }

        [MenuItem("Tools/GameWork/Menu/Inspect preview dimensions")]
        public static void InspectDimensions()
        {
            Canvas canvas = GameObject.Find("MainMenu/MenuCanvas").GetComponent<Canvas>();
            RectTransform root = canvas.GetComponent<RectTransform>();
            File.WriteAllText("Temp/MainMenuVisual.Dimensions.txt", "Screen=" + Screen.width + "x" + Screen.height
                + " safe=" + Screen.safeArea + " canvas=" + root.rect + " pixel=" + canvas.pixelRect
                + " scale=" + canvas.scaleFactor + " display=" + canvas.renderingDisplaySize
                + " localScale=" + root.localScale + " art=" + root.Find("BackgroundArt").GetComponent<RectTransform>().rect);
        }

        private static void Require(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
        }
    }
}
