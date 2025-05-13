using System;
using System.Collections;
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

                // Find the custom building and spawn the item
                CoroutineHelper.StartCoroutine(SpawnItemInCustomBuildingCoroutine(interactablePresetItem, owner, recipient, presetName, 
                    targetRoomName, buildingPreset, customFloorNames, customSubRoomName, customRoomPreset, customSubRoomPreset));
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemCustomBuilding] Error spawning item: {ex.Message}");
                Plugin.Log.LogError($"[SpawnItemCustomBuilding] Stack trace: {ex.StackTrace}");
            }
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
        
        private static IEnumerator SpawnItemInCustomBuildingCoroutine(InteractablePreset itemPreset, Human owner, Human recipient, string itemNameForLog, string targetRoomName = null, string buildingPreset = null, List<string> customFloorNames = null, string customSubRoomName = null, string customRoomPreset = null, string customSubRoomPreset = null)
            {
            List<NewRoom> matchingRooms = new List<NewRoom>();
            CityData cityData = CityData.Instance;
            
            int processedCount = 0;
            const int batchSize = 1; // Process 2 rooms before yielding

            // Iterate through ALL locations/buildings without pre-filtering
            foreach (var location in cityData.gameLocationDirectory)
            {
                if (location == null || location.thisAsAddress == null) continue;
                NewAddress building = location.thisAsAddress;
                
                // Ensure building has rooms
                if (building.rooms == null || building.rooms.Count == 0)
                {
                    // Optional: Log if needed
                    // Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Building {building.name ?? "(unnamed)"} has no rooms.");
                    continue;
                }

                // Iterate through rooms
                for (int i = 0; i < building.rooms.Count; i++)
                {
                    var room = building.rooms[i];
                    if (room == null) continue;

                    // Get room details
                    string roomName = room.name != null ? room.name : "unnamed";
                    string presetName = room.preset != null ? room.preset.name : "no preset";
                    string floorName = room.floor != null ? room.floor.name : "unknown floor";
                    string buildingName = building.name != null ? building.name : "unknown building"; // For logging

                    // Standard filtering
                    if (roomName.Contains("controller", StringComparison.OrdinalIgnoreCase) || 
                        roomName.EndsWith("Null", StringComparison.OrdinalIgnoreCase))
                    {
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Filtering out room: {roomName} (standard exclusion)");
                        continue;
                    }

                    // --- NEW: Prioritized Preliminary Check ---
                    bool criteriaProvided = !string.IsNullOrEmpty(targetRoomName) ||
                                            !string.IsNullOrEmpty(customRoomPreset) ||
                                            (customFloorNames != null && customFloorNames.Count > 0);

                    bool preliminaryMatch = !criteriaProvided; // Pass if no specific criteria given

                    if (criteriaProvided)
                    {
                        // Check if room matches AT LEAST ONE provided criterion
                        if (!string.IsNullOrEmpty(targetRoomName) && roomName.Contains(targetRoomName, StringComparison.OrdinalIgnoreCase))
                        {
                            preliminaryMatch = true;
                            // Plugin.Log.LogDebug($"[SpawnItemCustomBuilding] Preliminary match on Room Name: {roomName} contains {targetRoomName}");
                        }
                        else if (!string.IsNullOrEmpty(customRoomPreset) && presetName.Contains(customRoomPreset, StringComparison.OrdinalIgnoreCase))
                        {
                            preliminaryMatch = true;
                            // Plugin.Log.LogDebug($"[SpawnItemCustomBuilding] Preliminary match on Room Preset: {presetName} contains {customRoomPreset}");
                        }
                        else if (customFloorNames != null && customFloorNames.Count > 0)
                        {
                            foreach (string customFloorName in customFloorNames)
                            {
                                if (floorName.Contains(customFloorName, StringComparison.OrdinalIgnoreCase))
                                {
                                    preliminaryMatch = true;
                                    // Plugin.Log.LogDebug($"[SpawnItemCustomBuilding] Preliminary match on Floor Name: {floorName} contains {customFloorName}");
                                    break; 
                                }
                            }
                        }
                    }

                    if (criteriaProvided && !preliminaryMatch)
                    {
                        // Skip detailed checks if criteria were given but none matched preliminarily
                        // Plugin.Log.LogDebug($"[SpawnItemCustomBuilding] Skipping room {roomName}: No preliminary match on provided criteria.");
                        continue;
                    }
                    // --- END NEW: Prioritized Preliminary Check ---


                    // --- Detailed Checks (run if preliminary check passes or no criteria given) ---
                    bool isRoomMatch = false; 
                    bool isRoomPresetMatch = true; // Default true if not specified
                    bool isFloorMatch = false; 

                    // Detailed Room Preset Check (only if specified)
                    if (!string.IsNullOrEmpty(customRoomPreset))
                    {
                        isRoomPresetMatch = false; // Must explicitly match
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Detailed Room preset check - Room preset: {presetName}, Looking for: '{customRoomPreset}'");
                        if (presetName.Contains(customRoomPreset, StringComparison.OrdinalIgnoreCase))
                        {
                            isRoomPresetMatch = true;
                            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ Detailed ROOM PRESET MATCH: Room preset '{presetName}' contains '{customRoomPreset}'");
                        }
                        else
                        {
                            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✗ Detailed NO ROOM PRESET MATCH: Room preset '{presetName}' does not contain '{customRoomPreset}'");
                        }
                    }

                    // Detailed Floor Check (only if specified)
                    if (customFloorNames == null || customFloorNames.Count == 0)
                    {
                        isFloorMatch = true; // No specific floor requested
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ Detailed ANY FLOOR: Using floor '{floorName}' (no specific floor requested)");
                    }
                    else
                    {
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Detailed Floor check - Floor: {floorName}, Looking for: {string.Join(", ", customFloorNames)}");
                        foreach (string customFloorName in customFloorNames)
                        {
                            // Using simple Contains for now, retain original capital letter check if needed
                            if (floorName.Contains(customFloorName, StringComparison.OrdinalIgnoreCase))
                            {
                                isFloorMatch = true;
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ Detailed FLOOR MATCH: '{floorName}' contains '{customFloorName}'");
                                break;
                            }
                        }
                         if (!isFloorMatch) Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✗ Detailed NO FLOOR MATCH: Floor '{floorName}' does not match any requested floor.");
                    }

                    // Detailed Room Name Check (only if specified)
                    if (string.IsNullOrEmpty(targetRoomName))
                    {
                        isRoomMatch = true; // No specific room name requested
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ Detailed ANY ROOM: Using room '{roomName}' (no specific room requested)");
                    }
                    else
                    {
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Detailed Room check - Room name: {roomName}, Looking for: '{targetRoomName}'");
                        // Using simple Contains for now, retain original capital letter check if needed
                        if (roomName.Contains(targetRoomName, StringComparison.OrdinalIgnoreCase))
                        {
                            isRoomMatch = true;
                            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ Detailed ROOM MATCH: '{roomName}' contains '{targetRoomName}'");
                        }
                        else
                        {
                           Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✗ Detailed NO ROOM MATCH: Room '{roomName}' does not contain '{targetRoomName}'");
                        }
                    }
                    
                    // --- End Detailed Checks ---

                    // Log final check results before decision
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Final Check -> Room: {roomName}, Preset: {presetName}, Floor: {floorName}, Building: {buildingName} | RoomMatch: {isRoomMatch}, FloorMatch: {isFloorMatch}, PresetMatch: {isRoomPresetMatch}");

                    // Add room only if ALL relevant detailed checks passed
                    if (isRoomMatch && isFloorMatch && isRoomPresetMatch)
                    {
                        matchingRooms.Add(room);
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] *** FOUND MATCHING ROOM (passed all checks): {roomName} in {buildingName} ***");
                    }
                    else
                    {
                         // Log why it failed if needed
                         // Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Room {roomName} did not pass all detailed checks. Skipping.");
                    }
                    
                    // Batch processing yield
                    if (++processedCount % batchSize == 0)
                    {
                        yield return null; // Allow the game to continue running
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Processed {processedCount} rooms so far");
                    }
                } // End room loop
            } // End building loop
            
            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Found {matchingRooms.Count} matching rooms after full scan");
            if (matchingRooms.Count == 0)
            {
                Plugin.Log.LogError($"[SpawnItemCustomBuilding] No matching rooms found for building preset: {buildingPreset}, room name: {targetRoomName}");
                yield break;
            }
            int randomRoomIndex = UnityEngine.Random.Range(0, matchingRooms.Count);
            NewRoom selectedRoom = matchingRooms[randomRoomIndex];
            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Selected room: {selectedRoom.name}");
            if (!string.IsNullOrEmpty(customSubRoomName))
            {
                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Looking for sub-room: {customSubRoomName}");
                string locationPrefix = "";
                if (selectedRoom.gameLocation != null && 
                    selectedRoom.gameLocation.thisAsAddress != null && 
                    selectedRoom.gameLocation.thisAsAddress.company != null && 
                    !string.IsNullOrEmpty(selectedRoom.gameLocation.thisAsAddress.company.name))
                {
                    locationPrefix = selectedRoom.gameLocation.thisAsAddress.company.name;
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Using company name as prefix: {locationPrefix}");
                }
                else if (!string.IsNullOrEmpty(selectedRoom.name))
                {
                    int lastSpaceIndex = selectedRoom.name.LastIndexOf(' ');
                    if (lastSpaceIndex > 0)
                    {
                        locationPrefix = selectedRoom.name.Substring(0, lastSpaceIndex);
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Extracted prefix from room name: {locationPrefix}");
                    }
                }
                if (!string.IsNullOrEmpty(locationPrefix))
                {
                    List<NewRoom> subRooms = new List<NewRoom>();
                    NewAddress building = selectedRoom.gameLocation?.thisAsAddress;
                    if (building != null && building.rooms != null)
                    {
                        foreach (var room in building.rooms)
                        {
                            if (room == null || string.IsNullOrEmpty(room.name)) continue;
                            if (room.name.Contains("controller", StringComparison.OrdinalIgnoreCase) || 
                                room.name.EndsWith("Null", StringComparison.OrdinalIgnoreCase))
                            {
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Filtering out sub-room: {room.name} (contains 'controller' or ends with 'Null')");
                                continue;
                            }
                            bool hasCapitalLetters = customSubRoomName.Any(char.IsUpper);
                            bool isMatch = false;
                            bool isSubRoomPresetMatch = true;
                            if (customSubRoomPreset != null && !string.IsNullOrEmpty(customSubRoomPreset))
                            {
                                isSubRoomPresetMatch = false;
                                string roomPresetName = room.preset != null ? room.preset.name : "no preset";
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Sub-room preset check - Room preset: {roomPresetName}, Looking for: '{customSubRoomPreset}'");
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
                            if (hasCapitalLetters)
                            {
                                if (room.name.Contains(customSubRoomName, StringComparison.OrdinalIgnoreCase))
                                {
                                    isMatch = true;
                                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ SUB-ROOM CAPITAL MATCH: Found sub-room '{room.name}' containing '{customSubRoomName}'");
                                }
                            }
                            else if (room.name.Contains(customSubRoomName, StringComparison.OrdinalIgnoreCase))
                            {
                                isMatch = true;
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ SUB-ROOM LOWERCASE MATCH: Found sub-room '{room.name}' containing '{customSubRoomName}'");
                            }
                            if (!isMatch && room.name.Contains(locationPrefix, StringComparison.OrdinalIgnoreCase) && 
                                room.name.Contains(customSubRoomName, StringComparison.OrdinalIgnoreCase))
                            {
                                isMatch = true;
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] SUB-ROOM PREFIX MATCH: Found sub-room '{room.name}' containing both prefix '{locationPrefix}' and '{customSubRoomName}'");
                            }
                            if (isMatch && isSubRoomPresetMatch)
                            {
                                subRooms.Add(room);
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] *** FOUND MATCHING SUB-ROOM: {room.name} ***");
                            }
                            else if (isMatch && !isSubRoomPresetMatch)
                            {
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Sub-room '{room.name}' matches name but not preset. Skipping.");
                            }
                            // Only yield after processing a batch of rooms
                            if (++processedCount % 10 == 0)
                            {
                                yield return null; // Allow the game to continue running
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Processed {processedCount} sub-rooms so far");
                            }
                        }
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
            NewNode placementNode = null;
            Vector3 spawnPosition = Vector3.zero;
            if (selectedRoom.nodes != null && selectedRoom.nodes.Count > 0)
            {
                List<NewNode> nodesList = new List<NewNode>();
                foreach (var node in selectedRoom.nodes)
                {
                    if (node.isInaccessable) { Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Filtering out node: {node.name} (inaccessible)"); continue; }
                    nodesList.Add(node);
                }
                if (nodesList.Count > 0)
                {
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
            spawnPosition.y += 0.00f;
            spawnPosition.x += UnityEngine.Random.Range(-0.1f, 0.1f);
            spawnPosition.z += UnityEngine.Random.Range(-0.1f, 0.1f);
            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Calculated spawn position: {spawnPosition}");
            if (placementNode == null)
            {
                Plugin.Log.LogWarning($"[SpawnItemCustomBuilding] Could not find a valid node for placement.");
                yield break;
            }
            Il2CppSystem.Collections.Generic.List<Interactable.Passed> passedVars = new Il2CppSystem.Collections.Generic.List<Interactable.Passed>();
            passedVars.Add(new Interactable.Passed(Interactable.PassedVarType.roomID, placementNode.room.roomID, null));
            try
            {
                float randomYRotation = UnityEngine.Random.Range(0f, 360f);
                Vector3 randomRotation = new Vector3(0f, randomYRotation, 0f);
                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Using random rotation: {randomRotation}");
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
                    spawnedItem.node = placementNode;
                    spawnedItem.UpdateWorldPositionAndNode(true, true);
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Successfully created item in custom location");
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Item position: {spawnedItem.wPos}, node: {(spawnedItem.node != null ? spawnedItem.node.ToString() : "null")}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemCustomBuilding] Error spawning item: {ex.Message}");
                Plugin.Log.LogError($"[SpawnItemCustomBuilding] Stack trace: {ex.StackTrace}");
            }
        }
    }
}