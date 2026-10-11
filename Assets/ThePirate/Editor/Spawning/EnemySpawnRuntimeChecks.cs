using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ThePirate.Combat;
using ThePirate.Save;
using ThePirate.Spawning;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThePirate.EditorTools
{
    // Explicit batch check only. Never runs on normal editor startup.
    [InitializeOnLoad]
    public static class EnemySpawnRuntimeChecks
    {
        private const string Key = "ThePirate.SpawnChecks";
        private static int phase;
        private static double deadline;
        private static EnemySpawnDirector director;
        private static VoyageSaveData snapshot;
        private static string defeatedId;
        private static EnemySpawnZone firstZone;
        private static int assertions;
        static EnemySpawnRuntimeChecks()
        {
            EditorApplication.playModeStateChanged += Changed;
            if (SessionState.GetBool(Key, false)) EditorApplication.update += Tick;
        }
        public static void RunBatch()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Test uruchamiaj wyłącznie w osobnej sesji batch Unity.");
            SessionState.SetBool(Key, true);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }
        private static void Changed(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Key, false) || state != PlayModeStateChange.EnteredPlayMode) return;
            try
            {
                var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThePirate/Prefab/PlayerShip.prefab");
                var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThePirate/Prefab/EnemyAIApproach.prefab");
                Check(playerPrefab != null && enemyPrefab != null, "prefaby testowe dostępne");
                var player = UnityEngine.Object.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
                player.GetComponent<CombatShip>().Configure(1);
                player.GetComponent<GameWork.Framework.Ships.Movement.ShipMovementController>().enabled = false;
                var profile = ScriptableObject.CreateInstance<EnemySpawnProfile>();
                profile.name = "SpawnRuntimeTest"; profile.prefab = enemyPrefab;
                profile.hullMultiplier = AnimationCurve.Constant(1, 10, 2);
                profile.damageMultiplier = AnimationCurve.Constant(1, 10, 1.5f);
                profile.reloadMultiplier = AnimationCurve.Constant(1, 10, .5f);
                profile.speedMultiplier = AnimationCurve.Constant(1, 10, 1.2f);
                for (int i = 0; i < 2; i++)
                {
                    var zone = new GameObject("TestZone" + i).AddComponent<EnemySpawnZone>();
                    zone.zoneId = Guid.NewGuid().ToString("N"); zone.transform.position = new Vector3(150 + i * 150, .5f, 0);
                    zone.radius = 15; zone.activationDistance = 1000; zone.despawnDistance = 2000;
                    zone.minimumPlayerDistance = 0; zone.requireOutsideCamera = false;
                    zone.obstacleMask = 1 << 30; zone.spawnInterval = .1f;
                    zone.groups.Add(new EnemySpawnGroup { profile = profile, initialCount = 1, maximumAlive = 1, minimumLevel = 3, maximumLevel = 3, minimumRespawnSeconds = .3f, maximumRespawnSeconds = .3f, respawnLimit = 1 });
                    if (i == 0) firstZone = zone;
                }
                director = EnemySpawnDirector.Ensure(SceneManager.GetActiveScene());
                Application.runInBackground = true;
                EditorApplication.isPaused = true;
                Set("globalMaximumAlive", 1); phase = 1; deadline = EditorApplication.timeSinceStartup + 15;
                EditorApplication.update -= Tick; EditorApplication.update += Tick;
            }
            catch (Exception e) { Finish(e.ToString()); }
        }
        private static SpawnedEnemy[] Enemies() => UnityEngine.Object.FindObjectsByType<SpawnedEnemy>(FindObjectsInactive.Exclude);
        private static void Set(string name, object value) => typeof(EnemySpawnDirector).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(director, value);
        private static void Tick()
        {
            if (!EditorApplication.isPlaying || phase == 0) return;
            try
            {
                EditorApplication.Step();
                if (EditorApplication.timeSinceStartup > deadline) throw new Exception("Timeout fazy " + phase + ": " + director.Status + "; Time=" + Time.time + "; frame=" + Time.frameCount);
                // Batch Unity has no focused Game view; step the real runtime system explicitly.
                if (director.enabled) director.AdvanceSpawning();
                var enemies = Enemies();
                if (phase == 1 && enemies.Length == 1)
                {
                    Check(enemies[0].Level == 3, "poziom z grupy");
                    Check(enemies[0].GetComponent<CombatShip>().Hull.MaxHealth == enemies[0].GetComponent<CombatShip>().Hull.BaseMaxHealth * 2, "mnożnik HP");
                    var battery = enemies[0].GetComponentInChildren<WeaponBattery>();
                    Check(battery.EffectiveDamage == Mathf.RoundToInt(battery.BaseDamage * 1.5f), "mnożnik obrażeń");
                    Check(Mathf.Approximately(battery.EffectiveReloadTime, battery.BaseReloadTime * .5f), "mnożnik przeładowania");
                    phase = 2; deadline = EditorApplication.timeSinceStartup + 3;
                    return;
                }
                if (phase == 2)
                {
                    if (EditorApplication.timeSinceStartup < deadline - 1) return;
                    Check(enemies.Length == 1, "globalny limit 1");
                    Set("globalMaximumAlive", 2); Set("nextAttempt", 0f);
                    phase = 3; deadline = EditorApplication.timeSinceStartup + 10; return;
                }
                if (phase == 3 && enemies.Length == 2)
                {
                    director.enabled = false;
                    Check(enemies.Select(e => e.ZoneId).Distinct().Count() == 2, "dwie strefy obsadzone");
                    enemies[0].GetComponent<CombatShip>().Hull.TakeDamage(25);
                    object[] capture = { null, null };
                    bool captured = (bool)typeof(VoyageSaveRuntime).GetMethod("TryCapture", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, capture);
                    Check(captured, "zapis wielu wrogów: " + capture[1]);
                    snapshot = JsonUtility.FromJson<VoyageSaveData>(JsonUtility.ToJson((VoyageSaveData)capture[0]));
                    File.WriteAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath), "SpawnCheckSnapshot.json"), JsonUtility.ToJson(snapshot, true));
                    Check(snapshot.spawnedEnemies.Length == 2, "JSON zawiera dwóch wrogów");
                    string json = JsonUtility.ToJson(snapshot);
                    int begin = json.IndexOf(",\"spawnSlots\":", StringComparison.Ordinal);
                    int end = json.IndexOf(",\"floatingLoot\":", StringComparison.Ordinal);
                    var legacy = JsonUtility.FromJson<VoyageSaveData>(json.Remove(begin, end - begin));
                    Check(legacy.spawnSlots == null && legacy.spawnedEnemies == null &&
                        (bool)typeof(VoyageSaveRuntime).GetMethod("ValidData", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { legacy }), "zgodność ze starym zapisem bez stref");
                    Check((bool)typeof(VoyageSaveRuntime).GetMethod("ValidData", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { snapshot }), "walidacja zapisu");
                    var bad = JsonUtility.FromJson<VoyageSaveData>(JsonUtility.ToJson(snapshot));
                    bad.spawnedEnemies[1].instanceId = bad.spawnedEnemies[0].instanceId;
                    Check(!(bool)typeof(VoyageSaveRuntime).GetMethod("ValidData", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { bad }), "duplikat identyfikatora odrzucony");
                    object[] apply = { SceneManager.GetActiveScene(), snapshot, null };
                    Check((bool)typeof(VoyageSaveRuntime).GetMethod("TryApply", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, apply), "odtworzenie stanu: " + apply[2]);
                    enemies = Enemies();
                    Check(enemies.Length == 2, "brak duplikatów po wczytaniu");
                    foreach (var saved in snapshot.spawnedEnemies)
                        Check(enemies.Single(e => e.InstanceId == saved.instanceId).GetComponent<CombatShip>().Hull.CurrentHealth == saved.ship.hullHealth, "odtworzone HP i identyfikator");
                    var dead = enemies.Single(e => e.ZoneId == firstZone.zoneId);
                    defeatedId = dead.InstanceId; dead.GetComponent<CombatShip>().Hull.TakeDamage(int.MaxValue);
                    director.Capture(out var slots, out var records);
                    Check(records.Length == 1 && slots.Single(s => s.zoneId == firstZone.zoneId).pendingRespawn, "śmierć zachowuje odnowienie");
                    float cooldown = slots.Single(s => s.zoneId == firstZone.zoneId).remaining;
                    Check(cooldown > .2f, "czas odnowienia od śmierci");
                    Check(director.TryPrepareRestore(slots, records, out var prepared, out var error), "przygotowanie zapisu podczas odnowienia: " + error);
                    director.CommitRestore(slots, records, prepared);
                    director.Capture(out var restoredSlots, out var restoredRecords);
                    Check(Mathf.Approximately(restoredSlots.Single(s => s.zoneId == firstZone.zoneId).remaining, cooldown) && restoredRecords.Length == 1, "odtworzenie pozostałego czasu i jednej żywej instancji");
                    director.enabled = true; phase = 4; deadline = EditorApplication.timeSinceStartup + 10; return;
                }
                if (phase == 4 && enemies.Any(e => e.ZoneId == firstZone.zoneId && e.GetComponent<CombatShip>().Alive && e.InstanceId != defeatedId))
                {
                    var replacement = enemies.Single(e => e.ZoneId == firstZone.zoneId && e.GetComponent<CombatShip>().Alive);
                    director.enabled = false;
                    replacement.GetComponent<CombatShip>().Hull.TakeDamage(int.MaxValue);
                    director.Capture(out var slots, out var records);
                    Check(slots.Single(s => s.zoneId == firstZone.zoneId).exhausted, "limit odnowień wyczerpany");
                    Check(records.Length == 1, "martwy wróg nie wraca z zapisu");
                    Finish(null);
                }
            }
            catch (Exception e) { Finish(e.ToString()); }
        }
        private static void Check(bool condition, string label)
        {
            if (!condition) throw new Exception("FAIL: " + label);
            assertions++; Debug.Log("Spawn check: " + label);
        }
        private static void Finish(string error)
        {
            SessionState.SetBool(Key, false); EditorApplication.update -= Tick;
            string result = error == null ? "PASS: runtime spawning, two zones, limits, levels, JSON capture/restore, health, death and respawn limit; assertions=" + assertions : "FAIL: " + error;
            string temp = Path.Combine(Path.GetDirectoryName(Application.dataPath), "SpawnVerification");
            Directory.CreateDirectory(temp); File.WriteAllText(Path.Combine(temp, "EnemySpawnRuntimeChecks.result"), result);
            Debug.Log(result); EditorApplication.Exit(error == null ? 0 : 1);
        }
    }
}
