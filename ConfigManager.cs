using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BepInEx;

namespace MurderItemSpawner
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
        private string ConfigFolderPath => Path.Combine(Paths.PluginPath, "ShaneeexD-MurderItemSpawner");

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
            
            // Get the plugins base directory
            string pluginsBasePath = Paths.PluginPath;
            
            // List of all config files found
            List<string> configFiles = new List<string>();
            
            // First check our own folder for any *MIS.json files
            string[] localConfigFiles = Directory.GetFiles(ConfigFolderPath, "*MIS.json");
            if (localConfigFiles.Length > 0)
            {
                // Add all found config files
                configFiles.AddRange(localConfigFiles);
            }
            else
            {
                // Create and save a default config if no configs exist
                string defaultConfigPath = Path.Combine(ConfigFolderPath, "DefaultMIS.json");
                ModConfig defaultConfig = new ModConfig();
                defaultConfig.SaveToFile(defaultConfigPath);
                configFiles.Add(defaultConfigPath);
                Plugin.Log.LogInfo("Created default configuration file: DefaultMIS.json");
            }
            
            // Now search through all directories in the plugins folder
            try
            {
                // Get all directories in the plugins folder
                string[] directories = Directory.GetDirectories(pluginsBasePath, "*", SearchOption.AllDirectories);
                
                foreach (string directory in directories)
                {
                    // Skip our own directory since we already checked it
                    if (directory.Equals(ConfigFolderPath, StringComparison.OrdinalIgnoreCase))
                        continue;
                    
                    // Look for any *MIS.json files in this directory
                    string[] dirConfigFiles = Directory.GetFiles(directory, "*MIS.json");
                    if (dirConfigFiles.Length > 0)
                    {
                        configFiles.AddRange(dirConfigFiles);
                        Plugin.Log.LogInfo($"Found {dirConfigFiles.Length} additional config(s) in: {directory}");
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error searching for config files: {ex.Message}");
            }
            
            // Load all found config files
            foreach (string configFile in configFiles)
            {
                try
                {
                    ModConfig config = ModConfig.LoadFromFile(configFile);
                    if (config != null)
                    {
                        Configs.Add(config);
                        Plugin.Log.LogInfo($"Loaded configuration from {Path.GetFileName(Path.GetDirectoryName(configFile))}/{Path.GetFileName(configFile)} with {config.SpawnRules.Count} rules");
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
                SaveConfig(Configs[0], "DefaultMIS.json");
            }
            else
            {
                ModConfig defaultConfig = new ModConfig();
                SaveConfig(defaultConfig, "DefaultMIS.json");
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
                        SpawnItem(rule);
                        
                        // Mark as triggered
                        triggeredRules[rule.Name] = true;
                    }
                }
            }
        }

        // Spawn an item based on a rule
        private void SpawnItem(SpawnRule rule)
        {
            try
            {
                // Get the item owner based on the BelongsTo property
                Human itemOwner = GetOwner(rule.BelongsTo);
                if (itemOwner == null)
                {
                    Plugin.Log.LogInfo($"Cannot spawn item for rule '{rule.Name}': No valid owner found");
                    return;
                }

                // Get the recipient (spawn location reference) based on the ItemRecipient property
                Human spawnLocationRecipient = GetRecipient(rule.Recipient);
                if (spawnLocationRecipient == null)
                {
                    Plugin.Log.LogInfo($"Cannot spawn item for rule '{rule.Name}': No valid recipient found");
                    return;
                }

                // Get the spawn location using the recipient as reference
                Interactable spawnLocation = GetSpawnLocation(rule, itemOwner, spawnLocationRecipient);
                if (spawnLocation == null && rule.SpawnLocation == SpawnLocationType.Mailbox)
                {
                    Plugin.Log.LogInfo($"Cannot spawn item for rule '{rule.Name}': No valid spawn location found");
                    return;
                }

                if (rule.SpawnLocation == SpawnLocationType.HomeBuildingEntrance)
                {
                    
                }

                // Choose the appropriate spawn method based on the location type
                switch (rule.SpawnLocation)
                {
                    case SpawnLocationType.Mailbox:
                        // Use the mailbox spawner for mailbox locations
                        SpawnItemMailbox.SpawnItemAtLocation(
                            itemOwner,                    // Owner of the item
                            spawnLocationRecipient,       // Recipient used for spawn location reference
                            spawnLocation,                // The actual spawn location
                            rule.ItemToSpawn,
                            rule.UnlockMailbox,
                            rule.SpawnChance
                        );
                        break;
                        
                    case SpawnLocationType.Doormat:
                        // Use the doormat spawner for doormat locations
                        SpawnItemDoormat.SpawnItemAtLocation(
                            itemOwner,                    // Owner of the item
                            spawnLocationRecipient,        // Recipient used for spawn location reference
                            rule.ItemToSpawn,
                            rule.SpawnChance
                        );
                        break;
                        
                    case SpawnLocationType.HomeLobby:
                        // Use the lobby spawner for lobby locations
                        SpawnItemLobbyHome.SpawnItemAtLocation(
                            itemOwner,                    // Owner of the item
                            spawnLocationRecipient,        // Recipient used for spawn location reference
                            rule.ItemToSpawn,
                            rule.SpawnChance
                        );
                        break;

                    case SpawnLocationType.HomeBuildingEntrance:
                        SpawnItemBuildingEntranceHome.SpawnItemAtLocation(
                            itemOwner,                    // Owner of the item
                            spawnLocationRecipient,        // Recipient used for spawn location reference
                            rule.ItemToSpawn,
                            rule.SpawnChance,
                            rule.SubLocationTypeBuildingEntrances
                        );
                        break;
                        
                    case SpawnLocationType.WorkplaceBuildingEntrance:
                        SpawnItemBuildingEntranceWorkplace.SpawnItemAtLocation(
                            itemOwner,                    // Owner of the item
                            spawnLocationRecipient,        // Recipient used for spawn location reference
                            rule.ItemToSpawn,
                            rule.SpawnChance,
                            rule.SubLocationTypeBuildingEntrances
                        );
                        break;

                    case SpawnLocationType.Random:
                       
                        break;
                        
                    // Add cases for other location types here in the future
                    // case SpawnLocationType.Floor:
                    //    SpawnItemFloor.SpawnItemAtLocation(...);
                    //    break;
                        
                    default:
                        // Default to mailbox spawner for now
                        Plugin.Log.LogInfo($"Using mailbox spawner for location type: {rule.SpawnLocation} (will be implemented in the future)");
                        if (spawnLocation != null)
                        {
                            SpawnItemMailbox.SpawnItemAtLocation(
                                itemOwner,                    // Owner of the item
                                spawnLocationRecipient,        // Recipient used for spawn location reference
                                spawnLocation,
                                rule.ItemToSpawn,
                                rule.UnlockMailbox,
                                rule.SpawnChance
                            );
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error spawning item for rule '{rule.Name}': {ex.Message}");
            }
        }

        // Get the recipient based on the rule type
        private Human GetOwner(BelongsTo belongsTo)
        {
            switch (belongsTo)
            {
                case BelongsTo.Murderer:
                    return MurderController.Instance.currentMurderer;
                
                case BelongsTo.Victim:
                    return MurderController.Instance.currentVictim;
                
                case BelongsTo.Player:
                    return Player.Instance;
                
                case BelongsTo.MurdererDoctor:
                    return MurderController.Instance.currentMurderer.GetDoctor();
                
                case BelongsTo.VictimDoctor:
                    return MurderController.Instance.currentVictim.GetDoctor();
                
                case BelongsTo.MurdererLandlord:
                    return MurderController.Instance.currentMurderer.GetLandlord();
                
                case BelongsTo.VictimLandlord:
                    return MurderController.Instance.currentVictim.GetLandlord();
                
                case BelongsTo.Random:
                    // Choose randomly between murderer and victim
                    if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
                        return MurderController.Instance.currentMurderer;
                    else
                        return MurderController.Instance.currentVictim;
                
                default:
                    return MurderController.Instance.currentMurderer;
            }
        }

        private Human GetRecipient(Recipient recipient)
        {
            switch (recipient)
            {
                case Recipient.Murderer:
                    return MurderController.Instance.currentMurderer;
                
                case Recipient.Victim:
                    return MurderController.Instance.currentVictim;
                
                case Recipient.Player:
                    return Player.Instance;
                
                case Recipient.MurdererDoctor:
                    return MurderController.Instance.currentMurderer.GetDoctor();
                
                case Recipient.VictimDoctor:
                    return MurderController.Instance.currentVictim.GetDoctor();
                
                case Recipient.MurdererLandlord:
                    return MurderController.Instance.currentMurderer.GetLandlord();
                
                case Recipient.VictimLandlord:
                    return MurderController.Instance.currentVictim.GetLandlord();
                
                case Recipient.Random:
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
        private Interactable GetSpawnLocation(SpawnRule rule, Human belongsTo, Human recipient)
        {
            switch (rule.SpawnLocation)
            {
                case SpawnLocationType.Mailbox:
                    return Toolbox.Instance.GetMailbox(recipient);
                
                case SpawnLocationType.Doormat:
                    if (recipient != null && recipient.home != null)
                    {
                        Plugin.Log.LogInfo($"[ConfigManager] Checking doormat location for {belongsTo.name} at {recipient.home.name}");
                        return null; // Just return null, actual spawning will happen in SpawnItem
                    }
                    
                    Plugin.Log.LogWarning($"[ConfigManager] Cannot spawn item in doormat: Recipient or home address is null for {recipient?.name}");
                    return null;
                
                case SpawnLocationType.HomeLobby:
                    if (recipient != null && recipient.home != null)
                    {
                        Plugin.Log.LogInfo($"[ConfigManager] Checking lobby location for {belongsTo.name} at {recipient.home.name}");
                        return null; // Just return null, actual spawning will happen in SpawnItem
                    }
                    return null;
                
                case SpawnLocationType.HomeBuildingEntrance:
                    if (recipient != null && recipient.home != null)
                    {
                        Plugin.Log.LogInfo($"[ConfigManager] Checking building entrance location for {belongsTo.name} at {recipient.home.name}");
                        return null; // Just return null, actual spawning will happen in SpawnItem
                    }
                    return null;
                
                case SpawnLocationType.WorkplaceBuildingEntrance:
                    if (belongsTo != null && belongsTo.job != null && belongsTo.job.employer != null && belongsTo.job.employer.address != null)
                    {
                        Plugin.Log.LogInfo($"[ConfigManager] Checking workplace building entrance location for {belongsTo.name} at {belongsTo.job.employer.address.name}");
                        return null; // Just return null, actual spawning will happen in SpawnItem
                    }
                    return null;
                
                case SpawnLocationType.Random:
                    
                    return null;
                
                default:
                    return null;
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
