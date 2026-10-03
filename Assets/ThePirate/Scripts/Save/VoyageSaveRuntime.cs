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
            data = new VoyageSaveData
            {
                scenePath = PirateMenuBootstrap.GameplayScene,
                savedAtUtc = DateTime.UtcNow.ToString("o"),
                ships = new ShipSaveData[ships.Length]
            };
            for (int i = 0; i < ships.Length; i++) data.ships[i] = CaptureShip(ships[i]);
            if (ValidData(data)) return true;
            data = null;
            error = "Stan statków jest niepoprawny; gra nie została zapisana.";
            return false;
        }

        private static ShipSaveData CaptureShip(CombatShip ship)
        {
            var movement = ship.GetComponent<ShipMovementController>();
            var modules = ship.GetComponentsInChildren<ShipDamageModule>(true);
            var batteries = ship.GetComponentsInChildren<WeaponBattery>(true);
            var special = ship.GetComponent<SpecialWeaponController>();
            var saved = new ShipSaveData
            {
                team = ship.Team,
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
                cargoWeight = movement.CurrentCargoWeight,
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
            if (!ValidSceneShips(ships) || !ValidData(data))
            {
                error = "Zapis nie pasuje do obecnej sceny.";
                return false;
            }
            // Całe mapowanie sprawdzamy przed zmianą pierwszego obiektu.
            foreach (ShipSaveData saved in data.ships)
            {
                CombatShip ship = FindTeam(ships, saved.team);
                if (ship == null || !CanApply(ship, saved))
                {
                    error = "Zmienił się układ statku lub jego modułów. Zapis nie został zastosowany.";
                    return false;
                }
            }
            foreach (ShipSaveData saved in data.ships) ApplyShip(FindTeam(ships, saved.team), saved);
            // A player-only save represents a voyage after the enemy was removed.
            // Validate all saved ships first, then remove the scene's initial enemy.
            if (data.ships.Length == 1)
            {
                CombatShip enemy = FindTeam(ships, 2);
                if (enemy != null)
                {
                    enemy.gameObject.SetActive(false);
                    UnityEngine.Object.Destroy(enemy.gameObject);
                }
            }
            return true;
        }

        private static bool CanApply(CombatShip ship, ShipSaveData saved)
        {
            if (ship.Hull == null || ship.Hull.MaxHealth != saved.hullMaxHealth
                || ship.GetComponent<ShipMovementController>() == null
                || (ship.GetComponent<SpecialWeaponController>() != null) != saved.hasSpecialWeapon) return false;
            if (saved.docked)
            {
                PortDock port = PortDock.Find(ship.gameObject.scene, saved.portId);
                Vector3 delta = port != null ? port.Berth.position - saved.position : Vector3.one;
                delta.y = 0f;
                if (port == null || delta.sqrMagnitude > 0.01f || ship.GetComponent<ShipDocking>() == null || !port.AvailableFor(ship.GetComponent<ShipDocking>())) return false;
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

        private static void ApplyShip(CombatShip ship, ShipSaveData saved)
        {
            ship.GetComponent<ShipDocking>()?.Undock();
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
            movement.SetCargoWeight(saved.cargoWeight);
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
                if (ship != null && ship.gameObject.scene == scene) result.Add(ship);
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
            if (data == null || data.version != 1 || data.scenePath != PirateMenuBootstrap.GameplayScene
                || data.ships == null || data.ships.Length < 1 || data.ships.Length > 2) return false;
            bool player = false, enemy = false;
            foreach (ShipSaveData ship in data.ships)
            {
                if (ship == null || (ship.team != 1 && ship.team != 2)) return false;
                if (ship.team == 1) { if (player) return false; player = true; }
                if (ship.team == 2) { if (enemy) return false; enemy = true; }
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
    }
}
