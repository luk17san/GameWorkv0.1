using System;
using System.Collections.Generic;
using Framework.Save;
using GameWork.Framework.Ships.Movement;
using ThePirate.Combat;
using ThePirate.Menu;
using ThePirate.Ports;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThePirate.Save
{
    public static class VoyageSaveRuntime
    {
        private static VoyageSaveData pending;
        public static bool LoadPending => pending != null;
        public static string LastLoadError { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            pending = null;
            LastLoadError = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Subscribe()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            SceneManager.sceneLoaded += SceneLoaded;
        }

        public static bool HasValidSave => SingleSlotSaveStore.TryRead<VoyageSaveData>(ValidData, out _, out _);

        public static bool TrySave(out string message)
        {
            if (SceneManager.GetActiveScene().path != PirateMenuBootstrap.GameplayScene)
            {
                message = "Zapis jest dostępny tylko podczas rozgrywki.";
                return false;
            }
            if (!TryCapture(out VoyageSaveData data, out message)) return false;
            if (!SingleSlotSaveStore.TryWrite(data, ValidData, out message)) return false;
            LastLoadError = null;
            message = "Gra zapisana.";
            return true;
        }

        public static bool TryLoad(Func<bool> loadScene, out string message)
        {
            if (!SingleSlotSaveStore.TryRead<VoyageSaveData>(ValidData, out VoyageSaveData data, out message))
                return false;
            if (loadScene == null)
            {
                message = "Nie można uruchomić wczytywania gry.";
                return false;
            }
            pending = data;
            LastLoadError = null;
            if (loadScene())
            {
                message = "Wczytywanie gry…";
                return true;
            }
            pending = null;
            message = "Nie udało się otworzyć sceny zapisu.";
            return false;
        }

        private static bool TryCapture(out VoyageSaveData data, out string error)
        {
            data = null;
            error = null;
            CombatShip[] ships = CurrentShips(SceneManager.GetActiveScene());
            if (!ValidSceneShips(ships))
            {
                error = "Zapis wymaga statku gracza i co najwyżej jednego przeciwnika w tej scenie.";
                return false;
            }
            var playerCargo = ThePirate.Economy.ShipCargo.Ensure(FindTeam(ships, 1));
            if (playerCargo == null || !playerCargo.TryGetInventory(out _, out error))
            {
                error = error ?? "Brak ładowni gracza.";
                return false;
            }
            if (!ThePirate.Economy.ShipUpgrades.Ensure(FindTeam(ships, 1)).TryCapture(out _, out error)) return false;
            var identity = ThePirate.Ships.ShipIdentity.Ensure(FindTeam(ships, 1));
            var catalog = ThePirate.Ships.ShipCatalog.Load();
            if (catalog == null) { error = "Brak katalogu statków."; return false; }
            if (!catalog.TryFind(identity.TypeId, out _, out error)) return false;
            data = new VoyageSaveData
            {
                gold = ThePirate.Economy.PlayerGold.Ensure(FindTeam(ships, 1)).Balance,
                scenePath = PirateMenuBootstrap.GameplayScene,
                savedAtUtc = DateTime.UtcNow.ToString("o"),
                floatingLoot = ThePirate.Economy.FloatingLoot.CaptureScene(SceneManager.GetActiveScene()),
                ships = new ShipSaveData[ships.Length]
            };
            for (int i = 0; i < ships.Length; i++) data.ships[i] = CaptureShip(ships[i]);
            ThePirate.Spawning.EnemySpawnDirector.Ensure(SceneManager.GetActiveScene()).Capture(out data.spawnSlots, out data.spawnedEnemies);
            if (ValidData(data)) return true;
            data = null;
            error = "Stan statków jest niepoprawny; gra nie została zapisana.";
            return false;
        }

        public static ShipSaveData CaptureShip(CombatShip ship)
        {
            var movement = ship.GetComponent<ShipMovementController>();
            var modules = ship.GetComponentsInChildren<ShipDamageModule>(true);
            var batteries = ship.GetComponentsInChildren<WeaponBattery>(true);
            var special = ship.GetComponent<SpecialWeaponController>();
            Framework.Inventory.CargoInventory inventory = null;
            if (ship.Team == 1) ThePirate.Economy.ShipCargo.Ensure(ship).TryGetInventory(out inventory, out _);
            ThePirate.Economy.ShipUpgradeState upgradeState = null;
            if (ship.Team == 1) ThePirate.Economy.ShipUpgrades.Ensure(ship).TryCapture(out upgradeState, out _);
            var saved = new ShipSaveData
            {
                team = ship.Team,
                shipTypeId = ship.Team == 1 ? ThePirate.Ships.ShipIdentity.Ensure(ship).TypeId : null,
                shipInstanceId = ship.Team == 1 ? ThePirate.Ships.ShipIdentity.Ensure(ship).InstanceId : null,
                upgrades = upgradeState,
                docked = ship.IsDocked,
                portId = ship.GetComponent<ShipDocking>()?.PortId,
                combatRemaining = ship.CombatRemaining,
                position = ship.transform.position,
                rotation = ship.transform.rotation,
                hullHealth = ship.Hull.CurrentHealth,
                hullMaxHealth = ship.Hull.MaxHealth,
                requestedSpeed = movement.State.RequestedTargetSpeed,
                currentSpeed = movement.State.CurrentPropulsionSpeed,
                reverseArmed = movement.State.ReverseArmed,
                cargoWeight = inventory != null ? inventory.TotalWeight : movement.CurrentCargoWeight,
                hasCargoInventory = inventory != null,
                legacyCargoWeight = inventory != null ? inventory.LegacyWeight : 0f,
                cargo = inventory != null ? inventory.Snapshot() : null,
                externalVelocity = movement.State.ExternalVelocity,
                hasSpecialWeapon = special != null,
                specialReloadRemaining = special != null ? special.ReloadRemaining : 0f,
                modules = new ModuleSaveData[modules.Length],
                batteries = new BatterySaveData[batteries.Length]
            };
            for (int i = 0; i < modules.Length; i++)
                saved.modules[i] = new ModuleSaveData
                {
                    path = RelativePath(ship.transform, modules[i].transform),
                    health = modules[i].CurrentHealth,
                    maxHealth = modules[i].MaxHealth
                };
            for (int i = 0; i < batteries.Length; i++)
                saved.batteries[i] = new BatterySaveData
                {
                    path = RelativePath(ship.transform, batteries[i].transform),
                    reloadRemaining = batteries[i].ReloadRemaining
                };
            return saved;
        }

        private static void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (pending == null || scene.path != PirateMenuBootstrap.GameplayScene) return;
            VoyageSaveData toApply = pending;
            pending = null;
            if (!TryApply(scene, toApply, out string error))
            {
                LastLoadError = error;
                Debug.LogError("Wczytanie gry przerwano: " + error);
            }
        }

        private static bool TryApply(Scene scene, VoyageSaveData data, out string error)
        {
            error = null;
            CombatShip[] ships = CurrentShips(scene);
            if (!ValidSceneShips(ships) || !ValidData(data)) { error = "Zapis nie pasuje do obecnej sceny."; return false; }
            CombatShip originalPlayer = FindTeam(ships, 1);
            CombatShip prepared = null;
            var director = ThePirate.Spawning.EnemySpawnDirector.Ensure(scene);
            var preparedEnemies = new List<ThePirate.Spawning.SpawnedEnemy>();
            bool enemiesCommitted = false;
            try
            {
                ShipSaveData playerData = Array.Find(data.ships, saved => saved.team == 1);
                if (!string.IsNullOrEmpty(playerData.shipTypeId))
                {
                    var catalog = ThePirate.Ships.ShipCatalog.Load();
                    if (catalog == null) { error = "Brak katalogu statków."; return false; }
                    if (!catalog.TryFind(playerData.shipTypeId, out var definition, out error)) return false;
                    if (!ThePirate.Ships.ShipReplacement.TryPrepare(definition, scene, playerData.position, playerData.rotation, out prepared, out error)) return false;
                    ThePirate.Ships.ShipIdentity.Ensure(prepared).Configure(playerData.shipTypeId, playerData.shipInstanceId);
                    for (int i = 0; i < ships.Length; i++) if (ships[i] == originalPlayer) ships[i] = prepared;
                }
                // Wszystkie statki mapujemy przed zmianą starego gracza, złota lub przeciwnika.
                foreach (ShipSaveData saved in data.ships)
                {
                    CombatShip ship = FindTeam(ships, saved.team);
                    if (ship == null || !CanApply(ship, saved, originalPlayer.GetComponent<ShipDocking>()))
                    { error = "Zapis nie pasuje do projektu, ulepszeń, modułów, ładowni lub portu."; return false; }
                }
                if (!director.TryPrepareRestore(data.spawnSlots, data.spawnedEnemies, out preparedEnemies, out error)) return false;
                if (prepared != null)
                {
                    if (!ThePirate.Ships.ShipReplacement.TryCommit(originalPlayer, prepared, null, out error)) return false;
                    prepared = null;
                }
                ThePirate.Economy.PlayerGold.Ensure(FindTeam(ships, 1)).Wallet.TryRestore(data.gold);
                foreach (ShipSaveData saved in data.ships) ApplyShip(FindTeam(ships, saved.team), saved);
                ThePirate.Economy.FloatingLoot.RestoreScene(scene, data.floatingLoot);
                director.CommitRestore(data.spawnSlots, data.spawnedEnemies, preparedEnemies);
                enemiesCommitted = true;
                if (data.ships.Length == 1)
                {
                    CombatShip enemy = FindTeam(ships, 2);
                    if (enemy != null) { enemy.gameObject.SetActive(false); UnityEngine.Object.Destroy(enemy.gameObject); }
                }
                return true;
            }
            finally
            {
                if (prepared != null) ThePirate.Ships.ShipReplacement.Discard(prepared);
                if (!enemiesCommitted) foreach (var enemy in preparedEnemies) if (enemy != null) enemy.RetireWithoutReward();
            }
        }

        public static bool CanApply(CombatShip ship, ShipSaveData saved, ShipDocking originalDocking = null)
        {
            if (ship.Hull == null || (saved.team != 1 && ship.Hull.MaxHealth != saved.hullMaxHealth)
                || ship.GetComponent<ShipMovementController>() == null
                || (ship.GetComponent<SpecialWeaponController>() != null) != saved.hasSpecialWeapon) return false;
            if (saved.team == 1)
            {
                var upgrades = ThePirate.Economy.ShipUpgrades.Ensure(ship);
                if (!upgrades.CanRestore(saved.upgrades, out int maximum, out _) || maximum != saved.hullMaxHealth) return false;
                var cargo = ThePirate.Economy.ShipCargo.Ensure(ship);
                if (cargo == null || !cargo.TryGetInventory(out var inventory, out _)) return false;
                var stacks = saved.hasCargoInventory ? saved.cargo : new Framework.Inventory.ResourceStack[0];
                float legacyWeight = saved.hasCargoInventory ? saved.legacyCargoWeight : saved.cargoWeight;
                if (!inventory.CanRestore(stacks, legacyWeight, out float weight, out _)
                    || Mathf.Abs(weight - saved.cargoWeight) > 0.001f) return false;
            }
            if (saved.docked)
            {
                PortDock port = PortDock.Find(ship.gameObject.scene, saved.portId);
                Vector3 delta = port != null ? port.Berth.position - saved.position : Vector3.one;
                delta.y = 0f;
                if (port == null || delta.sqrMagnitude > 0.01f || ship.GetComponent<ShipDocking>() == null || !port.AvailableFor(originalDocking ?? ship.GetComponent<ShipDocking>())) return false;
            }
            var modules = ship.GetComponentsInChildren<ShipDamageModule>(true);
            var batteries = ship.GetComponentsInChildren<WeaponBattery>(true);
            if (modules.Length != saved.modules.Length || batteries.Length != saved.batteries.Length) return false;
            var moduleMap = new Dictionary<string, ShipDamageModule>();
            foreach (var module in modules) moduleMap[RelativePath(ship.transform, module.transform)] = module;
            foreach (var module in saved.modules)
                if (!moduleMap.TryGetValue(module.path, out var existing) || existing.MaxHealth != module.maxHealth)
                    return false;
            var batteryKeys = new HashSet<string>();
            foreach (var battery in batteries) batteryKeys.Add(RelativePath(ship.transform, battery.transform));
            foreach (var battery in saved.batteries)
                if (!batteryKeys.Contains(battery.path)) return false;
            return true;
        }

        public static void ApplyShip(CombatShip ship, ShipSaveData saved)
        {
            ship.GetComponent<ShipDocking>()?.Undock();
            if (saved.team == 1) ThePirate.Economy.ShipUpgrades.Ensure(ship).TryRestore(saved.upgrades, out _);
            Rigidbody body = ship.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.position = saved.position;
                body.rotation = saved.rotation;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
            else
            {
                ship.transform.SetPositionAndRotation(saved.position, saved.rotation);
            }
            var movement = ship.GetComponent<ShipMovementController>();
            if (saved.team == 1)
            {
                var cargo = ThePirate.Economy.ShipCargo.Ensure(ship);
                cargo.TryGetInventory(out var inventory, out _);
                inventory.TryRestore(saved.hasCargoInventory ? saved.cargo : new Framework.Inventory.ResourceStack[0],
                    saved.hasCargoInventory ? saved.legacyCargoWeight : saved.cargoWeight, out _);
                // Również identyczny/pusty stan ma poprawną masę po wczytaniu.
                movement.SetCargoWeight(inventory.TotalWeight);
            }
            else movement.SetCargoWeight(saved.cargoWeight);
            movement.RestoreMotion(saved.requestedSpeed, saved.currentSpeed,
                saved.reverseArmed, saved.externalVelocity);
            var moduleMap = new Dictionary<string, ShipDamageModule>();
            foreach (var module in ship.GetComponentsInChildren<ShipDamageModule>(true))
                moduleMap[RelativePath(ship.transform, module.transform)] = module;
            foreach (var module in saved.modules) moduleMap[module.path].RestoreCurrentHealth(module.health);
            var batteryMap = new Dictionary<string, WeaponBattery>();
            foreach (var battery in ship.GetComponentsInChildren<WeaponBattery>(true))
                batteryMap[RelativePath(ship.transform, battery.transform)] = battery;
            foreach (var battery in saved.batteries) batteryMap[battery.path].RestoreReload(battery.reloadRemaining);
            if (saved.hasSpecialWeapon)
                ship.GetComponent<SpecialWeaponController>().RestoreReload(saved.specialReloadRemaining);
            // Restore defeat after motion and modules, so sinking owns the final state.
            ship.Hull.RestoreCurrentHealth(saved.hullHealth);
            ship.RestoreCombat(saved.combatRemaining);
            if (saved.docked) ship.GetComponent<ShipDocking>().RestoreDock(PortDock.Find(ship.gameObject.scene, saved.portId));
            if (ship.Hull.IsDepleted) ShipSinking.Ensure(ship)?.FinishLoadedDefeat();
        }

        private static CombatShip[] CurrentShips(Scene scene)
        {
            var result = new List<CombatShip>();
            foreach (CombatShip ship in CombatShip.ActiveShips)
                if (ship != null && ship.gameObject.scene == scene && ship.GetComponent<ThePirate.Spawning.SpawnedEnemy>() == null) result.Add(ship);
            return result.ToArray();
        }

        private static bool ValidSceneShips(CombatShip[] ships)
        {
            if (ships.Length < 1 || ships.Length > 2) return false;
            CombatShip player = FindTeam(ships, 1);
            CombatShip enemy = FindTeam(ships, 2);
            return player != null && player.Alive && (ships.Length == 1 || enemy != null)
                && player.GetComponent<ShipMovementController>() != null
                && (enemy == null || enemy.GetComponent<ShipMovementController>() != null);
        }

        private static CombatShip FindTeam(CombatShip[] ships, int team)
        {
            foreach (CombatShip ship in ships) if (ship.Team == team) return ship;
            return null;
        }

        private static string RelativePath(Transform root, Transform current)
        {
            var parts = new List<string>();
            while (current != null && current != root)
            {
                parts.Add(current.name + "#" + current.GetSiblingIndex());
                current = current.parent;
            }
            parts.Reverse();
            return string.Join("/", parts);
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static bool Finite(Vector3 value) => Finite(value.x) && Finite(value.y) && Finite(value.z);
        private static bool ValidData(VoyageSaveData data)
        {
            if (data == null || !ThePirate.Economy.FloatingLoot.Valid(data.floatingLoot) || data.version != 1 || data.gold < 0 || data.scenePath != PirateMenuBootstrap.GameplayScene
                || data.ships == null || data.ships.Length < 1 || data.ships.Length > 2) return false;
            bool player = false, enemy = false;
            if (!ValidSpawnData(data)) return false;
            var allShips = new List<ShipSaveData>(data.ships);
            var dynamicShips = new HashSet<ShipSaveData>();
            if (data.spawnedEnemies != null) foreach (var record in data.spawnedEnemies) { allShips.Add(record.ship); dynamicShips.Add(record.ship); }
            foreach (ShipSaveData ship in allShips)
            {
                bool dynamicEnemy = dynamicShips.Contains(ship);
                if (ship == null || (dynamicEnemy ? ship.team < 2 : (ship.team != 1 && ship.team != 2))) return false;
                // JsonUtility materializes a null nested class as an empty object.
                // Enemies may carry that empty placeholder, but never player upgrades.
                if (ship.upgrades != null && (!ship.upgrades.Valid || (ship.team != 1 &&
                    (ship.upgrades.Any || !string.IsNullOrEmpty(ship.upgrades.effectsSignature))))) return false;
                if (!string.IsNullOrEmpty(ship.shipTypeId))
                {
                    if (ship.team != 1 || !Framework.Inventory.ResourceDefinition.ValidId(ship.shipTypeId)
                        || !Guid.TryParseExact(ship.shipInstanceId, "N", out _)) return false;
                }
                else if (!string.IsNullOrEmpty(ship.shipInstanceId)) return false;
                if (ship.team == 1) { if (player) return false; player = true; }
                if (ship.team == 2 && !dynamicEnemy) { if (enemy) return false; enemy = true; }
                if (!Finite(ship.position) || !Finite(ship.externalVelocity)
                    || !Finite(ship.rotation.x) || !Finite(ship.rotation.y)
                    || !Finite(ship.rotation.z) || !Finite(ship.rotation.w)
                    || Quaternion.Dot(ship.rotation, ship.rotation) < 0.9f
                    || Quaternion.Dot(ship.rotation, ship.rotation) > 1.1f
                    || !Finite(ship.requestedSpeed) || !Finite(ship.currentSpeed)
                    || !Finite(ship.cargoWeight) || ship.cargoWeight < 0f
                    || Mathf.Abs(ship.requestedSpeed) > 1000f || Mathf.Abs(ship.currentSpeed) > 1000f
                    || ship.hullMaxHealth < 1 || ship.hullHealth < 0 || ship.hullHealth > ship.hullMaxHealth
                    || (ship.team == 1 && ship.hullHealth == 0)
                    || ship.modules == null || ship.batteries == null
                    || !Finite(ship.specialReloadRemaining) || ship.specialReloadRemaining < 0f
                    || ship.specialReloadRemaining > 3600f
                    || ship.modules.Length > 64 || ship.batteries.Length > 32) return false;
                if (!Finite(ship.combatRemaining) || ship.combatRemaining < 0f || ship.combatRemaining > 3600f) return false;
                if (ship.docked && (ship.team != 1 || ship.hullHealth <= 0 || string.IsNullOrWhiteSpace(ship.portId) || ship.portId.Length > 128 || ship.combatRemaining > 0f || Mathf.Abs(ship.requestedSpeed) > 0.01f || Mathf.Abs(ship.currentSpeed) > 0.01f || ship.externalVelocity.sqrMagnitude > 0.0001f)) return false;
                if (!Finite(ship.legacyCargoWeight) || ship.legacyCargoWeight < 0f) return false;
                if (ship.hasCargoInventory)
                {
                    if (ship.team != 1 || ship.cargo == null || ship.cargo.Length > 1024) return false;
                    var ids = new HashSet<string>();
                    foreach (var stack in ship.cargo)
                        if (stack == null || !Framework.Inventory.ResourceDefinition.ValidId(stack.resourceId)
                            || stack.quantity <= 0 || !ids.Add(stack.resourceId)) return false;
                }
                else if (ship.legacyCargoWeight != 0f || (ship.cargo != null && ship.cargo.Length != 0)) return false;
                var paths = new HashSet<string>();
                foreach (ModuleSaveData module in ship.modules)
                    if (module == null || string.IsNullOrEmpty(module.path) || !paths.Add(module.path)
                        || module.maxHealth < 1 || module.health < 0 || module.health > module.maxHealth)
                        return false;
                paths.Clear();
                foreach (BatterySaveData battery in ship.batteries)
                    if (battery == null || string.IsNullOrEmpty(battery.path) || !paths.Add(battery.path)
                        || !Finite(battery.reloadRemaining) || battery.reloadRemaining < 0f
                        || battery.reloadRemaining > 3600f) return false;
            }
            return player && (data.ships.Length == 1 || enemy);
        }

        private static bool ValidSpawnData(VoyageSaveData data)
        {
            if (data.spawnSlots == null && data.spawnedEnemies == null) return true;
            if (data.spawnSlots == null || data.spawnedEnemies == null || data.spawnSlots.Length > 4096 || data.spawnedEnemies.Length > 256) return false;
            var keys = new Dictionary<string, ThePirate.Spawning.SpawnSlotSave>();
            foreach (var s in data.spawnSlots)
            {
                if (s == null || string.IsNullOrEmpty(s.zoneId) || s.zoneId.Length > 128 || s.group < 0 || s.slot < 0 || s.respawns < 0
                    || !Finite(s.remaining) || s.remaining < 0 || s.remaining > 86400) return false;
                string key = s.zoneId + ":" + s.group + ":" + s.slot;
                if (keys.ContainsKey(key)) return false;
                keys.Add(key, s);
            }
            var ids = new HashSet<string>(); var occupied = new HashSet<string>();
            foreach (var e in data.spawnedEnemies)
            {
                if (e == null || e.ship == null || e.ship.hullHealth <= 0 || e.level < 1 || e.level > 1000
                    || !Guid.TryParseExact(e.instanceId, "N", out _) || !ids.Add(e.instanceId) || !Finite(e.home)
                    || string.IsNullOrEmpty(e.profileName) || e.profileName.Length > 256 || string.IsNullOrEmpty(e.prefabName)
                    || !Finite(e.hullMultiplier) || e.hullMultiplier <= 0 || e.hullMultiplier > 10
                    || !Finite(e.damageMultiplier) || e.damageMultiplier <= 0 || e.damageMultiplier > 10
                    || !Finite(e.reloadMultiplier) || e.reloadMultiplier < .1f || e.reloadMultiplier > 10
                    || !Finite(e.speedMultiplier) || e.speedMultiplier <= 0 || e.speedMultiplier > 10) return false;
                string key = e.zoneId + ":" + e.group + ":" + e.slot;
                if (!keys.TryGetValue(key, out var state) || !state.started || state.exhausted || !occupied.Add(key)) return false;
            }
            return true;
        }
    }
}
