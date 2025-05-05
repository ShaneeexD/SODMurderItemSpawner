using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BepInEx;

namespace MurderCult
{
    public class ConfigManager
    {
        private static ConfigManager _instance;
        public static ConfigManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ConfigManager();
                }
                return _instance;
            }
        }

        // The loaded configurations
        public List<ModConfig> Configs { get; private set; } = new List<ModConfig>();
        
        // The config folder path
        private string ConfigFolderPath => Path.Combine(Paths.PluginPath, "MurderItemSpawner");

        // Get the default config file path
        private string DefaultConfigFilePath => Path.Combine(ConfigFolderPath, "default.json");

        // Dictionary to track which rules have been triggered
        private Dictionary<string, bool> triggeredRules = new Dictionary<string, bool>();

        // Private constructor
        private ConfigManager()
        {
            LoadConfig();
        }

        // Load all configurations from files
        public void LoadConfig()
        {
            // Clear existing configs
            Configs.Clear();
            
            // Create the directory if it doesn't exist
            if (!Directory.Exists(ConfigFolderPath))
            {
                Directory.CreateDirectory(ConfigFolderPath);
            }
            
            // Check if we need to create a default config
            bool needDefaultConfig = Directory.GetFiles(ConfigFolderPath, "*.json").Length == 0;
            if (needDefaultConfig)
            {
                // Create and save a default config
                ModConfig defaultConfig = new ModConfig();
                defaultConfig.SaveToFile(DefaultConfigFilePath);
                Plugin.Log.LogInfo("Created default configuration file");
            }
            
            // Load all JSON files from the directory
            string[] configFiles = Directory.GetFiles(ConfigFolderPath, "*.json");
            foreach (string configFile in configFiles)
            {
                try
                {
                    ModConfig config = ModConfig.LoadFromFile(configFile);
                    if (config != null)
                    {
                        Configs.Add(config);
                        Plugin.Log.LogInfo($"Loaded configuration from {Path.GetFileName(configFile)} with {config.SpawnRules.Count} rules");
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogError($"Error loading config file {configFile}: {ex.Message}");
                }
            }
            
            // Reset triggered rules
            triggeredRules.Clear();
            foreach (var config in Configs)
            {
                foreach (var rule in config.SpawnRules)
                {
                    triggeredRules[rule.Name] = false;
                }
            }
            
            int totalRules = 0;
            foreach (var config in Configs)
            {
                totalRules += config.SpawnRules.Count;
            }
            Plugin.Log.LogInfo($"Loaded {Configs.Count} configuration files with {totalRules} total rules");
        }

        // Save a configuration to file
        public void SaveConfig(ModConfig config, string fileName)
        {
            string filePath = Path.Combine(ConfigFolderPath, fileName);
            config.SaveToFile(filePath);
            Plugin.Log.LogInfo($"Configuration saved to {fileName}");
        }
        
        // Save the default configuration
        public void SaveDefaultConfig()
        {
            if (Configs.Count > 0)
            {
                SaveConfig(Configs[0], "default.json");
            }
            else
            {
                ModConfig defaultConfig = new ModConfig();
                SaveConfig(defaultConfig, "default.json");
            }
        }

        // Check if any rules should be triggered for a specific event
        public void CheckRulesForEvent(string eventName, string murderType)
        {
            // Check each configuration file
            foreach (var config in Configs)
            {
                if (!config.Enabled)
                    continue;

                foreach (var rule in config.SpawnRules)
                {
                    // Skip if rule is disabled or already triggered
                    if (!rule.Enabled || triggeredRules.ContainsKey(rule.Name) && triggeredRules[rule.Name])
                        continue;

                    // Check if this rule should be triggered
                    if (rule.TriggerEvents.Contains(eventName) && 
                        (string.IsNullOrEmpty(rule.MurderMO) || rule.MurderMO == murderType))
                    {
                        // Start the spawn process
                        if (config.ShowDebugMessages)
                        {
                            Plugin.Log.LogInfo($"Rule '{rule.Name}' triggered by event '{eventName}' with murder type '{murderType}'");
                        }
                        
                        // Schedule the spawn with the specified delay
                        ScheduleSpawn(rule, config.DefaultSpawnDelay);
                        
                        // Mark as triggered
                        triggeredRules[rule.Name] = true;
                    }
                }
            }
        }

        // Schedule an item spawn with delay
        private void ScheduleSpawn(SpawnRule rule, float defaultDelay = 1.0f)
        {
            // Use the rule's delay or the default
            float delay = rule.SpawnDelay >= 0 ? rule.SpawnDelay : defaultDelay;
            
            if (delay <= 0)
            {
                // Spawn immediately
                SpawnItem(rule);
            }
            else
            {
                // Start a timer for delayed spawn
                SpawnItemMailbox.StartTimer(delay, () => SpawnItem(rule));
            }
        }

        // Spawn an item based on a rule
        private void SpawnItem(SpawnRule rule)
        {
            try
            {
                // Get the recipient based on the rule
                Human recipient = GetRecipient(rule.ItemRecipient);
                if (recipient == null)
                {
                    Plugin.Log.LogInfo($"Cannot spawn item for rule '{rule.Name}': No valid recipient found");
                    return;
                }

                // Get the spawn location
                Interactable spawnLocation = GetSpawnLocation(rule, recipient);
                if (spawnLocation == null)
                {
                    Plugin.Log.LogInfo($"Cannot spawn item for rule '{rule.Name}': No valid spawn location found");
                    return;
                }

                // Spawn the item using the existing SpawnItemMailbox class
                SpawnItemMailbox.SpawnItemAtLocation(
                    recipient,
                    spawnLocation,
                    rule.ItemToSpawn,
                    rule.PositionOffset.ToVector3(),
                    rule.CustomItemText,
                    rule.ShowPositionMessage
                );
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error spawning item for rule '{rule.Name}': {ex.Message}");
            }
        }

        // Get the recipient based on the rule type
        private Human GetRecipient(RecipientType recipientType)
        {
            switch (recipientType)
            {
                case RecipientType.Murderer:
                    return MurderController.Instance.currentMurderer;
                
                case RecipientType.Victim:
                    return MurderController.Instance.currentVictim;
                
                case RecipientType.Player:
                    return Player.Instance;
                
                case RecipientType.Random:
                    // Choose randomly between murderer and victim
                    if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
                        return MurderController.Instance.currentMurderer;
                    else
                        return MurderController.Instance.currentVictim;
                
                default:
                    return MurderController.Instance.currentMurderer;
            }
        }

        // Get the spawn location based on the rule
        private Interactable GetSpawnLocation(SpawnRule rule, Human recipient)
        {
            switch (rule.SpawnLocation)
            {
                case SpawnLocationType.Mailbox:
                    return Toolbox.Instance.GetMailbox(recipient);
                
                case SpawnLocationType.Inventory:
                    return null;
                
                case SpawnLocationType.Floor:
                    return null;
                
                case SpawnLocationType.Desk:
                    return null;
                
                case SpawnLocationType.Bed:
                    return null;
                
                case SpawnLocationType.Custom:
                    return null;
                
                default:
                    return Toolbox.Instance.GetMailbox(recipient);
            }
        }

        // Reset all triggered rules
        public void ResetTriggeredRules()
        {
            triggeredRules.Clear();
            foreach (var config in Configs)
            {
                foreach (var rule in config.SpawnRules)
                {
                    triggeredRules[rule.Name] = false;
                }
            }
        }
    }
}
