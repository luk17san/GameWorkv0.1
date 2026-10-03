using System;
using System.IO;
using Framework.Menu;
using ThePirate.Save;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThePirate.Editor.Save
{
    public static class SaveMenuSetupBuilder
    {
        private const string MainMenuPath = "Assets/ThePirate/Scenes/MainMenu.unity";
        private const string PausePrefabPath = "Assets/ThePirate/Resources/GameWork/PauseMenu.prefab";
        private const string RequestPath = "Temp/GameWorkSaveUI.request";
        private const string ResultPath = "Temp/GameWorkSaveUI.result";

        [InitializeOnLoadMethod]
        private static void RequestedSetup()
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
                File.WriteAllText(ResultPath, "OK: dodano przyciski zapisu i wczytywania.");
            }
            catch (Exception exception)
            {
                File.WriteAllText(ResultPath, "ERROR: " + exception);
                Debug.LogException(exception);
            }
        }

        [MenuItem("GameWork/Menu/Dodaj zapis i wczytywanie")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Zatrzymaj Play Mode przed aktualizacją menu.");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PausePrefabPath) == null
                || AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuPath) == null)
                throw new FileNotFoundException("Brak menu podstawowego. Utwórz je najpierw.");

            UpgradeMainScene();
            UpgradePausePrefab();
            AssetDatabase.SaveAssets();
            Debug.Log("Menu zapisu gotowe. Otwórz MainMenu i uruchom Play Mode.");
        }

        private static void UpgradePausePrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PausePrefabPath);
            try
            {
                Transform card = Need(root.transform, "MenuCanvas/PausePanel/Card");
                var actions = root.GetComponent<SaveMenuActions>();
                if (actions == null) actions = root.AddComponent<SaveMenuActions>();
                Button resume = Need(card, "ResumeButton").GetComponent<Button>();
                Button back = Need(card, "ReturnButton").GetComponent<Button>();
                Button quit = Need(card, "QuitButton").GetComponent<Button>();
                Need(card, "Title").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 195);
                SetY(resume, 110);
                SetY(back, -130);
                SetY(quit, -210);

                Button save = GetOrCloneButton(card, "SaveButton", back, "Zapisz / nadpisz", 30, actions.SaveGame);
                Button load = GetOrCloneButton(card, "LoadButton", back, "Wczytaj", -50, actions.RequestLoad);
                Transform canvas = Need(root.transform, "MenuCanvas");
                Transform confirm = canvas.Find("LoadConfirmationPanel");
                if (confirm == null)
                {
                    confirm = UnityEngine.Object.Instantiate(Need(canvas, "ConfirmationPanel").gameObject,
                        canvas, false).transform;
                    confirm.name = "LoadConfirmationPanel";
                    Need(confirm, "Card/Question").GetComponent<Text>().text =
                        "Wczytać zapis?\nNiezapisany postęp zostanie utracony.";
                    Button accept = Need(confirm, "Card/AcceptButton").GetComponent<Button>();
                    Button cancel = Need(confirm, "Card/CancelButton").GetComponent<Button>();
                    ChangeAction(accept, actions.LoadGame);
                    ChangeAction(cancel, actions.CancelLoad);
                    Need(accept.transform, "Label").GetComponent<Text>().text = "Tak, wczytaj";
                }
                Button cancelLoad = Need(confirm, "Card/CancelButton").GetComponent<Button>();
                SetActions(actions, Need(root.transform, "MenuCanvas/Feedback").GetComponent<Text>(),
                    load, confirm.gameObject, cancelLoad);
                SerializedObject pauseView = new SerializedObject(root.GetComponent<PauseMenuView>());
                pauseView.FindProperty("secondaryConfirmationPanel").objectReferenceValue = confirm.gameObject;
                pauseView.ApplyModifiedPropertiesWithoutUndo();
                if (PrefabUtility.SaveAsPrefabAsset(root, PausePrefabPath) == null)
                    throw new IOException("Nie udało się zapisać prefabu menu pauzy.");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void UpgradeMainScene()
        {
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = default;
            try
            {
                scene = EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Additive);
                if (!scene.IsValid()) throw new IOException("Nie udało się otworzyć sceny menu.");
                if (scene.handle == previous.handle)
                    throw new IOException("MainMenu jest otwarte do edycji. Przełącz na ShipSandbox i uruchom narzędzie ponownie.");
                SceneManager.SetActiveScene(scene);
                GameObject root = null;
                foreach (GameObject candidate in scene.GetRootGameObjects())
                    if (candidate.name == "MainMenu") { root = candidate; break; }
                if (root == null) throw new IOException("Nie znaleziono obiektu MainMenu.");
                Transform card = Need(root.transform, "MenuCanvas/Background/Card");
                var actions = root.GetComponent<SaveMenuActions>();
                if (actions == null) actions = root.AddComponent<SaveMenuActions>();
                Button newGame = Need(card, "NewGameButton").GetComponent<Button>();
                Button quit = Need(card, "QuitButton").GetComponent<Button>();
                SetY(newGame, 0);
                SetY(quit, -160);
                Button load = GetOrCloneButton(card, "LoadButton", newGame, "Wczytaj", -80, actions.LoadGame);
                SetActions(actions, Need(root.transform, "MenuCanvas/Feedback").GetComponent<Text>(), load, null, null);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new IOException("Nie udało się zapisać sceny MainMenu.");
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                if (scene.IsValid() && scene.isLoaded && scene.handle != previous.handle)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static Transform Need(Transform root, string path)
        {
            Transform found = root.Find(path);
            if (found == null) throw new IOException("Brak elementu menu: " + path);
            return found;
        }

        private static void SetY(Button button, float value)
        {
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, value);
        }

        private static Button GetOrCloneButton(Transform parent, string name, Button example,
            string title, float y, UnityAction action)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return existing.GetComponent<Button>();
            GameObject clone = UnityEngine.Object.Instantiate(example.gameObject, parent, false);
            clone.name = name;
            Button button = clone.GetComponent<Button>();
            ChangeAction(button, action);
            Need(clone.transform, "Label").GetComponent<Text>().text = title;
            SetY(button, y);
            return button;
        }

        private static void ChangeAction(Button button, UnityAction action)
        {
            while (button.onClick.GetPersistentEventCount() > 0)
                UnityEventTools.RemovePersistentListener(button.onClick, 0);
            UnityEventTools.AddPersistentListener(button.onClick, action);
        }

        private static void SetActions(SaveMenuActions actions, Text feedback, Button load,
            GameObject confirmation, Button cancel)
        {
            SerializedObject serialized = new SerializedObject(actions);
            serialized.FindProperty("feedback").objectReferenceValue = feedback;
            serialized.FindProperty("loadButton").objectReferenceValue = load;
            serialized.FindProperty("loadConfirmationPanel").objectReferenceValue = confirmation;
            serialized.FindProperty("cancelLoadButton").objectReferenceValue = cancel;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
