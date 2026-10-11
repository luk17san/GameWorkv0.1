using System;
using UnityEngine;

namespace ThePirate.Save
{
    [Serializable]
    public sealed class VoyageSaveData
    {
        public int version = 1;
        // Opcjonalne pole v1: starsze zapisy otrzymują saldo 0.
        public int gold;
        public string scenePath;
        public string savedAtUtc;
        public ShipSaveData[] ships;
        public ThePirate.Spawning.SpawnSlotSave[] spawnSlots;
        public ThePirate.Spawning.SpawnEnemySave[] spawnedEnemies;
        public ThePirate.Economy.FloatingLootData[] floatingLoot;
    }

    [Serializable]
    public sealed class ShipSaveData
    {
        public int team;
        public string shipTypeId;
        public string shipInstanceId;
        public ThePirate.Economy.ShipUpgradeState upgrades;
        public bool docked;
        public string portId;
        public float combatRemaining;
        public Vector3 position;
        public Quaternion rotation;
        public int hullHealth;
        public int hullMaxHealth;
        public float requestedSpeed;
        public float currentSpeed;
        public bool reverseArmed;
        public float cargoWeight;
        public bool hasCargoInventory;
        public float legacyCargoWeight;
        public Framework.Inventory.ResourceStack[] cargo;
        public Vector3 externalVelocity;
        public ModuleSaveData[] modules;
        public BatterySaveData[] batteries;
        public bool hasSpecialWeapon;
        public float specialReloadRemaining;
    }

    [Serializable]
    public sealed class ModuleSaveData
    {
        public string path;
        public int health;
        public int maxHealth;
    }

    [Serializable]
    public sealed class BatterySaveData
    {
        public string path;
        public float reloadRemaining;
    }
}
