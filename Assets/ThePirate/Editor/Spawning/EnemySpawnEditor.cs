using System;
using System.Collections.Generic;
using ThePirate.Spawning;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ThePirate.EditorTools
{
    public sealed class EnemySpawnEditor : EditorWindow
    {
        private EnemySpawnZone zone;
        private UnityEditor.Editor inspector;
        private Vector2 scroll;
        private int pointCount = 6;
        private string report = "";

        [MenuItem("ThePirate/Edytor przeciwników")]
        public static void Open() { GetWindow<EnemySpawnEditor>("Przeciwnicy"); }
        private void OnDisable() { if (inspector != null) DestroyImmediate(inspector); }
        private void OnSelectionChange()
        {
            if (Selection.activeGameObject != null)
            {
                var selected = Selection.activeGameObject.GetComponent<EnemySpawnZone>();
                if (selected != null) SetZone(selected);
            }
            Repaint();
        }
        private void SetZone(EnemySpawnZone value)
        {
            if (zone == value) return;
            if (inspector != null) DestroyImmediate(inspector);
            zone = value; inspector = null; report = "";
        }
        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Strefy tworzą przeciwników w Play Mode. Initial Count określa liczbę obsadzanych miejsc; Maximum Alive ogranicza jednoczesną obsadę.", MessageType.Info);
            if (EditorApplication.isPlaying) foreach (var d in UnityEngine.Object.FindObjectsByType<EnemySpawnDirector>(FindObjectsInactive.Exclude)) EditorGUILayout.HelpBox(d.gameObject.scene.name + ": " + d.Status, MessageType.Info);
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Dodaj strefę")) CreateZone();
                if (GUILayout.Button("Nowy profil")) CreateProfile();
                EditorGUILayout.EndHorizontal();
                SetZone((EnemySpawnZone)EditorGUILayout.ObjectField("Wybrana strefa", zone, typeof(EnemySpawnZone), true));
                foreach (var candidate in UnityEngine.Object.FindObjectsByType<EnemySpawnZone>(FindObjectsInactive.Exclude))
                    if (GUILayout.Button(candidate.displayName + " — " + candidate.gameObject.scene.name))
                    { SetZone(candidate); Selection.activeGameObject = candidate.gameObject; }
                if (zone == null) return;
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Pokaż w Scene")) { Selection.activeGameObject = zone.gameObject; SceneView.lastActiveSceneView?.FrameSelected(); }
                if (GUILayout.Button("Duplikuj")) DuplicateZone();
                EditorGUILayout.EndHorizontal();
                pointCount = EditorGUILayout.IntSlider("Nowe punkty", pointCount, 1, 32);
                if (GUILayout.Button("Dodaj punkty w strefie")) AddPoints();
                if (GUILayout.Button("Dodaj grupę"))
                { Undo.RecordObject(zone, "Dodaj grupę"); zone.groups.Add(new EnemySpawnGroup()); Dirty(); }
                if (GUILayout.Button("Sprawdź konfigurację")) report = Validate(zone);
                if (report.Length > 0) EditorGUILayout.HelpBox(report, MessageType.Info);
                scroll = EditorGUILayout.BeginScrollView(scroll);
                if (inspector == null) inspector = UnityEditor.Editor.CreateEditor(zone);
                inspector.OnInspectorGUI();
                EditorGUILayout.EndScrollView();
            }
        }
        private void CreateZone()
        {
            var obj = new GameObject("EnemySpawnZone");
            Undo.RegisterCreatedObjectUndo(obj, "Dodaj strefę");
            var created = Undo.AddComponent<EnemySpawnZone>(obj);
            created.zoneId = Guid.NewGuid().ToString("N");
            if (SceneView.lastActiveSceneView != null) obj.transform.position = SceneView.lastActiveSceneView.pivot;
            obj.transform.position = new Vector3(obj.transform.position.x, 0, obj.transform.position.z);
            SetZone(created); Selection.activeGameObject = obj; Dirty();
        }
        private void DuplicateZone()
        {
            var obj = Instantiate(zone.gameObject, zone.transform.parent);
            obj.name = zone.gameObject.name + " Copy";
            Undo.RegisterCreatedObjectUndo(obj, "Duplikuj strefę");
            var copy = obj.GetComponent<EnemySpawnZone>();
            copy.zoneId = Guid.NewGuid().ToString("N");
            copy.displayName += " — kopia";
            SetZone(copy); Selection.activeGameObject = obj; Dirty();
        }
        private void CreateProfile()
        {
            string path = EditorUtility.SaveFilePanelInProject("Profil przeciwnika", "EnemyProfile", "asset", "Wybierz lokalizację profilu.");
            if (string.IsNullOrEmpty(path)) return;
            var profile = CreateInstance<EnemySpawnProfile>();
            AssetDatabase.CreateAsset(profile, AssetDatabase.GenerateUniqueAssetPath(path));
            Undo.RegisterCreatedObjectUndo(profile, "Nowy profil");
            Selection.activeObject = profile; EditorGUIUtility.PingObject(profile);
        }
        private void AddPoints()
        {
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Dodaj punkty spawnu");
            Undo.RecordObject(zone, "Lista punktów");
            for (int i = 0; i < pointCount; i++)
            {
                var obj = new GameObject("SpawnPoint_" + (zone.spawnPoints.Count + 1));
                Undo.RegisterCreatedObjectUndo(obj, "Dodaj punkt");
                Undo.SetTransformParent(obj.transform, zone.transform, "Rodzic punktu");
                // Deterministic preview avoids changing Unity's gameplay RNG.
                obj.transform.position = zone.SamplePoint((i + .5f) / pointCount, Mathf.Repeat(i * .618034f, 1));
                obj.transform.rotation = Quaternion.Euler(0, zone.transform.eulerAngles.y, 0);
                zone.spawnPoints.Add(obj.transform);
            }
            Dirty(); Undo.CollapseUndoOperations(undoGroup);
        }
        private void Dirty() { EditorUtility.SetDirty(zone); EditorSceneManager.MarkSceneDirty(zone.gameObject.scene); SceneView.RepaintAll(); }
        public static string Validate(EnemySpawnZone z)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(z.zoneId)) errors.Add("Brak identyfikatora strefy.");
            foreach (var other in UnityEngine.Object.FindObjectsByType<EnemySpawnZone>(FindObjectsInactive.Exclude))
                if (other != z && other.zoneId == z.zoneId) errors.Add("Powtórzony identyfikator: " + other.name + ". Duplikuj strefy przez to okno.");
            if (z.radius <= 0 || z.rectangleSize.x <= 0 || z.rectangleSize.y <= 0) errors.Add("Wymiary muszą być dodatnie.");
            if (z.maximumAlive < 1 || z.spawnInterval <= 0 || z.clearanceRadius <= 0) errors.Add("Limit, odstęp spawnu i promień kolizji muszą być dodatnie.");
            if (z.despawnDistance <= z.activationDistance) errors.Add("Odległość usuwania musi przekraczać odległość aktywacji.");
            if (z.obstacleMask.value == 0) errors.Add("Wybierz warstwy przeszkód. Walidacja kolizji jest obecnie wyłączona.");
            if (z.transform.lossyScale != Vector3.one) errors.Add("Wymiary są w metrach świata; skala Transform nie skaluje strefy.");
            if (z.groups.Count == 0) errors.Add("Dodaj przynajmniej jedną grupę.");
            int total = 0;
            foreach (var g in z.groups)
            {
                if (g == null) { errors.Add("Pusta grupa."); continue; }
                total += g.initialCount;
                if (g.initialCount < 0 || g.maximumAlive < 1 || g.initialCount > g.maximumAlive) errors.Add(g.name + ": błędna liczebność.");
                if (g.minimumLevel < 1 || g.maximumLevel < g.minimumLevel) errors.Add(g.name + ": błędny zakres poziomów.");
                if (g.minimumRespawnSeconds < 0 || g.maximumRespawnSeconds < g.minimumRespawnSeconds || g.respawnLimit < -1) errors.Add(g.name + ": błędne odnowienie.");
                if (g.profile == null || g.profile.prefab == null) { errors.Add(g.name + ": przypisz profil i prefab."); continue; }
                var p = g.profile;
                if (!EnemySpawnDirector.ValidProfile(p, g.minimumLevel, g.maximumLevel, out string profileError)) errors.Add(g.name + ": " + profileError + ". Sprawdź External Control, Movement Stats, włączone AI i krzywe 0–10.");
                if (!PrefabUtility.IsPartOfPrefabAsset(p.prefab)) errors.Add(g.name + ": statek musi być zasobem prefabu.");
                if (p.team < 2) errors.Add(g.name + ": drużyna przeciwnika musi wynosić co najmniej 2.");
                if (p.prefab.GetComponent<ThePirate.Combat.CombatShip>() == null || p.prefab.GetComponent<ThePirate.AI.EnemyShipAI>() == null || p.prefab.GetComponent<Rigidbody>() == null)
                    errors.Add(g.name + ": prefab wymaga CombatShip, EnemyShipAI i Rigidbody na głównym obiekcie.");
                if (!p.prefab.activeSelf) errors.Add(g.name + ": prefab jest nieaktywny.");
                if (p.overrideAI && (p.loseTargetRange < p.detectionRange || p.minimumDistance >= p.approachDistance)) errors.Add(g.name + ": nieprawidłowe dystanse AI.");
                CheckCurve(p.hullMultiplier, g, "HP", errors); CheckCurve(p.damageMultiplier, g, "obrażenia", errors);
                CheckCurve(p.reloadMultiplier, g, "przeładowanie", errors); CheckCurve(p.speedMultiplier, g, "prędkość", errors);
            }
            if (total > z.maximumAlive) errors.Add("Suma liczebności początkowych przekracza limit strefy.");
            foreach (var point in z.spawnPoints)
            {
                if (point == null) { errors.Add("Pusty punkt spawnu."); continue; }
                if (!z.Contains(point.position)) errors.Add(point.name + ": poza strefą.");
                if (Physics.CheckSphere(point.position, z.clearanceRadius, z.obstacleMask, QueryTriggerInteraction.Ignore)) errors.Add(point.name + ": kolizja na wybranych warstwach.");
                foreach (var center in z.exclusionCenters)
                    if (center != null && new Vector2(point.position.x - center.position.x, point.position.z - center.position.z).magnitude < z.exclusionRadius + z.clearanceRadius)
                        errors.Add(point.name + ": w obszarze wykluczenia.");
            }
            return errors.Count == 0 ? "Konfiguracja poprawna w sprawdzanym zakresie. To nie jest test rozgrywki." : string.Join("\n", errors);
        }
        private static void CheckCurve(AnimationCurve curve, EnemySpawnGroup group, string label, List<string> errors)
        {
            if (curve == null || curve.length == 0) { errors.Add(group.name + ": brak krzywej " + label); return; }
            if (group.maximumLevel - (long)group.minimumLevel > 1000) { errors.Add(group.name + ": zakres poziomów przekracza 1000."); return; }
            for (long level = group.minimumLevel; level <= group.maximumLevel; level++)
            {
                float value = curve.Evaluate(level);
                if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0) { errors.Add(group.name + ": niedodatni mnożnik " + label + " na poziomie " + level); break; }
            }
        }
    }

    [CustomEditor(typeof(EnemySpawnZone))]
    public sealed class EnemySpawnZoneInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Spawnowanie działa w Play Mode. Punkty muszą spełniać odległość, widok kamery i warstwy przeszkód.", MessageType.Info);
            DrawDefaultInspector();
            if (GUILayout.Button("Sprawdź konfigurację")) Debug.Log(EnemySpawnEditor.Validate((EnemySpawnZone)target), target);
        }
        private void OnSceneGUI()
        {
            var z = (EnemySpawnZone)target;
            Handles.color = z.color;
            Handles.Label(z.transform.position, z.displayName);
            if (z.shape != SpawnZoneShape.Circle) return;
            EditorGUI.BeginChangeCheck();
            float radius = Handles.RadiusHandle(Quaternion.Euler(90, 0, 0), z.transform.position, z.radius);
            if (EditorGUI.EndChangeCheck()) { Undo.RecordObject(z, "Zmień promień"); z.radius = Mathf.Max(1, radius); EditorUtility.SetDirty(z); }
        }
    }

    [CustomEditor(typeof(EnemySpawnProfile))]
    public sealed class EnemySpawnProfileInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Profil ustawia HP, baterie, szybkość i opcjonalne AI przy tworzeniu statku. Pozostałe komponenty i nagrody edytuj na prefabie.", MessageType.Info);
            DrawDefaultInspector();
            var p = (EnemySpawnProfile)target;
            using (new EditorGUI.DisabledScope(p.prefab == null))
                if (GUILayout.Button("Otwórz prefab i jego komponenty")) AssetDatabase.OpenAsset(p.prefab);
        }
    }
}


