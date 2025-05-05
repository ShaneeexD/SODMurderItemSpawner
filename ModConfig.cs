using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using UnityEngine;
using BepInEx;

namespace MurderCult
{
    [Serializable]
    public class ModConfig
    {
        // General settings
        public bool Enabled { get; set; } = true;
        public float DefaultSpawnDelay { get; set; } = 1.0f;
        public bool ShowDebugMessages { get; set; } = true;
        
        // List of spawn rules
        public List<SpawnRule> SpawnRules { get; set; } = new List<SpawnRule>();

        // Default constructor with sample rules
        public ModConfig()
        {
            // Add a default rule as an example
            SpawnRules.Add(new SpawnRule
            {
                Name = "Default Pencil in Mailbox",
                Enabled = true,
                TriggerEvents = new List<string> { "OnVictimKilled" },
                MurderMO = "TheDoveKiller",
                ItemToSpawn = "Pencil",
                SpawnLocation = SpawnLocationType.Mailbox,
                ItemRecipient = RecipientType.Murderer,
                PositionOffset = new Vector3Serializable(0.2f, 0.0f, 0.12f),
                CustomItemText = "The Dove Killer was here",
                SpawnDelay = 1.0f
            });
        }

        // Save configuration to file
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

        // Load configuration from file
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

            // Return default config if loading fails
            return new ModConfig();
        }
    }

    // Serializable Vector3 for JSON
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

    // Enum for spawn location types
    public enum SpawnLocationType
    {
        Mailbox,
        Inventory,
        Floor,
        Desk,
        Bed,
        Custom
    }

    // Enum for who should receive the item
    public enum RecipientType
    {
        Murderer,
        Victim,
        Player,
        Random
    }

    // Class to define a spawn rule
    [Serializable]
    public class SpawnRule
    {
        // Identification
        public string Name { get; set; } = "Unnamed Rule";
        public bool Enabled { get; set; } = true;
        
        // Trigger conditions
        public List<string> TriggerEvents { get; set; } = new List<string>();
        public string MurderMO { get; set; } = "";
        
        // What to spawn
        public string ItemToSpawn { get; set; } = "Pencil";
        
        // Where to spawn
        public SpawnLocationType SpawnLocation { get; set; } = SpawnLocationType.Mailbox;
        public RecipientType ItemRecipient { get; set; } = RecipientType.Murderer;
        
        // Custom position
        public Vector3Serializable PositionOffset { get; set; } = new Vector3Serializable(0, 0, 0);
        
        // Item properties
        public string CustomItemText { get; set; } = "";
        
        // Timing
        public float SpawnDelay { get; set; } = 1.0f;
        
        // Additional options
        public bool ShowPositionMessage { get; set; } = true;
        public int SpawnCount { get; set; } = 1;
        
        // Custom location (if SpawnLocation is Custom)
        public string CustomLocationName { get; set; } = "";
    }
}
