using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MurderItemSpawner
{
    // Serializable class to store our tracking data
    [Serializable]
    public class MurderItemSpawnerSaveData
    {
        // Dictionary of spawned items (rule name -> bool)
        public List<SavedItem> SpawnedItems = new List<SavedItem>();
        
        // Dictionary of item spawn events (rule name -> event name)
        public List<SavedItemEvent> ItemSpawnEvents = new List<SavedItemEvent>();
        
        // Dictionary of triggered rules (rule_name_event -> bool)
        public List<SavedTriggeredRule> TriggeredRules = new List<SavedTriggeredRule>();
        
        // Current case ID to detect when a new case starts
        public string CurrentCaseId = "";
        
        // Convert from dictionary to serializable format
        public void SetFromDictionaries(Dictionary<string, bool> spawnedItems, Dictionary<string, string> itemSpawnEvents, Dictionary<string, bool> triggeredRules, string caseId)
        {
            SpawnedItems.Clear();
            foreach (var kvp in spawnedItems)
            {
                SpawnedItems.Add(new SavedItem { RuleName = kvp.Key, IsSpawned = kvp.Value });
            }
            
            ItemSpawnEvents.Clear();
            foreach (var kvp in itemSpawnEvents)
            {
                ItemSpawnEvents.Add(new SavedItemEvent { RuleName = kvp.Key, EventName = kvp.Value });
            }
            
            TriggeredRules.Clear();
            foreach (var kvp in triggeredRules)
            {
                TriggeredRules.Add(new SavedTriggeredRule { RuleEventKey = kvp.Key, IsTriggered = kvp.Value });
            }
            
            CurrentCaseId = caseId;
        }
        
        // Convert to dictionaries
        public void GetDictionaries(out Dictionary<string, bool> spawnedItems, out Dictionary<string, string> itemSpawnEvents, out Dictionary<string, bool> triggeredRules, out string caseId)
        {
            spawnedItems = new Dictionary<string, bool>();
            foreach (var item in SpawnedItems)
            {
                spawnedItems[item.RuleName] = item.IsSpawned;
            }
            
            itemSpawnEvents = new Dictionary<string, string>();
            foreach (var item in ItemSpawnEvents)
            {
                itemSpawnEvents[item.RuleName] = item.EventName;
            }
            
            triggeredRules = new Dictionary<string, bool>();
            foreach (var item in TriggeredRules)
            {
                triggeredRules[item.RuleEventKey] = item.IsTriggered;
            }
            
            caseId = CurrentCaseId;
        }
    }
    
    // Serializable classes for dictionary entries
    [Serializable]
    public class SavedItem
    {
        public string RuleName;
        public bool IsSpawned;
    }
    
    [Serializable]
    public class SavedItemEvent
    {
        public string RuleName;
        public string EventName;
    }
    
    [Serializable]
    public class SavedTriggeredRule
    {
        public string RuleEventKey;
        public bool IsTriggered;
    }
}
