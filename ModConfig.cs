using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using UnityEngine;
using BepInEx;

namespace MurderCult
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
        Mailbox,
        Doormat,
        Lobby,
        Desk,
        Bed,
        Custom
    }

    public enum BelongsTo
    {
        Murderer,
        Victim,
        Player,
        MurdererNeighbor,
        VictimNeighbor,
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
        MurdererNeighbor,
        VictimNeighbor,
        MurdererDoctor,
        VictimDoctor,
        MurdererLandlord,
        VictimLandlord,
        Random
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
    }
}
