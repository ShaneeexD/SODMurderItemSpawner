using System;
using System.Collections.Generic;
using SOD.Common;
using UnityEngine;
using System.Linq;

namespace MurderItemSpawner
{
    public class SpawnItemCustomBuilding : MonoBehaviour
    {
        private static SpawnItemCustomBuilding _instance;
        private static SpawnItemCustomBuilding Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SpawnItemCustomBuilding_Instance");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<SpawnItemCustomBuilding>();
                }
                return _instance;
            }
        }

        // Method to spawn an item in a custom building and room using coroutines to prevent freezing
        public static void SpawnItemAtLocation(Human owner, Human recipient, string presetName, float spawnChance, 
            string targetRoomName = null, string buildingPreset = null, List<string> customFloorNames = null, 
            string customSubRoomName = null, string customRoomPreset = null, string customSubRoomPreset = null)
        {
            try
            {
                // Check if we should spawn based on chance
                float randomValue = UnityEngine.Random.Range(0f, 1f);
                if (randomValue > spawnChance)
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Skipping spawn of {presetName} due to chance (roll: {randomValue}, needed: <= {spawnChance})");
                    return;
                }

                // Get the interactable preset
                InteractablePreset interactablePresetItem = Toolbox.Instance.GetInteractablePreset(presetName);
                if (interactablePresetItem == null)
                {
                    Plugin.Log.LogError($"[SpawnItemCustomBuilding] Could not find interactable preset with name {presetName}");
                    return;
                }

                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Owner: {owner.name}, Recipient: {recipient.name}");
                if (buildingPreset != null)
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Looking for building: {buildingPreset}, room name: {targetRoomName}");
                }
                else
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Looking for any building with room name: {targetRoomName}");
                }
                
                if (customFloorNames != null && customFloorNames.Count > 0)
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Using specific floor names: {string.Join(", ", customFloorNames)}");
                }
                else
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] No specific floor names provided, checking all floors");
                }
                
                if (customSubRoomName != null)
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Looking for sub-room with name: {customSubRoomName}");
                }
                
                if (customRoomPreset != null)
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Looking for room with preset: {customRoomPreset}");
                }
                
                if (customSubRoomPreset != null)
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Looking for sub-room with preset: {customSubRoomPreset}");
                }

                // Start the coroutine to find rooms and spawn the item
                UniverseLib.RuntimeHelper.StartCoroutine(FindRoomsCoroutine(interactablePresetItem, owner, recipient, presetName, 
                    targetRoomName, buildingPreset, customFloorNames, customSubRoomName, customRoomPreset, customSubRoomPreset));
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemCustomBuilding] Error spawning item: {ex.Message}");
                Plugin.Log.LogError($"[SpawnItemCustomBuilding] Stack trace: {ex.StackTrace}");
            }
        }
        
        // Coroutine to find rooms and spawn items asynchronously to prevent freezing
        private static System.Collections.IEnumerator FindRoomsCoroutine(InteractablePreset itemPreset, Human owner, Human recipient, 
            string itemNameForLog, string targetRoomName = null, string buildingPreset = null, List<string> customFloorNames = null, 
            string customSubRoomName = null, string customRoomPreset = null, string customSubRoomPreset = null)
        {
            // Find rooms in buildings with the specified preset
            List<NewRoom> matchingRooms = new List<NewRoom>();
            
            // Get all locations in the city
            CityData cityData = CityData.Instance;
            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Searching for buildings with preset: {buildingPreset}");
            
            // Collect all valid locations first
            List<NewGameLocation> locationsToProcess = new List<NewGameLocation>();
            
            try
            {
                foreach (var location in cityData.gameLocationDirectory)
                {
                    if (location != null && location.thisAsAddress != null)
                    {
                        locationsToProcess.Add(location);
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemCustomBuilding] Error collecting locations: {ex.Message}");
                yield break;
            }
            
            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Processing {locationsToProcess.Count} locations in batches");
            
            // Process locations in batches to prevent freezing
            int batchSize = 5; // Process 5 locations at a time
            int processedCount = 0;
            
            for (int i = 0; i < locationsToProcess.Count; i += batchSize)
            {
                int endIndex = Mathf.Min(i + batchSize, locationsToProcess.Count);
                
                // Process a batch of locations
                for (int j = i; j < endIndex; j++)
                {
                    var location = locationsToProcess[j];
                    ProcessLocationForRooms(location, buildingPreset, targetRoomName, customFloorNames,
                        customSubRoomName, customRoomPreset, customSubRoomPreset, matchingRooms);
                }
                
                processedCount += (endIndex - i);
                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Processed {processedCount}/{locationsToProcess.Count} locations. Yielding to prevent freezing...");
                
                // Yield after each batch to prevent freezing
                yield return null;
            }
            
            // After processing all locations, spawn the item if we found matching rooms
            if (matchingRooms.Count > 0)
            {
                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Found {matchingRooms.Count} matching rooms for spawning '{itemNameForLog}'");
                
                try
                {
                    // Select a random room from the matching rooms
                    int randomRoomIndex = UnityEngine.Random.Range(0, matchingRooms.Count);
                    NewRoom selectedRoom = matchingRooms[randomRoomIndex];
                    
                    string roomName = selectedRoom.name != null ? selectedRoom.name : "unnamed";
                    string buildingName = selectedRoom.building != null && selectedRoom.building.name != null ? 
                        selectedRoom.building.name : "unknown building";
                        
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Selected room: {roomName} in building: {buildingName}");
                    
                    Interactable spawnedItem = null;
                    
                    try
                    {
                        // Find a suitable node in the room for placement
                        List<NewNode> nodesList = new List<NewNode>();
                        NewNode placementNode = null;
                        Vector3 spawnPosition = selectedRoom.transform.position; // Default to room position
                        
                        // Get all nodes in the room
                        if (selectedRoom.nodes != null && selectedRoom.nodes.Count > 0)
                        {
                            foreach (var node in selectedRoom.nodes)
                            {
                                if (node == null) continue;
                                nodesList.Add(node);
                            }
                            
                            if (nodesList.Count > 0)
                            {
                                // Pick a random node in the room
                                int randomNodeIndex = UnityEngine.Random.Range(0, nodesList.Count);
                                placementNode = nodesList[randomNodeIndex];
                                spawnPosition = placementNode.position;
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Using node in room: {placementNode}");
                            }
                            else
                            {
                                Plugin.Log.LogWarning($"[SpawnItemCustomBuilding] No nodes found in selected room.");
                                yield break;
                            }
                        }
                        else
                        {
                            Plugin.Log.LogWarning($"[SpawnItemCustomBuilding] No nodes found in selected room.");
                            yield break;
                        }
                        
                        // Add a small offset to ensure it's visible
                        spawnPosition.y += 0.00f;
                        
                        // Add some randomization to the position
                        spawnPosition.x += UnityEngine.Random.Range(-0.1f, 0.1f);
                        spawnPosition.z += UnityEngine.Random.Range(-0.1f, 0.1f);
                        
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Calculated spawn position: {spawnPosition}");
                        
                        // Make sure we have a valid node for placement
                        if (placementNode == null)
                        {
                            Plugin.Log.LogWarning($"[SpawnItemCustomBuilding] Could not find a valid node for placement.");
                            yield break;
                        }
                        
                        // Create a list of passed variables for the room ID
                        Il2CppSystem.Collections.Generic.List<Interactable.Passed> passedVars = new Il2CppSystem.Collections.Generic.List<Interactable.Passed>();
                        passedVars.Add(new Interactable.Passed(Interactable.PassedVarType.roomID, placementNode.room.roomID, null));
                        
                        // Create a random rotation (0-360 degrees on Y axis)
                        float randomYRotation = UnityEngine.Random.Range(0f, 360f);
                        Vector3 randomRotation = new Vector3(0f, randomYRotation, 0f);
                        
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Using random rotation: {randomRotation}");
                        
                        // Create the item in the selected room
                        spawnedItem = InteractableCreator.Instance.CreateWorldInteractable(
                            itemPreset,                // The item preset
                            owner,                     // The owner of the item
                            owner,                     // The writer (same as owner)
                            recipient,                 // The receiver
                            spawnPosition,             // The position in the room
                            randomRotation,            // Random rotation on Y axis (0-360 degrees)
                            passedVars,                // Passed variables with room ID
                            null,                      // No passed object
                            ""                         // No load GUID
                        );
                        
                        if (spawnedItem != null)
                        {
                            // Set the node to the placement node
                            spawnedItem.node = placementNode;
                            
                            // Update the item's position and node
                            spawnedItem.UpdateWorldPositionAndNode(true, true);
                            
                            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Successfully spawned '{itemNameForLog}' in custom building. Item node: {spawnedItem.node.name}");
                            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Item '{itemNameForLog}' final world position: {spawnedItem.wPos}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.LogError($"[SpawnItemCustomBuilding] Error creating item: {ex.Message}");
                        Plugin.Log.LogError($"[SpawnItemCustomBuilding] Stack trace: {ex.StackTrace}");
                    }
                    
                    if (spawnedItem == null)
                    {
                        Plugin.Log.LogError($"[SpawnItemCustomBuilding] Failed to spawn '{itemNameForLog}' in custom building");
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogError($"[SpawnItemCustomBuilding] Error spawning item: {ex.Message}");
                }
            }
            else
            {
                Plugin.Log.LogError($"[SpawnItemCustomBuilding] No matching rooms found for spawning '{itemNameForLog}'");
            }
        }
        
        // Helper method to process a single location for rooms
        private static void ProcessLocationForRooms(NewGameLocation location, string buildingPreset, string targetRoomName,
            List<string> customFloorNames, string customSubRoomName, string customRoomPreset, string customSubRoomPreset,
            List<NewRoom> matchingRooms)
        {
            try
            {
                NewAddress building = location.thisAsAddress;
                
                // Check if this building matches by name or preset
                bool isBuildingMatch = false;
                string buildingPresetName = "unknown";
                
                if (buildingPreset == null)
                {
                    isBuildingMatch = true;
                    Plugin.Log.LogDebug($"[SpawnItemCustomBuilding] ANY BUILDING: Checking rooms in '{location.name}'");
                }
                // Otherwise check if the location name matches the building preset
                else if (location.name != null)
                {
                    // First try exact match
                    if (location.name.Equals(buildingPreset, StringComparison.OrdinalIgnoreCase))
                    {
                        isBuildingMatch = true;
                        Plugin.Log.LogDebug($"[SpawnItemCustomBuilding] EXACT BUILDING NAME MATCH: Found building with name '{location.name}' exactly matching '{buildingPreset}'");
                    }
                    // Then try word boundary matching
                    else if (IsWordBoundaryMatch(location.name, buildingPreset))
                    {
                        isBuildingMatch = true;
                        Plugin.Log.LogDebug($"[SpawnItemCustomBuilding] BUILDING NAME WORD MATCH: Found building with name '{location.name}' containing whole word '{buildingPreset}'");
                    }
                }
                // Then check if the building preset matches
                else if (building.preset != null && building.preset.name != null)
                {
                    buildingPresetName = building.preset.name;
                    
                    // First try exact match
                    if (buildingPresetName.Equals(buildingPreset, StringComparison.OrdinalIgnoreCase))
                    {
                        isBuildingMatch = true;
                        Plugin.Log.LogDebug($"[SpawnItemCustomBuilding] EXACT BUILDING PRESET MATCH: Found building '{location.name}', Preset: '{buildingPresetName}' exactly matching '{buildingPreset}'");
                    }
                    // Then try word boundary matching
                    else if (IsWordBoundaryMatch(buildingPresetName, buildingPreset))
                    {
                        isBuildingMatch = true;
                        Plugin.Log.LogDebug($"[SpawnItemCustomBuilding] BUILDING PRESET WORD MATCH: Found building '{location.name}', Preset: '{buildingPresetName}' containing whole word '{buildingPreset}'");
                    }
                }
                
                // Skip buildings that don't match the preset
                if (!isBuildingMatch) return;
                
                // Check if the building has rooms
                if (building.rooms == null || building.rooms.Count == 0)
                {
                    Plugin.Log.LogDebug($"[SpawnItemCustomBuilding] Building {building.name} has no rooms.");
                    return;
                }
                
                // Check all rooms in this building
                for (int i = 0; i < building.rooms.Count; i++)
                {
                    var room = building.rooms[i];
                    if (room == null) continue;
                    
                    string roomName = room.name != null ? room.name : "unnamed";
                    string presetName = room.preset != null ? room.preset.name : "no preset";
                    string floorName = room.floor != null ? room.floor.name : "unknown floor";
                    string buildingName = building.name != null ? building.name : "unknown building";
                    
                    // Filter out rooms with 'controller' in the name or ending with 'Null'
                    if (roomName.Contains("controller", StringComparison.OrdinalIgnoreCase) || 
                        roomName.EndsWith("Null", StringComparison.OrdinalIgnoreCase))
                    {
                        Plugin.Log.LogDebug($"[SpawnItemCustomBuilding] Filtering out room: {roomName} (contains 'controller' or ends with 'Null')");
                        continue; // Skip this room entirely
                    }
                    
                    // Check if this room matches the criteria
                    bool isRoomMatch = CheckRoomMatch(room, targetRoomName, customFloorNames, customRoomPreset);
                    bool isSubRoomMatch = CheckSubRoomMatch(room, customSubRoomName, customSubRoomPreset);
                    
                    // If the room matches all criteria, add it to the list
                    if (isRoomMatch && isSubRoomMatch)
                    {
                        matchingRooms.Add(room);
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] MATCH: Found matching room '{roomName}' in '{buildingName}', Floor: '{floorName}', Preset: '{presetName}'");
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemCustomBuilding] Error processing location: {ex.Message}");
            }
        }
        
        // Helper method to check if a room matches the criteria
        private static bool CheckRoomMatch(NewRoom room, string targetRoomName, List<string> customFloorNames, string customRoomPreset)
        {
            if (room == null) return false;
            
            string roomName = room.name != null ? room.name : "unnamed";
            string presetName = room.preset != null ? room.preset.name : "no preset";
            string floorName = room.floor != null ? room.floor.name : "unknown floor";
            
            // Check room name if specified
            bool isRoomNameMatch = true; // Default to true if no target room name is specified
            if (targetRoomName != null)
            {
                isRoomNameMatch = false; // Default to false if a target room name is specified
                
                // First try exact match
                if (roomName.Equals(targetRoomName, StringComparison.OrdinalIgnoreCase))
                {
                    isRoomNameMatch = true;
                    Plugin.Log.LogDebug($"[SpawnItemCustomBuilding] EXACT ROOM NAME MATCH: Room name '{roomName}' exactly matches '{targetRoomName}'");
                }
                // Then try word boundary matching
                else if (IsWordBoundaryMatch(roomName, targetRoomName))
                {
                    isRoomNameMatch = true;
                    Plugin.Log.LogDebug($"[SpawnItemCustomBuilding] ROOM NAME WORD MATCH: Room name '{roomName}' contains whole word '{targetRoomName}'");
                }
            }
            
            // Check room preset if specified
            bool isRoomPresetMatch = true; // Default to true if no room preset is specified
            if (customRoomPreset != null)
            {
                isRoomPresetMatch = false; // Default to false if a room preset is specified
                
                // First try exact match
                if (presetName.Equals(customRoomPreset, StringComparison.OrdinalIgnoreCase))
                {
                    isRoomPresetMatch = true;
                    Plugin.Log.LogDebug($"[SpawnItemCustomBuilding] EXACT ROOM PRESET MATCH: Room preset '{presetName}' exactly matches '{customRoomPreset}'");
                }
                // Then try word boundary matching
                else if (IsWordBoundaryMatch(presetName, customRoomPreset))
                {
                    isRoomPresetMatch = true;
                    Plugin.Log.LogDebug($"[SpawnItemCustomBuilding] ROOM PRESET WORD MATCH: Room preset '{presetName}' contains whole word '{customRoomPreset}'");
                }
            }
            
            // Check floor name if specified
            bool isFloorMatch = true; // Default to true if no floor names are specified
            if (customFloorNames != null && customFloorNames.Count > 0)
            {
                isFloorMatch = false; // Default to false if floor names are specified
                
                foreach (var customFloorName in customFloorNames)
                {
                    // First try exact match
                    if (floorName.Equals(customFloorName, StringComparison.OrdinalIgnoreCase))
                    {
                        isFloorMatch = true;
                        Plugin.Log.LogDebug($"[SpawnItemCustomBuilding] EXACT FLOOR NAME MATCH: Floor name '{floorName}' exactly matches '{customFloorName}'");
                        break;
                    }
                    // Then try word boundary matching
                    else if (IsWordBoundaryMatch(floorName, customFloorName))
                    {
                        isFloorMatch = true;
                        Plugin.Log.LogDebug($"[SpawnItemCustomBuilding] FLOOR NAME WORD MATCH: Floor name '{floorName}' contains whole word '{customFloorName}'");
                        break;
                    }
                }
            }
            
            // Room matches if it matches all specified criteria
            return isRoomNameMatch && isRoomPresetMatch && isFloorMatch;
        }
        
        // Helper method to check if a sub-room matches the criteria
        private static bool CheckSubRoomMatch(NewRoom room, string customSubRoomName, string customSubRoomPreset)
        {
            // If no sub-room criteria are specified, consider it a match
            if (customSubRoomName == null && customSubRoomPreset == null)
                return true;
            
            // For now, since we can't directly access sub-rooms, we'll consider the room itself
            // This is a simplification - in a real implementation, you would need to find a way to access sub-rooms
            // or modify the criteria to work with the available data structure
            
            // If sub-room name is specified, check if the room name contains it
            if (customSubRoomName != null)
            {
                string roomName = room.name != null ? room.name : "unnamed";
                
                // Check if the room name contains the sub-room name (as a partial match)
                if (!roomName.Contains(customSubRoomName, StringComparison.OrdinalIgnoreCase) && 
                    !IsWordBoundaryMatch(roomName, customSubRoomName))
                {
                    return false;
                }
                
                Plugin.Log.LogDebug($"[SpawnItemCustomBuilding] Using room name '{roomName}' as proxy for sub-room with name '{customSubRoomName}'");
            }
            
            // If sub-room preset is specified, check if the room preset contains it
            if (customSubRoomPreset != null)
            {
                string presetName = room.preset != null ? room.preset.name : "no preset";
                
                // Check if the room preset contains the sub-room preset (as a partial match)
                if (!presetName.Contains(customSubRoomPreset, StringComparison.OrdinalIgnoreCase) && 
                    !IsWordBoundaryMatch(presetName, customSubRoomPreset))
                {
                    return false;
                }
                
                Plugin.Log.LogDebug($"[SpawnItemCustomBuilding] Using room preset '{presetName}' as proxy for sub-room with preset '{customSubRoomPreset}'");
            }
            
            // If we got here, the room matches the sub-room criteria (or we're using the room as a proxy)
            return true;
        }

        // Helper method for smart matching that allows partial matches for common terms
        // but requires more precise matching for specific identifiers
        private static bool IsWordBoundaryMatch(string source, string target)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
                return false;
                
            // Convert both strings to lowercase for case-insensitive comparison
            string sourceLower = source.ToLower();
            string targetLower = target.ToLower();
            
            // First check if they're exactly equal
            if (sourceLower == targetLower)
                return true;
                
            // List of common terms that can use partial matching
            string[] commonTerms = new string[] { 
                "basement", "ground", "floor", "bathroom", "power", "room", "lobby", 
                "kitchen", "office", "bedroom", "living", "dining", "hotel", "apartment", "bar" 
            };
            
            // If the target is a common term, allow partial matching
            foreach (string term in commonTerms)
            {
                if (targetLower == term && sourceLower.Contains(term))
                    return true;
            }
            
            // For specific identifiers (containing separators like underscore),
            // use more precise matching
            if (targetLower.Contains("_") || targetLower.Contains(" ") || targetLower.Contains("-"))
            {
                // For compound terms like "CityHall_Top", require the whole string to match
                return sourceLower.Contains(targetLower);
            }
            
            // For other cases, check if source contains target as a whole word
            char[] separators = new char[] { ' ', '_', '-', '.', '/', '\\' };
            string[] words = sourceLower.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (string word in words)
            {
                if (word == targetLower)
                    return true;
            }
            
            return false;
        }
        
        // Method to spawn an item in a custom building with specific room name
        private static Interactable SpawnItemInCustomBuilding(InteractablePreset itemPreset, Human owner, Human recipient, 
            string itemNameForLog, string targetRoomName = null, string buildingPreset = null, List<string> customFloorNames = null, 
            string customSubRoomName = null, string customRoomPreset = null, string customSubRoomPreset = null)
        {
            // Find rooms in buildings with the specified preset
            List<NewRoom> matchingRooms = new List<NewRoom>();
            
            // Get all locations in the city
            CityData cityData = CityData.Instance;
            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Searching for buildings with preset: {buildingPreset}");
            
            // Search all locations in the game
            foreach (var location in cityData.gameLocationDirectory)
            {
                if (location == null || location.thisAsAddress == null) continue;
                
                NewAddress building = location.thisAsAddress;
                // Only log the location name for debugging purposes
                // This is the name of the location, not the preset
                Plugin.Log.LogDebug($"[SpawnItemCustomBuilding] Checking location: {location.name}");
                
                // Check if this building matches by name or preset
                bool isBuildingMatch = false;
                string buildingPresetName = "unknown";
                
                if (buildingPreset == null)
                {
                    isBuildingMatch = true;
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ ANY BUILDING: Checking rooms in '{location.name}'");
                }
                // Otherwise check if the location name matches the building preset
                else if (location.name != null)
                {
                    // First try exact match
                    if (location.name.Equals(buildingPreset, StringComparison.OrdinalIgnoreCase))
                    {
                        isBuildingMatch = true;
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ EXACT BUILDING NAME MATCH: Found building with name '{location.name}' exactly matching '{buildingPreset}'");
                    }
                    // Then try word boundary matching
                    else if (IsWordBoundaryMatch(location.name, buildingPreset))
                    {
                        isBuildingMatch = true;
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ BUILDING NAME WORD MATCH: Found building with name '{location.name}' containing whole word '{buildingPreset}'");
                    }
                }
                // Then check if the building preset matches
                else if (building.preset != null && building.preset.name != null)
                {
                    buildingPresetName = building.preset.name;
                    
                    // First try exact match
                    if (buildingPresetName.Equals(buildingPreset, StringComparison.OrdinalIgnoreCase))
                    {
                        isBuildingMatch = true;
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ EXACT BUILDING PRESET MATCH: Found building '{location.name}', Preset: '{buildingPresetName}' exactly matching '{buildingPreset}'");
                    }
                    // Then try word boundary matching
                    else if (IsWordBoundaryMatch(buildingPresetName, buildingPreset))
                    {
                        isBuildingMatch = true;
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ BUILDING PRESET WORD MATCH: Found building '{location.name}', Preset: '{buildingPresetName}' containing whole word '{buildingPreset}'");
                    }
                }
                
                // Skip buildings that don't match the preset
                if (!isBuildingMatch) continue;
                
                // Check if the building has rooms
                if (building.rooms == null || building.rooms.Count == 0)
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Building {building.name} has no rooms.");
                    continue;
                }
                
                // Check all rooms in this building
                for (int i = 0; i < building.rooms.Count; i++)
                {
                    var room = building.rooms[i];
                    if (room == null) continue;
                    
                    string roomName = room.name != null ? room.name : "unnamed";
                    string presetName = room.preset != null ? room.preset.name : "no preset";
                    string floorName = room.floor != null ? room.floor.name : "unknown floor";
                    string buildingName = building.name != null ? building.name : "unknown building";
                    
                    // Filter out rooms with 'controller' in the name or ending with 'Null'
                    if (roomName.Contains("controller", StringComparison.OrdinalIgnoreCase) || 
                        roomName.EndsWith("Null", StringComparison.OrdinalIgnoreCase))
                    {
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Filtering out room: {roomName} (contains 'controller' or ends with 'Null')");
                        continue; // Skip this room entirely
                    }
                    
                    bool isRoomMatch = false;
                    bool isRoomPresetMatch = true; // Default to true if no room preset is specified
                    bool isFloorMatch = false; // Default to true if no floor names are specified
                    
                    // Check if the room preset matches the customRoomPreset parameter
                    if (customRoomPreset != null && !string.IsNullOrEmpty(customRoomPreset))
                    {
                        isRoomPresetMatch = false; // Default to false if a room preset is specified
                        
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Room preset check - Room preset: {presetName}, Looking for: '{customRoomPreset}'");
                        
                        // Check if the room preset name matches the customRoomPreset parameter
                        if (presetName.Contains(customRoomPreset, StringComparison.OrdinalIgnoreCase))
                        {
                            isRoomPresetMatch = true;
                            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ ROOM PRESET MATCH: Room preset '{presetName}' contains '{customRoomPreset}'");
                        }
                        else
                        {
                            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✗ NO ROOM PRESET MATCH: Room preset '{presetName}' does not contain '{customRoomPreset}'");
                        }
                    }
                    
                    // Check if the floor matches any of the custom floor names
                    if (customFloorNames == null || customFloorNames.Count == 0)
                    {
                        // If no custom floor names are provided, match any floor
                        isFloorMatch = true;
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ ANY FLOOR: Using floor '{floorName}' (no specific floor requested)");
                    }
                    else
                    {
                        foreach (string customFloorName in customFloorNames)
                        {
                            // Check if the custom floor name has any capital letters
                            bool hasCapitalLetters = customFloorName.Any(char.IsUpper);
                            
                            if (hasCapitalLetters)
                            {
                                // For names with capital letters, require EXACT match (not case-sensitive)
                                // This handles cases like "CityHall_Top" vs "CityHall_Top (Floor 4)"
                                if (floorName.Contains(customFloorName, StringComparison.OrdinalIgnoreCase))
                                {
                                    isFloorMatch = true;
                                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ CAPITAL LETTER MATCH: '{floorName}' contains '{customFloorName}' (case insensitive)");
                                    break;
                                }
                                else
                                {
                                    // Log that we're NOT matching this floor
                                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✗ NO CAPITAL MATCH: Floor '{floorName}' does not contain '{customFloorName}'");
                                    // Important: Do NOT set isFloorMatch to false here, as we need to check all floor names
                                }
                            }
                            // For lowercase terms like "basement", allow partial matching
                            else if (floorName.Contains(customFloorName, StringComparison.OrdinalIgnoreCase))
                            {
                                isFloorMatch = true;
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ LOWERCASE MATCH: '{floorName}' contains '{customFloorName}'");
                                break;
                            }
                        }
                    }
                    
                    // Simple dynamic room matching using only location names
                    if (targetRoomName == null)
                    {
                        // If no target room name is specified, match any room
                        isRoomMatch = true;
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ ANY ROOM: Using room '{roomName}' (no specific room requested)");
                    }
                    else
                    {
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Room check - Room name: {roomName}, Looking for: '{targetRoomName}'");
                        
                        // Check if the target room name has any capital letters
                        bool hasCapitalLetters = targetRoomName.Any(char.IsUpper);
                        
                        if (hasCapitalLetters)
                        {
                            // For names with capital letters, require EXACT match (not case-sensitive)
                            // This handles cases like "Bathroom" vs "Bathroom (Stall 2)"
                            if (roomName.Contains(targetRoomName, StringComparison.OrdinalIgnoreCase))
                            {
                                isRoomMatch = true;
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ CAPITAL LETTER ROOM MATCH: '{roomName}' contains '{targetRoomName}' (case insensitive)");
                            }
                            else
                            {
                                // Log that we're NOT matching this room
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✗ NO CAPITAL ROOM MATCH: Room '{roomName}' does not contain '{targetRoomName}'");
                            }
                        }
                        // For lowercase terms like "bathroom", allow partial matching
                        else if (roomName.Contains(targetRoomName, StringComparison.OrdinalIgnoreCase))
                        {
                            isRoomMatch = true;
                            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ LOWERCASE ROOM MATCH: '{roomName}' contains '{targetRoomName}'");
                        }
                    }
                    
                    // Log the room
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Room: {roomName}, Preset: {presetName}, Floor: {floorName}, Building: {buildingName}, RoomMatch: {isRoomMatch}, FloorMatch: {isFloorMatch}");
                    
                    // Filter out rooms with 'controller' in the name or ending with 'Null'
                    bool isFilteredRoom = roomName.Contains("controller", StringComparison.OrdinalIgnoreCase) || 
                                          roomName.EndsWith("Null", StringComparison.OrdinalIgnoreCase);
                    
                    if (isFilteredRoom)
                    {
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Filtering out room: {roomName} (contains 'controller' or ends with 'Null')");
                    }
                    
                    // Add matching rooms to our list
                    if (isRoomMatch && isFloorMatch && isRoomPresetMatch && !isFilteredRoom)
                    {
                        matchingRooms.Add(room);
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] *** FOUND MATCHING ROOM: {roomName} in {buildingName} ***");
                    }
                    else if (!isRoomPresetMatch)
                    {
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Room {roomName} matches name and floor but not preset. Skipping.");
                    }
                }
            }
            
            // Log summary
            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Found {matchingRooms.Count} matching rooms");
            
            // If we couldn't find any matching rooms, give up
            if (matchingRooms.Count == 0)
            {
                Plugin.Log.LogError($"[SpawnItemCustomBuilding] No matching rooms found for building preset: {buildingPreset}, room name: {targetRoomName}");
                return null;
            }
            
            // Choose a random matching room
            int randomRoomIndex = UnityEngine.Random.Range(0, matchingRooms.Count);
            NewRoom selectedRoom = matchingRooms[randomRoomIndex];
            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Selected room: {selectedRoom.name}");
            
            // Check if we need to find a sub-room
            if (!string.IsNullOrEmpty(customSubRoomName))
            {
                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Looking for sub-room: {customSubRoomName}");
                
                // Get the location name prefix to find related rooms
                string locationPrefix = "";
                
                // Try to get the company name first
                if (selectedRoom.gameLocation != null && 
                    selectedRoom.gameLocation.thisAsAddress != null && 
                    selectedRoom.gameLocation.thisAsAddress.company != null && 
                    !string.IsNullOrEmpty(selectedRoom.gameLocation.thisAsAddress.company.name))
                {
                    locationPrefix = selectedRoom.gameLocation.thisAsAddress.company.name;
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Using company name as prefix: {locationPrefix}");
                }
                // If no company name, try to extract a prefix from the room name
                else if (!string.IsNullOrEmpty(selectedRoom.name))
                {
                    // Try to extract the prefix (everything before the last space)
                    int lastSpaceIndex = selectedRoom.name.LastIndexOf(' ');
                    if (lastSpaceIndex > 0)
                    {
                        locationPrefix = selectedRoom.name.Substring(0, lastSpaceIndex);
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Extracted prefix from room name: {locationPrefix}");
                    }
                }
                
                if (!string.IsNullOrEmpty(locationPrefix))
                {
                    // Find all rooms in the same building
                    List<NewRoom> subRooms = new List<NewRoom>();
                    
                    // Get the building
                    NewAddress building = selectedRoom.gameLocation?.thisAsAddress;
                    if (building != null && building.rooms != null)
                    {
                        // Check all rooms in this building
                        foreach (var room in building.rooms)
                        {
                            if (room == null || string.IsNullOrEmpty(room.name)) continue;
                            
                            // Filter out rooms with 'controller' in the name or ending with 'Null'
                            if (room.name.Contains("controller", StringComparison.OrdinalIgnoreCase) || 
                                room.name.EndsWith("Null", StringComparison.OrdinalIgnoreCase))
                            {
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Filtering out sub-room: {room.name} (contains 'controller' or ends with 'Null')");
                                continue;
                            }
                            
                            // Check if this room matches our sub-room criteria
                            bool hasCapitalLetters = customSubRoomName.Any(char.IsUpper);
                            bool isMatch = false;
                            bool isSubRoomPresetMatch = true; // Default to true if no sub-room preset is specified
                            
                            // Check if the sub-room preset matches the customSubRoomPreset parameter
                            if (customSubRoomPreset != null && !string.IsNullOrEmpty(customSubRoomPreset))
                            {
                                isSubRoomPresetMatch = false; // Default to false if a sub-room preset is specified
                                string roomPresetName = room.preset != null ? room.preset.name : "no preset";
                                
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Sub-room preset check - Room preset: {roomPresetName}, Looking for: '{customSubRoomPreset}'");
                                
                                // Check if the room preset name matches the customSubRoomPreset parameter
                                if (roomPresetName.Contains(customSubRoomPreset, StringComparison.OrdinalIgnoreCase))
                                {
                                    isSubRoomPresetMatch = true;
                                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ SUB-ROOM PRESET MATCH: Room preset '{roomPresetName}' contains '{customSubRoomPreset}'");
                                }
                                else
                                {
                                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✗ NO SUB-ROOM PRESET MATCH: Room preset '{roomPresetName}' does not contain '{customSubRoomPreset}'");
                                }
                            }
                            
                            // For names with capital letters, use more precise matching
                            if (hasCapitalLetters)
                            {
                                if (room.name.Contains(customSubRoomName, StringComparison.OrdinalIgnoreCase))
                                {
                                    isMatch = true;
                                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ SUB-ROOM CAPITAL MATCH: Found sub-room '{room.name}' containing '{customSubRoomName}'");
                                }
                            }
                            // For lowercase names, use more flexible matching
                            else if (room.name.Contains(customSubRoomName, StringComparison.OrdinalIgnoreCase))
                            {
                                isMatch = true;
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ SUB-ROOM LOWERCASE MATCH: Found sub-room '{room.name}' containing '{customSubRoomName}'");
                            }
                            
                            // Also check if the room name contains both the location prefix and the sub-room name
                            if (!isMatch && room.name.Contains(locationPrefix, StringComparison.OrdinalIgnoreCase) && 
                                room.name.Contains(customSubRoomName, StringComparison.OrdinalIgnoreCase))
                            {
                                isMatch = true;
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ SUB-ROOM PREFIX MATCH: Found sub-room '{room.name}' containing both prefix '{locationPrefix}' and '{customSubRoomName}'");
                            }
                            
                            if (isMatch && isSubRoomPresetMatch)
                            {
                                subRooms.Add(room);
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Added sub-room '{room.name}' to potential matches");
                            }
                            else if (isMatch && !isSubRoomPresetMatch)
                            {
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Sub-room '{room.name}' matches name but not preset. Skipping.");
                            }
                        }
                        
                        // If we found matching sub-rooms, use one of them instead
                        if (subRooms.Count > 0)
                        {
                            int randomSubRoomIndex = UnityEngine.Random.Range(0, subRooms.Count);
                            selectedRoom = subRooms[randomSubRoomIndex];
                            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Selected sub-room: {selectedRoom.name}");
                        }
                        else
                        {
                            Plugin.Log.LogWarning($"[SpawnItemCustomBuilding] No matching sub-rooms found for '{customSubRoomName}' in building with prefix '{locationPrefix}'. Using original room instead.");
                        }
                    }
                }
                else
                {
                    Plugin.Log.LogWarning($"[SpawnItemCustomBuilding] Could not determine location prefix for sub-room matching. Using original room instead.");
                }
            }
            
            // Find a node in the selected room
            NewNode placementNode = null;
            Vector3 spawnPosition = Vector3.zero;
            
            // Get a node from the room
            if (selectedRoom.nodes != null && selectedRoom.nodes.Count > 0)
            {
                // Convert HashSet to List for easier random selection
                List<NewNode> nodesList = new List<NewNode>();
                foreach (var node in selectedRoom.nodes)
                {
                    nodesList.Add(node);
                }
                
                if (nodesList.Count > 0)
                {
                    // Pick a random node in the room
                    int randomNodeIndex = UnityEngine.Random.Range(0, nodesList.Count);
                    placementNode = nodesList[randomNodeIndex];
                    spawnPosition = placementNode.position;
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Using node in room: {placementNode}");
                }
                else
                {
                    Plugin.Log.LogWarning($"[SpawnItemCustomBuilding] No nodes found in selected room.");
                    return null;
                }
            }
            else
            {
                Plugin.Log.LogWarning($"[SpawnItemCustomBuilding] No nodes found in selected room.");
                return null;
            }
            // Add a small offset to ensure it's visible
            spawnPosition.y += 0.00f;
            
            // Add some randomization to the position
            spawnPosition.x += UnityEngine.Random.Range(-0.1f, 0.1f);
            spawnPosition.z += UnityEngine.Random.Range(-0.1f, 0.1f);

            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Calculated spawn position: {spawnPosition}");

            // Make sure we have a valid node for placement
            if (placementNode == null)
            {
                Plugin.Log.LogWarning($"[SpawnItemCustomBuilding] Could not find a valid node for placement.");
                return null;
            }

            // Create a list of passed variables for the room ID
            Il2CppSystem.Collections.Generic.List<Interactable.Passed> passedVars = new Il2CppSystem.Collections.Generic.List<Interactable.Passed>();
            passedVars.Add(new Interactable.Passed(Interactable.PassedVarType.roomID, placementNode.room.roomID, null));

            try
            {
                // Create a random rotation (0-360 degrees on Y axis)
                float randomYRotation = UnityEngine.Random.Range(0f, 360f);
                Vector3 randomRotation = new Vector3(0f, randomYRotation, 0f);
                
                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Using random rotation: {randomRotation}");
                
                // Create the item in the selected room
                Interactable spawnedItem = InteractableCreator.Instance.CreateWorldInteractable(
                    itemPreset,                // The item preset
                    owner,                     // The owner of the item
                    owner,                     // The writer (same as owner)
                    recipient,                 // The receiver
                    spawnPosition,             // The position in the room
                    randomRotation,            // Random rotation on Y axis (0-360 degrees)
                    passedVars,                // Passed variables with room ID
                    null,                      // No passed object
                    ""                         // No load GUID
                );
                
                if (spawnedItem != null)
                {
                    // Set the node to the placement node
                    spawnedItem.node = placementNode;

                    // Update the item's position and node
                    spawnedItem.UpdateWorldPositionAndNode(true, true);
                    
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Successfully created item in custom location");
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Item position: {spawnedItem.wPos}, node: {(spawnedItem.node != null ? spawnedItem.node.ToString() : "null")}");
                    
                    return spawnedItem;
                }
                else
                {
                    Plugin.Log.LogError($"[SpawnItemCustomBuilding] Failed to create item in custom location");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemCustomBuilding] Error creating item: {ex.Message}");
                Plugin.Log.LogError($"[SpawnItemCustomBuilding] Stack trace: {ex.StackTrace}");
                return null;
            }
        }
    }
}
