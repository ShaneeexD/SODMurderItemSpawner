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

        // Method to spawn an item in a custom building and room
        public static void SpawnItemAtLocation(Human owner, Human recipient, string presetName, float spawnChance, 
            string targetRoomName, string buildingPreset = null, List<string> customFloorNames = null)
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

                // Find the custom building and spawn the item
                Interactable spawnedItem = SpawnItemInCustomBuilding(interactablePresetItem, owner, recipient, presetName, targetRoomName, buildingPreset, customFloorNames);
                
                if (spawnedItem != null)
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Successfully spawned '{presetName}' in custom building. Item node: {spawnedItem.node.name}");
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Item '{presetName}' final world position: {spawnedItem.wPos}");
                }
                else
                {
                    Plugin.Log.LogError($"[SpawnItemCustomBuilding] Failed to spawn '{presetName}' in custom building");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemCustomBuilding] Error spawning item: {ex.Message}");
                Plugin.Log.LogError($"[SpawnItemCustomBuilding] Stack trace: {ex.StackTrace}");
            }
        }

        // Method to spawn an item in a custom building with specific room name
        private static Interactable SpawnItemInCustomBuilding(InteractablePreset itemPreset, Human owner, Human recipient, 
            string itemNameForLog, string targetRoomName, string buildingPreset = null, List<string> customFloorNames = null)
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
                
                // If no building preset is specified, match any building
                if (buildingPreset == null)
                {
                    isBuildingMatch = true;
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ ANY BUILDING: Checking rooms in '{location.name}'");
                }
                // Otherwise check if the location name contains the building preset
                else if (location.name != null && location.name.Contains(buildingPreset, StringComparison.OrdinalIgnoreCase))
                {
                    isBuildingMatch = true;
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ BUILDING NAME MATCH: Found building with name '{location.name}' containing '{buildingPreset}'");
                }
                // Then check if the building preset contains the specified preset
                else if (building.preset != null && building.preset.name != null)
                {
                    buildingPresetName = building.preset.name;
                    
                    if (buildingPresetName.Contains(buildingPreset, StringComparison.OrdinalIgnoreCase))
                    {
                        isBuildingMatch = true;
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ BUILDING PRESET MATCH: Found building '{location.name}', Preset: '{buildingPresetName}' contains '{buildingPreset}'");
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
                    
                    bool isRoomMatch = false;
                    bool isFloorMatch = true; // Default to true if no floor names are specified
                    
                    // Check if we need to filter by floor name
                    if (customFloorNames != null && customFloorNames.Count > 0)
                    {
                        isFloorMatch = false; // Start with false when we have floor names to check
                        
                        // Log the floor name we're checking
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Floor check - Room: {roomName}, Actual Floor: '{floorName}', Looking for floors: '{string.Join(", ", customFloorNames)}'");
                        
                        // Check if this room's floor matches any of the specified floor names
                        foreach (string customFloorName in customFloorNames)
                        {
                            // Use Contains instead of Equals for more flexible matching
                            if (floorName.Contains(customFloorName, StringComparison.OrdinalIgnoreCase))
                            {
                                isFloorMatch = true;
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ FLOOR MATCH: '{floorName}' contains '{customFloorName}'");
                                break;
                            }
                        }
                    }
                    
                    // Simple dynamic room matching using only location names
                    // Log the room name for debugging
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Room check - Room name: {roomName}, Looking for: '{targetRoomName}'");
                    
                    // Check if the room name contains the target name (case insensitive)
                    if (roomName.Contains(targetRoomName, StringComparison.OrdinalIgnoreCase))
                    {
                        isRoomMatch = true;
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ ROOM MATCH: Room name '{roomName}' contains '{targetRoomName}'");
                    }
                    
                    // Log the room
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Room: {roomName}, Preset: {presetName}, Floor: {floorName}, Building: {buildingName}, RoomMatch: {isRoomMatch}, FloorMatch: {isFloorMatch}");
                    
                    // Add matching rooms to our list
                    if (isRoomMatch && isFloorMatch)
                    {
                        matchingRooms.Add(room);
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] *** FOUND MATCHING ROOM: {roomName} in {buildingName} ***");
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
