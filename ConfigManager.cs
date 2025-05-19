using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SOD.Common;
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
        
        // Dictionary of spawned items (rule name -> bool)
        private Dictionary<string, bool> spawnedItems = new Dictionary<string, bool>();
        
        // Dictionary of item spawn events (rule name -> event name)
        private Dictionary<string, string> itemSpawnEvents = new Dictionary<string, string>();
        
        // Dictionary to track trigger counts for rules that require multiple triggers (rule_name_event -> count)
        private Dictionary<string, int> triggerCounts = new Dictionary<string, int>();
        
        // Current case ID to detect when a new case starts
        private string currentCaseId = "";

        // Private constructor
        private ConfigManager()
        {
            LoadConfig();
            LoadTrackingData();
        }
        
        // Reset tracking dictionaries for a new murder case
        public void ResetTracking()
        {
            Plugin.LogDebug("Resetting item spawn tracking for new murder case");
            triggeredRules.Clear();
            spawnedItems.Clear();
            itemSpawnEvents.Clear();
            triggerCounts.Clear();
            
            // Save the empty tracking data
            SaveTrackingData();
        }
        
        // Get the current save file name
        private string GetCurrentSaveFileName()
        {
            // Check if we have a save file loaded
            if (RestartSafeController.Instance != null && RestartSafeController.Instance.saveStateFileInfo != null)
            {
                return RestartSafeController.Instance.saveStateFileInfo.Name;
            }
            
            // If no save file is loaded, use the default
            return "DEFAULT_SAVE";
        }
        
        // Get all possible save file names for the current session
        // This includes both the current save file name and DEFAULT_SAVE if we're in a new game
        private List<string> GetAllSaveFileNames()
        {
            List<string> saveNames = new List<string>();
            
            // Always add the current save file name
            saveNames.Add(GetCurrentSaveFileName());
            
            // If we're using a real save file (not the default), also add DEFAULT_SAVE
            // This helps with migration from DEFAULT_SAVE to the actual save name
            if (GetCurrentSaveFileName() != "DEFAULT_SAVE")
            {
                saveNames.Add("DEFAULT_SAVE");
            }
            
            return saveNames;
        }
        
        // Save tracking data to PlayerPrefs
        public void SaveTrackingData()
        {
            try
            {
                // Get the current case ID from MurderController
                string caseId = "";
                if (MurderController.Instance != null && MurderController.Instance.chosenMO != null)
                {
                    // Use the murder MO name as a case identifier
                    caseId = MurderController.Instance.chosenMO.name;
                }
                
                // Prepare the serialized data once
                // Save the spawned items
                StringBuilder spawnedItemsStr = new StringBuilder();
                foreach (var kvp in spawnedItems)
                {
                    spawnedItemsStr.Append(kvp.Key).Append(":").Append(kvp.Value ? "1" : "0").Append(",");
                }
                
                // Save the item spawn events
                StringBuilder itemSpawnEventsStr = new StringBuilder();
                foreach (var kvp in itemSpawnEvents)
                {
                    itemSpawnEventsStr.Append(kvp.Key).Append(":").Append(kvp.Value).Append(",");
                }
                
                // Save the triggered rules
                StringBuilder triggeredRulesStr = new StringBuilder();
                foreach (var kvp in triggeredRules)
                {
                    triggeredRulesStr.Append(kvp.Key).Append(":").Append(kvp.Value ? "1" : "0").Append(",");
                }
                
                // Save the trigger counts
                StringBuilder triggerCountsStr = new StringBuilder();
                foreach (var kvp in triggerCounts)
                {
                    triggerCountsStr.Append(kvp.Key).Append(":").Append(kvp.Value).Append(",");
                }
                
                // Get all save file names to save data for
                List<string> saveFileNames = GetAllSaveFileNames();
                
                // Save data for each save file name
                foreach (string saveFileName in saveFileNames)
                {
                    // Save the current case ID
                    PlayerPrefs.SetString($"MIS_{saveFileName}_CurrentCaseId", caseId);
                    
                    // Save the serialized data
                    PlayerPrefs.SetString($"MIS_{saveFileName}_SpawnedItems", spawnedItemsStr.ToString());
                    PlayerPrefs.SetString($"MIS_{saveFileName}_ItemSpawnEvents", itemSpawnEventsStr.ToString());
                    PlayerPrefs.SetString($"MIS_{saveFileName}_TriggeredRules", triggeredRulesStr.ToString());
                    PlayerPrefs.SetString($"MIS_{saveFileName}_TriggerCounts", triggerCountsStr.ToString());
                    
                    Plugin.LogDebug($"Saved tracking data for save file: {saveFileName}, case ID: {caseId}");
                }
                
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error saving tracking data: {ex.Message}");
            }
        }
        
        // Load tracking data from PlayerPrefs
        public void LoadTrackingData()
        {
            try
            {
                // Get all possible save file names
                List<string> saveFileNames = GetAllSaveFileNames();
                string usableSaveFileName = null;
                
                // Find the first save file name that has data
                foreach (string saveFileName in saveFileNames)
                {
                    if (PlayerPrefs.HasKey($"MIS_{saveFileName}_CurrentCaseId"))
                    {
                        usableSaveFileName = saveFileName;
                        break;
                    }
                }
                
                // If no save file has data, reset tracking and return
                if (usableSaveFileName == null)
                {
                    Plugin.LogDebug($"No saved tracking data found for any save files: {string.Join(", ", saveFileNames)}");
                    ResetTracking(); // Reset tracking for a new save
                    return;
                }
                
                // Get the saved case ID
                string savedCaseId = PlayerPrefs.GetString($"MIS_{usableSaveFileName}_CurrentCaseId");
                
                // Get the current case ID from MurderController
                string currentCaseId = "";
                if (MurderController.Instance != null && MurderController.Instance.chosenMO != null)
                {
                    // Use the murder MO name as a case identifier
                    currentCaseId = MurderController.Instance.chosenMO.name;
                }
                
                // Check if this is the same case
                if (!string.IsNullOrEmpty(currentCaseId) && savedCaseId != currentCaseId)
                {
                    Plugin.LogDebug($"New case detected (old: {savedCaseId}, new: {currentCaseId}). Resetting tracking.");
                    ResetTracking();
                    return;
                }
                
                // Clear existing data before loading
                spawnedItems.Clear();
                itemSpawnEvents.Clear();
                triggeredRules.Clear();
                triggerCounts.Clear();
                
                // Load the spawned items
                if (PlayerPrefs.HasKey($"MIS_{usableSaveFileName}_SpawnedItems"))
                {
                    string spawnedItemsStr = PlayerPrefs.GetString($"MIS_{usableSaveFileName}_SpawnedItems");
                    string[] items = spawnedItemsStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string item in items)
                    {
                        string[] parts = item.Split(':');
                        if (parts.Length == 2)
                        {
                            spawnedItems[parts[0]] = parts[1] == "1";
                        }
                    }
                }
                
                // Load the item spawn events
                if (PlayerPrefs.HasKey($"MIS_{usableSaveFileName}_ItemSpawnEvents"))
                {
                    string itemSpawnEventsStr = PlayerPrefs.GetString($"MIS_{usableSaveFileName}_ItemSpawnEvents");
                    string[] events = itemSpawnEventsStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string evt in events)
                    {
                        string[] parts = evt.Split(':');
                        if (parts.Length == 2)
                        {
                            itemSpawnEvents[parts[0]] = parts[1];
                        }
                    }
                }
                
                // Load the triggered rules
                if (PlayerPrefs.HasKey($"MIS_{usableSaveFileName}_TriggeredRules"))
                {
                    string triggeredRulesStr = PlayerPrefs.GetString($"MIS_{usableSaveFileName}_TriggeredRules");
                    string[] rules = triggeredRulesStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string rule in rules)
                    {
                        string[] parts = rule.Split(':');
                        if (parts.Length == 2)
                        {
                            triggeredRules[parts[0]] = parts[1] == "1";
                        }
                    }
                }
                
                // Load the trigger counts
                if (PlayerPrefs.HasKey($"MIS_{usableSaveFileName}_TriggerCounts"))
                {
                    string triggerCountsStr = PlayerPrefs.GetString($"MIS_{usableSaveFileName}_TriggerCounts");
                    string[] counts = triggerCountsStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string count in counts)
                    {
                        string[] parts = count.Split(':');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int countValue))
                        {
                            triggerCounts[parts[0]] = countValue;
                        }
                    }
                }
                
                // Save the current case ID
                this.currentCaseId = currentCaseId;
                
                // If we loaded from DEFAULT_SAVE but we're using a real save name now,
                // make sure to save the data under the real save name too
                string currentSaveFileName = GetCurrentSaveFileName();
                if (usableSaveFileName != currentSaveFileName)
                {
                    Plugin.LogDebug($"Migrating tracking data from {usableSaveFileName} to {currentSaveFileName}");
                    SaveTrackingData(); // This will save to all save file names
                }
                
                Plugin.LogDebug($"Loaded tracking data for save file: {usableSaveFileName}, case ID: {this.currentCaseId}");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error loading tracking data: {ex.Message}");
                // Reset tracking in case of error
                ResetTracking();
            }
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
        
                // First pass: Process rules that don't require prior items
                foreach (var rule in config.SpawnRules.Where(r => !r.RequiresPriorItem))
                {
                    // Skip if rule is disabled
                    if (!rule.Enabled)
                        continue;
                
                    // First check if this item has already been spawned (for OnlySpawnOnce rules)
                    if (rule.OnlySpawnOnce && spawnedItems.ContainsKey(rule.Name) && spawnedItems[rule.Name])
                    {
                        Plugin.LogDebug($"Skipping rule '{rule.Name}' for event '{eventName}' because the item has already been spawned (OnlySpawnOnce=true)");
                        continue;
                    }
                    
                    // If OnlySpawnOnce is true, check if any event has already triggered this rule
                    if (rule.OnlySpawnOnce)
                    {
                        bool alreadyTriggered = false;
                        foreach (string triggerEvent in rule.TriggerEvents)
                        {
                            string checkKey = $"{rule.Name}_{triggerEvent}";
                            if (triggeredRules.ContainsKey(checkKey) && triggeredRules[checkKey])
                            {
                                alreadyTriggered = true;
                                break;
                            }
                        }
                
                        if (alreadyTriggered)
                        {
                            Plugin.LogDebug($"Skipping rule '{rule.Name}' for event '{eventName}' because it has already been triggered (OnlySpawnOnce=true)");
                            continue;
                        }
                    }

                    // Check if this rule should be triggered
                    if (rule.TriggerEvents.Contains(eventName) && 
                        (string.IsNullOrEmpty(rule.MurderMO) || rule.MurderMO == murderType))
                    {
                        // Start the spawn process
                        if (config.ShowDebugMessages)
                        {
                            Plugin.Log.LogInfo($"Rule '{rule.Name}' triggered by event '{eventName}' with murder type '{murderType}'");
                        }
                        
                        // Check if this rule requires multiple triggers
                        bool shouldSpawn = true;
                        if (rule.RequiresMultipleTriggers && rule.RequiredTriggerCount > 1)
                        {
                            // Get the key for tracking this rule's trigger count
                            string triggerCountKey = $"{rule.Name}_{eventName}";
                            
                            // Get the current trigger count or initialize to 0
                            int currentCount = 0;
                            if (triggerCounts.ContainsKey(triggerCountKey))
                            {
                                currentCount = triggerCounts[triggerCountKey];
                            }
                            
                            // Increment the trigger count
                            currentCount++;
                            triggerCounts[triggerCountKey] = currentCount;
                            
                            // Check if we've reached the required count
                            if (currentCount < rule.RequiredTriggerCount)
                            {
                                Plugin.LogDebug($"Rule '{rule.Name}' for event '{eventName}' has been triggered {currentCount}/{rule.RequiredTriggerCount} times");
                                shouldSpawn = false;
                            }
                            else
                            {
                                Plugin.LogDebug($"Rule '{rule.Name}' for event '{eventName}' has reached required trigger count ({currentCount}/{rule.RequiredTriggerCount})");
                            }
                        }
                        
                        // Only spawn if we should
                        bool spawnSuccessful = false;
                        if (shouldSpawn)
                        {
                            // Schedule the spawn with the specified delay
                            // Only mark the rule as triggered if the spawn was successful
                            spawnSuccessful = SpawnItem(rule, eventName);
                        }
                
                        if (spawnSuccessful)
                        {
                            // Mark the current event as triggered for this rule
                            // This prevents the same event from triggering the rule multiple times
                            string currentEventKey = $"{rule.Name}_{eventName}";
                            triggeredRules[currentEventKey] = true;
                            Plugin.LogDebug($"Rule '{rule.Name}' marked as triggered for event '{eventName}'");
                    
                        // If OnlySpawnOnce is true, also mark all other trigger events for this rule
                        if (rule.OnlySpawnOnce)
                        {
                            // Mark all other possible trigger events for this rule as triggered
                            foreach (string triggerEvent in rule.TriggerEvents)
                            {
                                // Skip the current event as it's already marked above
                                if (triggerEvent == eventName)
                                    continue;
                                
                                string triggerKey = $"{rule.Name}_{triggerEvent}";
                                triggeredRules[triggerKey] = true;
                                Plugin.LogDebug($"Rule '{rule.Name}' marked as triggered for event '{triggerEvent}' (due to OnlySpawnOnce=true)");
                        }
                    }
                }
            }
        }
        
            // Second pass: Process rules that require prior items
            foreach (var rule in config.SpawnRules.Where(r => r.RequiresPriorItem))
            {
                // Skip if rule is disabled
                if (!rule.Enabled)
                    continue;
                
                // If OnlySpawnOnce is true, check if any event has already triggered this rule
                if (rule.OnlySpawnOnce)
                {
                    bool alreadyTriggered = false;
                    foreach (string triggerEvent in rule.TriggerEvents)
                    {
                        string checkKey = $"{rule.Name}_{triggerEvent}";
                        if (triggeredRules.ContainsKey(checkKey) && triggeredRules[checkKey])
                        {
                            alreadyTriggered = true;
                            break;
                        }
                    }
                
                    if (alreadyTriggered)
                    {
                        Plugin.LogDebug($"Skipping rule '{rule.Name}' for event '{eventName}' because it has already been triggered (OnlySpawnOnce=true)");
                        continue;
                    }
                }

                // Check if this rule should be triggered
                if (rule.TriggerEvents.Contains(eventName) && 
                    (string.IsNullOrEmpty(rule.MurderMO) || rule.MurderMO == murderType))
                    {
                    // Start the spawn process
                    if (config.ShowDebugMessages)
                    {
                        Plugin.Log.LogInfo($"Rule '{rule.Name}' triggered by event '{eventName}' with murder type '{murderType}'");
                    }
                    
                    // Check if this rule requires multiple triggers
                    bool shouldSpawn = true;
                    if (rule.RequiresMultipleTriggers && rule.RequiredTriggerCount > 1)
                    {
                        // Get the key for tracking this rule's trigger count
                        string triggerCountKey = $"{rule.Name}_{eventName}";
                        
                        // Get the current trigger count or initialize to 0
                        int currentCount = 0;
                        if (triggerCounts.ContainsKey(triggerCountKey))
                        {
                            currentCount = triggerCounts[triggerCountKey];
                        }
                        
                        // Increment the trigger count
                        currentCount++;
                        triggerCounts[triggerCountKey] = currentCount;
                        
                        // Check if we've reached the required count
                        if (currentCount < rule.RequiredTriggerCount)
                        {
                            Plugin.LogDebug($"Rule '{rule.Name}' for event '{eventName}' has been triggered {currentCount}/{rule.RequiredTriggerCount} times");
                            shouldSpawn = false;
                        }
                        else
                        {
                            Plugin.LogDebug($"Rule '{rule.Name}' for event '{eventName}' has reached required trigger count ({currentCount}/{rule.RequiredTriggerCount})");
                        }
                    }
                    
                    // Only spawn if we should
                    bool spawnSuccessful = false;
                    if (shouldSpawn)
                    {
                        // Schedule the spawn with the specified delay
                        // Only mark the rule as triggered if the spawn was successful
                        spawnSuccessful = SpawnItem(rule, eventName);
                    }
                
                    if (spawnSuccessful)
                    {
                        // Mark the current event as triggered for this rule
                        // This prevents the same event from triggering the rule multiple times
                        string currentEventKey = $"{rule.Name}_{eventName}";
                        triggeredRules[currentEventKey] = true;
                        Plugin.LogDebug($"Rule '{rule.Name}' marked as triggered for event '{eventName}'");
                    
                        // If OnlySpawnOnce is true, also mark all other trigger events for this rule
                        if (rule.OnlySpawnOnce)
                        {
                            // Mark all other possible trigger events for this rule as triggered
                            foreach (string triggerEvent in rule.TriggerEvents)
                            {
                                // Skip the current event as it's already marked above
                                if (triggerEvent == eventName)
                                    continue;
                                
                                string triggerKey = $"{rule.Name}_{triggerEvent}";
                                triggeredRules[triggerKey] = true;
                                Plugin.LogDebug($"Rule '{rule.Name}' marked as triggered for event '{triggerEvent}' (due to OnlySpawnOnce=true)");
                            }
                        }
                    }
                }
            }
        }
    }


        // Spawn an item based on a rule
        // Returns true if the item was spawned, false if it was skipped
        private bool SpawnItem(SpawnRule rule, string eventName = "")
        {
            try
            {
                // Check if this item should only be spawned once and has already been spawned
                if (rule.OnlySpawnOnce && spawnedItems.ContainsKey(rule.Name) && spawnedItems[rule.Name])
                {
                    Plugin.LogDebug($"Skipping spawn for rule '{rule.Name}': Item has already been spawned once");
                    return false;
                }
                
                // Check if this item requires another item to be spawned first
                if (rule.RequiresPriorItem && !string.IsNullOrEmpty(rule.RequiredPriorItem))
                {
                    // Check if the required item exists
                    if (!spawnedItems.ContainsKey(rule.RequiredPriorItem) || !spawnedItems[rule.RequiredPriorItem])
                    {
                        Plugin.LogDebug($"Skipping spawn for rule '{rule.Name}': Required prior item '{rule.RequiredPriorItem}' has not been spawned yet");
                        return false;
                    }
                    
                    // If we require a separate trigger and the prior item was spawned in this same event, skip
                    if (rule.RequiresSeparateTrigger && !string.IsNullOrEmpty(eventName) && 
                        itemSpawnEvents.ContainsKey(rule.RequiredPriorItem) && 
                        itemSpawnEvents[rule.RequiredPriorItem] == eventName)
                    {
                        Plugin.LogDebug($"Skipping spawn for rule '{rule.Name}': Required prior item '{rule.RequiredPriorItem}' was spawned in the same event ('{eventName}') and RequiresSeparateTrigger is true");
                        return false;
                    }
                    
                    Plugin.LogDebug($"Required prior item '{rule.RequiredPriorItem}' found, proceeding with spawn for rule '{rule.Name}'");
                }

                // Get the item owner based on the BelongsTo property
                Human itemOwner = GetOwner(rule.BelongsTo);
                if (itemOwner == null)
                {
                    Plugin.Log.LogInfo($"Cannot spawn item for rule '{rule.Name}': No valid owner found");
                    return false;
                }

                // Get the recipient (spawn location reference) based on the ItemRecipient property
                Human spawnLocationRecipient = GetRecipient(rule.Recipient);
                if (spawnLocationRecipient == null)
                {
                    Plugin.Log.LogInfo($"Cannot spawn item for rule '{rule.Name}': No valid recipient found");
                    return false;
                }
                
                // Check trait matching if enabled
                if (rule.UseTraits && rule.TraitModifiers != null && rule.TraitModifiers.Count > 0)
                {
                    Plugin.LogDebug($"Checking trait matching for rule '{rule.Name}'...");
                    
                    bool allTraitModifiersMatch = true;
                    
                    foreach (var traitModifier in rule.TraitModifiers)
                    {
                        // Get the human to check traits for
                        Human humanToCheck = null;
                        
                        switch (traitModifier.Who)
                        {
                            case BelongsTo.Murderer:
                                humanToCheck = MurderController.Instance?.currentMurderer;
                                break;
                            case BelongsTo.Victim:
                                humanToCheck = MurderController.Instance?.currentVictim;
                                break;
                            case BelongsTo.Player:
                                humanToCheck = Player.Instance;
                                break;
                            case BelongsTo.MurdererDoctor:
                                if (MurderController.Instance?.currentMurderer != null)
                                {
                                    humanToCheck = MurderController.Instance.currentMurderer.GetDoctor();
                                }
                                break;
                            case BelongsTo.VictimDoctor:
                                if (MurderController.Instance?.currentVictim != null)
                                {
                                    humanToCheck = MurderController.Instance.currentVictim.GetDoctor();
                                }
                                break;
                            case BelongsTo.MurdererLandlord:
                                if (MurderController.Instance?.currentMurderer != null)
                                {
                                    humanToCheck = MurderController.Instance.currentMurderer.GetLandlord();
                                }
                                break;
                            case BelongsTo.VictimLandlord:
                                if (MurderController.Instance?.currentVictim != null)
                                {
                                    humanToCheck = MurderController.Instance.currentVictim.GetLandlord();
                                }
                                break;
                            default:
                                Plugin.LogDebug($"Unknown trait modifier Who value: {traitModifier.Who}");
                                break;
                        }
                        
                        if (humanToCheck == null)
                        {
                            Plugin.LogDebug($"Cannot check traits: Human not found for {traitModifier.Who}");
                            allTraitModifiersMatch = false;
                            break;
                        }
                        
                        // Check if the human's traits match the specified rule
                        bool traitsMatch = CheckTraitMatch(humanToCheck, traitModifier);
                        
                        if (!traitsMatch)
                        {
                            Plugin.LogDebug($"Trait matching failed for {traitModifier.Who}");
                            allTraitModifiersMatch = false;
                            break;
                        }
                    }
                    
                    if (!allTraitModifiersMatch)
                    {
                        Plugin.LogDebug($"Skipping spawn for rule '{rule.Name}': Trait matching failed");
                        return false;
                    }
                    
                    Plugin.LogDebug($"All trait modifiers matched for rule '{rule.Name}', proceeding with spawn");
                }

                // Get the spawn location using the recipient as reference
                Interactable spawnLocation = GetSpawnLocation(rule, itemOwner, spawnLocationRecipient);
                if (spawnLocation == null && rule.SpawnLocation == SpawnLocationType.Mailbox)
                {
                    Plugin.Log.LogInfo($"Cannot spawn item for rule '{rule.Name}': No valid spawn location found");
                    return false;
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
                            rule.SpawnChance,
                            rule.UseMultipleOwners,        // Whether to use multiple owners
                            rule.Owners                    // List of additional owners
                        );
                        break;
                        
                    case SpawnLocationType.HomeLobby:
                        // Use the lobby spawner for lobby locations
                        SpawnItemLobbyHome.SpawnItemAtLocation(
                            itemOwner,                    // Owner of the item
                            spawnLocationRecipient,        // Recipient used for spawn location reference
                            rule.ItemToSpawn,
                            rule.SpawnChance,
                            rule.UseMultipleOwners,        // Whether to use multiple owners
                            rule.Owners                    // List of additional owners
                        );
                        break;

                    case SpawnLocationType.WorkplaceLobby:
                        // Use the lobby spawner for lobby locations
                        SpawnItemLobbyWork.SpawnItemAtLocation(
                            itemOwner,                    // Owner of the item
                            spawnLocationRecipient,        // Recipient used for spawn location reference
                            rule.ItemToSpawn,
                            rule.SpawnChance,
                            rule.SubLocationTypeBuildingEntrances,
                            rule.UseMultipleOwners,        // Whether to use multiple owners
                            rule.Owners                    // List of additional owners
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

                    case SpawnLocationType.HotelRooftopBar:
                        SpawnItemHotelRooftopBar.SpawnItemAtLocation(
                            itemOwner,                    // Owner of the item
                            spawnLocationRecipient,        // Recipient used for spawn location reference
                            rule.ItemToSpawn,
                            rule.SpawnChance,
                            rule.HotelRooftopBarSubLocations
                        );
                        break;
                        
                    case SpawnLocationType.Random:
                        // Check if we have any random locations defined
                        if (rule.RandomSpawnLocations == null || rule.RandomSpawnLocations.Count == 0)
                        {
                            Plugin.Log.LogWarning($"[ConfigManager] Random spawn location selected but no RandomSpawnLocations defined in rule '{rule.Name}'");
                            return false;
                        }
                        
                        // Choose a random location from the pool
                        int randomIndex = UnityEngine.Random.Range(0, rule.RandomSpawnLocations.Count);
                        string randomLocationName = rule.RandomSpawnLocations[randomIndex];
                        
                        // Try to parse the location name to a SpawnLocationType
                        if (!Enum.TryParse(randomLocationName, out SpawnLocationType randomLocationType))
                        {
                            Plugin.Log.LogWarning($"[ConfigManager] Invalid random location name '{randomLocationName}' in rule '{rule.Name}'");
                            return false;
                        }
                        
                       // Plugin.Log.LogInfo($"[ConfigManager] Randomly selected location: {randomLocationType} for rule '{rule.Name}'");
                        
                        // Create a copy of the rule with the randomly selected location
                        SpawnRule randomRule = new SpawnRule
                        {
                            Name = rule.Name + "_Random",
                            ItemToSpawn = rule.ItemToSpawn,
                            BelongsTo = rule.BelongsTo,
                            Recipient = rule.Recipient,
                            SpawnLocation = randomLocationType,
                            SpawnChance = rule.SpawnChance,
                            UnlockMailbox = rule.UnlockMailbox,
                            SubLocationTypeBuildingEntrances = rule.SubLocationTypeBuildingEntrances
                        };
                        
                        // Recursively call SpawnItem with the new rule
                        return SpawnItem(randomRule);

                        case SpawnLocationType.CityHallBathroom:
                            SpawnItemCityHallBathroom.SpawnItemAtLocation(
                                itemOwner,
                                spawnLocationRecipient,
                                rule.ItemToSpawn,
                                rule.SpawnChance
                            );
                            break;
                            
                        case SpawnLocationType.Custom:
                            // All parameters are now optional - we can spawn in any building, any room, any floor
                            SpawnItemCustomBuilding.SpawnItemAtLocation(
                                itemOwner,                    // Owner of the item
                                spawnLocationRecipient,        // Recipient used for spawn location reference
                                rule.ItemToSpawn,
                                rule.SpawnChance,
                                rule.CustomRoomName,           // Room name is now optional (legacy)
                                rule.CustomBuildingPreset,     // Building preset is now optional
                                rule.CustomFloorNames,
                                rule.CustomSubRoomName,        // Sub-room name is optional (legacy)
                                rule.CustomRoomPreset,         // Room preset is optional (legacy)
                                rule.CustomSubRoomPreset,      // Sub-room preset is optional (legacy)
                                rule.CustomRoomNames,          // List of room names (new)
                                rule.CustomRoomPresets,        // List of room presets (new)
                                rule.CustomSubRoomNames,       // List of sub-room names (new)
                                rule.CustomSubRoomPresets,     // List of sub-room presets (new)
                                rule.UseFurniture,             // Whether to use furniture for item placement
                                rule.FurniturePresets          // List of furniture presets to look for
                            );
                            break;
                            
                        case SpawnLocationType.Home:
                            // Spawn item in the recipient's home, but owned by the owner
                            SpawnItemHome.SpawnItemAtLocation(
                                itemOwner,                    // Owner of the item
                                spawnLocationRecipient,        // Recipient (whose home will be used for spawn location)
                                rule.ItemToSpawn,              // Item to spawn
                                rule.SpawnChance,              // Chance to spawn
                                rule.CustomRoomName,           // Optional target room name
                                rule.UseFurniture,             // Whether to use furniture for item placement
                                rule.FurniturePresets          // List of furniture presets to look for
                            );
                            break;
                            
                        case SpawnLocationType.Workplace:
                            // Spawn item in the recipient's workplace, but owned by the owner
                            SpawnItemWork.SpawnItemAtLocation(
                                itemOwner,                    // Owner of the item
                                spawnLocationRecipient,        // Recipient (whose workplace will be used for spawn location)
                                rule.ItemToSpawn,              // Item to spawn
                                rule.SpawnChance,              // Chance to spawn
                                rule.CustomRoomName,           // Optional target room name
                                rule.UseFurniture,             // Whether to use furniture for item placement
                                rule.FurniturePresets,         // List of furniture presets to look for
                                rule.UseMultipleOwners,        // Whether to use multiple owners
                                rule.Owners                    // List of additional owners
                            );
                            break;
                            
                        default:
                            // Default to mailbox spawner for now
                            Plugin.Log.LogInfo($"Using mailbox spawner for location type by default: {rule.SpawnLocation}");
                            if (spawnLocation != null)
                            {
                                SpawnItemMailbox.SpawnItemAtLocation(
                                    itemOwner,                    // Owner of the item
                                    spawnLocationRecipient,       // Recipient used for spawn location reference
                                    spawnLocation,                // The actual spawn location
                                    rule.ItemToSpawn,
                                    rule.UnlockMailbox,
                                    rule.SpawnChance
                                );
                            }
                            break;
                    }
                    
                    // Mark the item as spawned if OnlySpawnOnce is true
                    if (rule.OnlySpawnOnce)
                    {
                        spawnedItems[rule.Name] = true;
                        Plugin.Log.LogInfo($"Marked item from rule '{rule.Name}' as spawned (OnlySpawnOnce=true)");
                    }
                    
                    // Record which event spawned this item
                    if (!string.IsNullOrEmpty(eventName))
                    {
                        itemSpawnEvents[rule.Name] = eventName;
                        Plugin.Log.LogInfo($"Item from rule '{rule.Name}' was spawned by event '{eventName}'");
                    }
                    
                    return true;
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogError($"Error spawning item: {ex.Message}");
                    return false;
                }
        }

        // Get the recipient based on the rule type
        private Human GetOwner(BelongsTo belongsTo)
        {
            return GetOwnerForFingerprint(belongsTo);
        }
        
        // Get the Human object for a BelongsTo enum value (for fingerprints)
        public Human GetOwnerForFingerprint(BelongsTo belongsTo)
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
                        //Plugin.Log.LogInfo($"[ConfigManager] Checking doormat location for {belongsTo.name} at {recipient.home.name}");
                        return null; // Just return null, actual spawning will happen in SpawnItem
                    }
                    
                    Plugin.Log.LogWarning($"[ConfigManager] Cannot spawn item in doormat: Recipient or home address is null for {recipient?.name}");
                    return null;
                
                case SpawnLocationType.HomeLobby:
                    if (recipient != null && recipient.home != null)
                    {
                        //Plugin.Log.LogInfo($"[ConfigManager] Checking lobby location for {belongsTo.name} at {recipient.home.name}");
                        return null; // Just return null, actual spawning will happen in SpawnItem
                    }
                    return null;
                
                case SpawnLocationType.HomeBuildingEntrance:
                    if (recipient != null && recipient.home != null)
                    {
                        //Plugin.Log.LogInfo($"[ConfigManager] Checking building entrance location for {belongsTo.name} at {recipient.home.name}");
                        return null; // Just return null, actual spawning will happen in SpawnItem
                    }
                    return null;
                
                case SpawnLocationType.WorkplaceBuildingEntrance:
                    if (belongsTo != null && belongsTo.job != null && belongsTo.job.employer != null && belongsTo.job.employer.address != null)
                    {
                        //Plugin.Log.LogInfo($"[ConfigManager] Checking workplace building entrance location for {belongsTo.name} at {belongsTo.job.employer.address.name}");
                        return null; // Just return null, actual spawning will happen in SpawnItem
                    }
                    return null;
                case SpawnLocationType.CityHallBathroom:
                    if (belongsTo != null)
                    {
                        //Plugin.Log.LogInfo($"[ConfigManager] Checking city hall bathroom location for {belongsTo.name}");
                        return null; // Just return null, actual spawning will happen in SpawnItemCityHallBathroom
                    }
                    return null;
                case SpawnLocationType.HotelRooftopBar:
                    if (belongsTo != null)
                    {
                        //Plugin.Log.LogInfo($"[ConfigManager] Checking hotel rooftop bar location for {belongsTo.name}");
                        return null; // Just return null, actual spawning will happen in SpawnItemHotelRooftopBar
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
        
        // Check if a human's traits match the specified trait rule
        private bool CheckTraitMatch(Human human, TraitModifier traitModifier)
        {
            // If the human is null or has no traits, return false
            if (human == null || human.characterTraits == null || human.characterTraits.Count == 0)
            {
                Plugin.LogDebug($"Cannot check traits: Human is null or has no traits");
                return false;
            }
            
            // Debug: Output all traits for this human
            Plugin.LogDebug($"Checking traits for {human.name}:");
            foreach (var trait in human.characterTraits)
            {
                if (trait != null)
                {
                    Plugin.LogDebug($"- {trait.name}");
                }
            }
            
            // Get the list of trait names for this human
            List<string> humanTraits = new List<string>();
            foreach (var trait in human.characterTraits)
            {
                if (trait != null)
                {
                    humanTraits.Add(trait.name);
                }
            }
            
            // Check if any of the traits match
            if (traitModifier.Rule == TraitRule.IfAnyOfThese)
            {
                foreach (string traitName in traitModifier.TraitList)
                {
                    if (humanTraits.Contains(traitName))
                    {
                        Plugin.LogDebug($"Found matching trait: {traitName}");
                        return true;
                    }
                }
                
                // No matches found
                Plugin.LogDebug($"No matching traits found for IfAnyOfThese rule");
                return false;
            }
            // Check if all of the traits match
            else if (traitModifier.Rule == TraitRule.IfAllOfThese)
            {
                foreach (string traitName in traitModifier.TraitList)
                {
                    if (!humanTraits.Contains(traitName))
                    {
                        Plugin.LogDebug($"Missing required trait: {traitName}");
                        return false;
                    }
                }
                
                // All traits matched
                Plugin.LogDebug($"All required traits found for IfAllOfThese rule");
                return true;
            }
            // Check if none of the traits match
            else if (traitModifier.Rule == TraitRule.IfNoneOfThese)
            {
                foreach (string traitName in traitModifier.TraitList)
                {
                    if (humanTraits.Contains(traitName))
                    {
                        Plugin.LogDebug($"Found excluded trait: {traitName}");
                        return false;
                    }
                }
                
                // No excluded traits found
                Plugin.LogDebug($"No excluded traits found for IfNoneOfThese rule");
                return true;
            }
            
            // Default case (should never happen)
            Plugin.LogDebug($"Unknown trait rule: {traitModifier.Rule}");
            return false;
        }
    }
}