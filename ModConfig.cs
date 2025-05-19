using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using UnityEngine;
using BepInEx;

namespace MurderItemSpawner
{
    public class ModConfig
    {
        public static ModConfig Instance { get; set; }

        public bool Enabled { get; set; } = true;
        public bool ShowDebugMessages { get; set; } = true;

        public List<SpawnRule> SpawnRules { get; set; } = new List<SpawnRule>();

        public ModConfig()
        {
            SpawnRules.Add(new SpawnRule
            {
                Name = "Default Pencil in Mailbox",
                Enabled = true,
                MurderMO = "ExampleMO",
                TriggerEvents = new List<string> { "OnVictimKilled" },
                ItemToSpawn = "Pencil",
                SpawnLocation = SpawnLocationType.Mailbox,
                BelongsTo = BelongsTo.Murderer,
                Recipient = Recipient.Victim,
                SpawnChance = 1f,
                UnlockMailbox = true,
                SubLocationTypeBuildingEntrances = SubLocationTypeBuildingEntrances.Inside,
                RandomSpawnLocations = new List<string> { "Doormat", "Lobby", "BuildingEntrance", "WorkplaceBuildingEntrance" },
                HotelRooftopBarSubLocations = new List<string> { "RooftopBar", "BarDiningRoom" }
            });
        }

        public void SaveToFile(string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Failed to save configuration: {ex.Message}");
            }
        }

        public static ModConfig LoadFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<ModConfig>(json);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Failed to load configuration: {ex.Message}");
            }

            return new ModConfig();
        }
        

    }

    [Serializable]
    public class Vector3Serializable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3Serializable() { }

        public Vector3Serializable(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }

        public static Vector3Serializable FromVector3(Vector3 vector)
        {
            return new Vector3Serializable(vector.x, vector.y, vector.z);
        }
    }

    public enum SpawnLocationType
    {
        Mailbox = 0,
        Doormat = 1,
        HomeLobby = 2,
        WorkplaceLobby = 3,
        HomeBuildingEntrance = 4,
        WorkplaceBuildingEntrance = 5,
        CityHallBathroom = 6,
        HotelRooftopBar = 7,
        Home = 8,
        Workplace = 9,
        Random = 10,
        Custom = 11
    }

    public enum SubLocationTypeBuildingEntrances
    {
        Inside,
        Outside
    }

    public enum BelongsTo
    {
        Murderer,
        Victim,
        Player,
        MurdererDoctor,
        VictimDoctor,
        MurdererLandlord,
        VictimLandlord,
        Random
    }

    public enum Recipient
    {
        Murderer,
        Victim,
        Player,
        MurdererDoctor,
        VictimDoctor,
        MurdererLandlord,
        VictimLandlord,
        Random
    }
    
    public enum TraitRule
    {
        IfAnyOfThese,    // If any of the traits match
        IfAllOfThese,    // If all of the traits match
        IfNoneOfThese    // If none of the traits match
    }

    [Serializable]
    public class TraitModifier
    {
        public BelongsTo Who { get; set; } = BelongsTo.Victim;
        public TraitRule Rule { get; set; } = TraitRule.IfAnyOfThese;
        public List<string> TraitList { get; set; } = new List<string>();
    }
    
    [Serializable]
    public class SpawnRule
    {
        public string Name { get; set; } = "Unnamed Rule";
        public bool Enabled { get; set; } = true;

        public List<string> TriggerEvents { get; set; } = new List<string>();
        public string MurderMO { get; set; } = "ExampleMO";

        public string ItemToSpawn { get; set; } = "Pencil";

        public SpawnLocationType SpawnLocation { get; set; } = SpawnLocationType.Mailbox;
        public BelongsTo BelongsTo { get; set; } = BelongsTo.Murderer;

        public Recipient Recipient { get; set; } = Recipient.Murderer;

        public int SpawnCount { get; set; } = 1;
        public float SpawnChance { get; set; } = 1f;
        public bool UnlockMailbox { get; set; } = true;
        public SubLocationTypeBuildingEntrances SubLocationTypeBuildingEntrances { get; set; } = SubLocationTypeBuildingEntrances.Inside;
        public List<string> RandomSpawnLocations { get; set; } = new List<string>();
        public List<string> HotelRooftopBarSubLocations { get; set; } = new List<string>();
        public string CustomBuildingPreset { get; set; } = "";
        // Single string versions kept for backward compatibility
        public string CustomRoomName { get; set; } = "";
        public string CustomRoomPreset { get; set; } = "";
        public string CustomSubRoomName { get; set; } = "";
        public string CustomSubRoomPreset { get; set; } = "";
        // New list versions
        public List<string> CustomRoomNames { get; set; } = new List<string>();
        public List<string> CustomRoomPresets { get; set; } = new List<string>();
        public List<string> CustomSubRoomNames { get; set; } = new List<string>();
        public List<string> CustomSubRoomPresets { get; set; } = new List<string>();
        public List<string> CustomFloorNames { get; set; } = new List<string>();
        
        // Furniture placement options
        public bool UseFurniture { get; set; } = false;
        public List<string> FurniturePresets { get; set; } = new List<string>();

        // Multiple owners options
        public bool UseMultipleOwners { get; set; } = false;
        public List<BelongsTo> Owners { get; set; } = new List<BelongsTo>();

        // Spawn control options
        public bool OnlySpawnOnce { get; set; } = false;
        
        // Item dependency options
        public bool RequiresPriorItem { get; set; } = false;
        public string RequiredPriorItem { get; set; } = "";
        public bool RequiresSeparateTrigger { get; set; } = true; // Default to requiring a separate trigger
        
        // Multiple trigger options
        public bool RequiresMultipleTriggers { get; set; } = false;
        public int RequiredTriggerCount { get; set; } = 1; // Default to 1, meaning it triggers on first occurrence
        
        // Trait matching options
        public bool UseTraits { get; set; } = false;
        public List<TraitModifier> TraitModifiers { get; set; } = new List<TraitModifier>();

    }
}
