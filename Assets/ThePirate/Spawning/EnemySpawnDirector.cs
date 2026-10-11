using System;
using System.Collections.Generic;
using GameWork.Framework.Ships.Movement;
using ThePirate.Combat;
using ThePirate.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThePirate.Spawning
{
    [Serializable]
    public sealed class SpawnSlotSave
    {
        public string zoneId;
        public int group, slot, respawns;
        public bool started, exhausted, pendingRespawn;
        public float remaining;
    }
    [Serializable]
    public sealed class SpawnEnemySave
    {
        public string instanceId, zoneId, profileName;
        public string prefabName;
        public float hullMultiplier, damageMultiplier, reloadMultiplier, speedMultiplier;
        public int group, slot, level;
        public Vector3 home;
        public ShipSaveData ship;
    }

    [DisallowMultipleComponent]
    public sealed class EnemySpawnDirector : MonoBehaviour
    {
        [SerializeField, Min(1)] private int globalMaximumAlive = 6;
        [SerializeField] private string status = "Inicjalizacja";
        public string Status => status;
        private sealed class Slot
        {
            public EnemySpawnZone zone;
            public int group, index, respawns;
            public bool started, exhausted, pendingRespawn;
            public float due;
            public SpawnedEnemy enemy;
        }
        private readonly List<Slot> slots = new List<Slot>();
        private bool initialized;
        private float nextAttempt;
        private int cursor;
        public static EnemySpawnDirector Ensure(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var existing = root.GetComponentInChildren<EnemySpawnDirector>(true);
                if (existing != null) { existing.Initialize(); return existing; }
            }
            var obj = new GameObject("EnemySpawnDirector");
            SceneManager.MoveGameObjectToScene(obj, scene);
            var result = obj.AddComponent<EnemySpawnDirector>(); result.Initialize(); return result;
        }
        private void Initialize()
        {
            if (initialized) return;
            initialized = true;
            var ids = new HashSet<string>();
            foreach (var z in FindObjectsByType<EnemySpawnZone>(FindObjectsInactive.Exclude))
            {
                if (z.gameObject.scene != gameObject.scene) continue;
                if (string.IsNullOrEmpty(z.zoneId) || !ids.Add(z.zoneId))
                { Debug.LogError("Strefa ma pusty lub powtórzony Zone Id: " + z.name, z); continue; }
                for (int g = 0; g < z.groups.Count; g++)
                {
                    var group = z.groups[g];
                    if (group == null) continue;
                    // Initial Count defines population slots; Maximum Alive is a concurrent cap.
                    for (int i = 0; i < Mathf.Clamp(group.initialCount, 0, 256); i++)
                        slots.Add(new Slot { zone = z, group = g, index = i });
                }
            }
        }
        private static float Distance(Vector3 a, Vector3 b) => new Vector2(a.x - b.x, a.z - b.z).magnitude;
        private CombatShip Player()
        {
            foreach (var ship in CombatShip.ActiveShips)
                if (ship != null && ship.Team == 1 && ship.Alive && ship.gameObject.scene == gameObject.scene) return ship;
            return null;
        }
        private void Update()
        {
            if (Framework.Menu.PauseService.GameplayInputBlocked || VoyageSaveRuntime.LoadPending) return;
            AdvanceSpawning();
        }
        // The normal game loop calls this after the pause/focus guard.
        // Explicit stepping also permits deterministic inspection in batch Play Mode.
        public void AdvanceSpawning()
        {
            Initialize(); ResolveDeaths();
            var player = Player();
            if (player == null) { status = "Brak żywego gracza"; return; }
            var camera = Camera.main;
            foreach (var s in slots)
            {
                if (s.enemy == null || !s.enemy.GetComponent<CombatShip>().Alive) continue;
                var ship = s.enemy.GetComponent<CombatShip>();
                if (Distance(player.transform.position, s.zone.transform.position) > s.zone.despawnDistance
                    && Distance(player.transform.position, ship.transform.position) > s.zone.despawnDistance
                    && ship.CombatRemaining <= 0 && !Visible(camera, ship.transform.position, s.zone.clearanceRadius))
                { s.enemy.RetireWithoutReward(); s.enemy = null; s.due = Time.time; }
            }
            if (Time.time < nextAttempt || slots.Count == 0) return;
            nextAttempt = Time.time + .5f;
            int total = 0;
            foreach (var ship in CombatShip.ActiveShips)
                if (ship != null && ship.gameObject.scene == gameObject.scene && ship.Team >= 2 && ship.Alive) total++;
            if (total >= globalMaximumAlive) { status = "Osiągnięto globalny limit " + globalMaximumAlive; return; }
            for (int attempt = 0; attempt < slots.Count; attempt++)
            {
                var s = slots[cursor++ % slots.Count];
                if (s.zone == null || !s.zone.isActiveAndEnabled || s.exhausted || s.enemy != null || Time.time < s.due) continue;
                if (Distance(player.transform.position, s.zone.transform.position) > s.zone.activationDistance)
                { status = "Gracz poza zasięgiem aktywacji: " + s.zone.displayName; continue; }
                var group = s.zone.groups[s.group];
                int zoneAlive = 0, groupAlive = 0;
                foreach (var other in slots)
                    if (other.zone == s.zone && other.enemy != null && other.enemy.GetComponent<CombatShip>().Alive)
                    { zoneAlive++; if (other.group == s.group) groupAlive++; }
                if (zoneAlive >= s.zone.maximumAlive || groupAlive >= group.maximumAlive) continue;
                if (!ValidProfile(group.profile, group.minimumLevel, group.maximumLevel, out status)) continue;
                if (!TryPoint(s.zone, player, camera, out var position, out var rotation)) continue;
                int level = UnityEngine.Random.Range(group.minimumLevel, group.maximumLevel + 1);
                var enemy = Create(s, position, rotation, level, Guid.NewGuid().ToString("N"), position);
                if (enemy == null) continue;
                if (s.pendingRespawn) s.respawns++;
                s.pendingRespawn = false;
                s.started = true; s.enemy = enemy;
                nextAttempt = Time.time + Mathf.Max(.1f, s.zone.spawnInterval);
                status = "Utworzono " + group.name + ", poziom " + level + " w " + s.zone.displayName;
                break;
            }
        }
        private void ResolveDeaths()
        {
            foreach (var s in slots)
            {
                if (s.enemy == null) continue;
                var ship = s.enemy.GetComponent<CombatShip>();
                if (ship.Alive) continue;
                NotifyDefeated(s.enemy);
            }
        }
        public void NotifyDefeated(SpawnedEnemy enemy)
        {
            var s = Find(enemy.ZoneId, enemy.GroupIndex, enemy.SlotIndex);
            if (s == null || s.enemy != enemy) return;
            s.enemy = null; s.pendingRespawn = true;
            var g = s.zone.groups[s.group];
            s.exhausted = !g.respawn || (g.respawnLimit >= 0 && s.respawns >= g.respawnLimit);
            s.due = Time.time + UnityEngine.Random.Range(g.minimumRespawnSeconds, Mathf.Max(g.minimumRespawnSeconds, g.maximumRespawnSeconds));
        }
        private static bool Visible(Camera camera, Vector3 point, float radius)
        {
            if (camera == null) return true; // Never spawn/despawn through an unknown view.
            return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(camera), new Bounds(point, Vector3.one * radius * 2));
        }
        private bool TryPoint(EnemySpawnZone z, CombatShip player, Camera camera, out Vector3 point, out Quaternion rotation)
        {
            Physics.SyncTransforms();
            point = default; rotation = Quaternion.identity;
            if (z.obstacleMask.value == 0) { status = z.displayName + ": ustaw Obstacle Mask (bez wody)"; return false; }
            for (int i = 0; i < 24; i++)
            {
                Transform manual = z.spawnPoints.Count > 0 ? z.spawnPoints[UnityEngine.Random.Range(0, z.spawnPoints.Count)] : null;
                if (z.spawnPoints.Count > 0 && manual == null) continue;
                point = manual != null ? manual.position : z.SamplePoint(UnityEngine.Random.value, UnityEngine.Random.value);
                rotation = manual != null ? manual.rotation : Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
                if (!z.Contains(point) || Distance(point, player.transform.position) < z.minimumPlayerDistance) continue;
                if (z.requireOutsideCamera && Visible(camera, point, z.clearanceRadius)) continue;
                bool blocked = Physics.CheckSphere(point, z.clearanceRadius, z.obstacleMask, QueryTriggerInteraction.Ignore);
                foreach (var center in z.exclusionCenters)
                    if (center != null && Distance(center.position, point) < z.exclusionRadius + z.clearanceRadius) blocked = true;
                foreach (var port in FindObjectsByType<ThePirate.Ports.PortDock>(FindObjectsInactive.Exclude))
                    if (port.gameObject.scene == gameObject.scene && Distance(port.Berth.position, point) < z.exclusionRadius + z.clearanceRadius) blocked = true;
                foreach (var ship in CombatShip.ActiveShips)
                    if (ship != null && ship.gameObject.scene == gameObject.scene && Distance(ship.transform.position, point) < z.clearanceRadius * 2) blocked = true;
                if (!blocked) return true;
            }
            status = z.displayName + ": brak wolnego punktu poza kamerą / za blisko gracza / przeszkoda";
            return false;
        }
        private SpawnedEnemy Create(Slot slot, Vector3 point, Quaternion rotation, int level, string id, Vector3 home)
        {
            var p = slot.zone.groups[slot.group].profile;
            var obj = Instantiate(p.prefab, point, rotation);
            SceneManager.MoveGameObjectToScene(obj, gameObject.scene);
            var ship = obj.GetComponent<CombatShip>();
            ship.Configure(p.team);
            float speed = p.speedMultiplier.Evaluate(level);
            ship.GetComponent<ShipMovementController>().SetSpawnSpeedMultiplier(speed);
            int maximum = Mathf.Max(1, Mathf.RoundToInt(ship.Hull.BaseMaxHealth * p.hullMultiplier.Evaluate(level)));
            ship.Hull.SetMaximumPreservingDamage(maximum); ship.Hull.RestoreCurrentHealth(maximum);
            foreach (var battery in obj.GetComponentsInChildren<WeaponBattery>(true))
                battery.SetSpawnMultipliers(p.damageMultiplier.Evaluate(level), p.reloadMultiplier.Evaluate(level));
            obj.GetComponent<ThePirate.AI.EnemyShipAI>().ConfigureSpawn(p, speed, home);
            var marker = obj.GetComponent<SpawnedEnemy>() ?? obj.AddComponent<SpawnedEnemy>();
            marker.Initialize(id, slot.zone.zoneId, slot.group, slot.index, level, home);
            marker.Bind(this);
            obj.name = p.prefab.name + " Lv." + level;
            return marker;
        }
        public static bool ValidProfile(EnemySpawnProfile p, int minimum, int maximum, out string error)
        {
            error = "Niepoprawny profil przeciwnika";
            if (p == null || p.prefab == null || !p.prefab.activeSelf || p.team < 2 || minimum < 1 || maximum < minimum || maximum > 1000) return false;
            var movement = p.prefab.GetComponent<ShipMovementController>();
            if (p.prefab.GetComponent<CombatShip>() == null || p.prefab.GetComponent<Framework.Health.Health>() == null
                || p.prefab.GetComponent<Rigidbody>() == null || p.prefab.GetComponent<ThePirate.AI.EnemyShipAI>() == null
                || !p.prefab.GetComponent<ThePirate.AI.EnemyShipAI>().enabled || movement == null || !movement.enabled || !movement.ExternalControl || !movement.HasMovementConfiguration) return false;
            var curves = new[] { p.hullMultiplier, p.damageMultiplier, p.reloadMultiplier, p.speedMultiplier };
            foreach (var curve in curves)
                for (int level = minimum; level <= maximum; level++)
                {
                    if (curve == null || curve.length == 0) return false;
                    float v = curve.Evaluate(level);
                    if (float.IsNaN(v) || float.IsInfinity(v) || v <= 0 || v > 10) return false;
                }
            for (int level = minimum; level <= maximum; level++) if (p.reloadMultiplier.Evaluate(level) < .1f) return false;
            error = null; return true;
        }
        public void Capture(out SpawnSlotSave[] state, out SpawnEnemySave[] enemies)
        {
            ResolveDeaths();
            var states = new List<SpawnSlotSave>(); var records = new List<SpawnEnemySave>();
            foreach (var s in slots)
            {
                states.Add(new SpawnSlotSave { zoneId = s.zone.zoneId, group = s.group, slot = s.index, respawns = s.respawns, started = s.started, exhausted = s.exhausted, pendingRespawn = s.pendingRespawn, remaining = Mathf.Max(0, s.due - Time.time) });
                if (s.enemy != null && s.enemy.GetComponent<CombatShip>().Alive)
                    records.Add(new SpawnEnemySave { instanceId = s.enemy.InstanceId, zoneId = s.zone.zoneId, group = s.group, slot = s.index, level = s.enemy.Level,
                        profileName = s.zone.groups[s.group].profile.name, prefabName = s.zone.groups[s.group].profile.prefab.name,
                        hullMultiplier = s.zone.groups[s.group].profile.hullMultiplier.Evaluate(s.enemy.Level), damageMultiplier = s.zone.groups[s.group].profile.damageMultiplier.Evaluate(s.enemy.Level),
                        reloadMultiplier = s.zone.groups[s.group].profile.reloadMultiplier.Evaluate(s.enemy.Level), speedMultiplier = s.zone.groups[s.group].profile.speedMultiplier.Evaluate(s.enemy.Level),
                        home = s.enemy.Home, ship = VoyageSaveRuntime.CaptureShip(s.enemy.GetComponent<CombatShip>()) });
            }
            state = states.ToArray(); enemies = records.ToArray();
        }
        private Slot Find(string id, int group, int index) => slots.Find(s => s.zone.zoneId == id && s.group == group && s.index == index);
        public bool TryPrepareRestore(SpawnSlotSave[] states, SpawnEnemySave[] records, out List<SpawnedEnemy> prepared, out string error)
        {
            prepared = new List<SpawnedEnemy>(); error = null;
            if (states == null && records == null) return true; // Legacy save.
            if (states == null || records == null || states.Length != slots.Count) { error = "Liczba stref/grup zmieniła się od zapisu."; return false; }
            foreach (var state in states)
                if (Find(state.zoneId, state.group, state.slot) == null) { error = "Brak strefy lub grupy z zapisu."; return false; }
            foreach (var r in records)
            {
                var s = Find(r.zoneId, r.group, r.slot);
                if (s == null || s.zone.groups[s.group].profile == null || s.zone.groups[s.group].profile.name != r.profileName
                    || !ValidProfile(s.zone.groups[s.group].profile, r.level, r.level, out error)) { error = "Profil przeciwnika z zapisu jest niedostępny."; return false; }
                var p = s.zone.groups[s.group].profile;
                if (p.prefab.name != r.prefabName || p.team != r.ship.team || !Mathf.Approximately(p.hullMultiplier.Evaluate(r.level), r.hullMultiplier)
                    || !Mathf.Approximately(p.damageMultiplier.Evaluate(r.level), r.damageMultiplier) || !Mathf.Approximately(p.reloadMultiplier.Evaluate(r.level), r.reloadMultiplier)
                    || !Mathf.Approximately(p.speedMultiplier.Evaluate(r.level), r.speedMultiplier)) { error = "Profil lub krzywe poziomów zmieniły się od zapisu."; return false; }
                var enemy = Create(s, r.ship.position, r.ship.rotation, r.level, r.instanceId, r.home);
                prepared.Add(enemy);
                if (!VoyageSaveRuntime.CanApply(enemy.GetComponent<CombatShip>(), r.ship)) { error = "Statystyki lub komponenty przeciwnika zmieniły się od zapisu."; return false; }
            }
            return true;
        }
        public void CommitRestore(SpawnSlotSave[] states, SpawnEnemySave[] records, List<SpawnedEnemy> prepared)
        {
            if (states == null) return;
            foreach (var s in slots) if (s.enemy != null) s.enemy.RetireWithoutReward();
            foreach (var data in states)
            {
                var s = Find(data.zoneId, data.group, data.slot);
                s.enemy = null; s.started = data.started; s.exhausted = data.exhausted; s.pendingRespawn = data.pendingRespawn; s.respawns = data.respawns; s.due = Time.time + data.remaining;
            }
            for (int i = 0; i < records.Length; i++)
            {
                var r = records[i]; var enemy = prepared[i];
                VoyageSaveRuntime.ApplyShip(enemy.GetComponent<CombatShip>(), r.ship);
                Find(r.zoneId, r.group, r.slot).enemy = enemy;
            }
            nextAttempt = Time.time + .5f;
        }
    }
}
