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
            string customSubRoomName = null, string customRoomPreset = null, string customSubRoomPreset = null,
            List<string> customRoomNames = null, List<string> customRoomPresets = null,
            List<string> customSubRoomNames = null, List<string> customSubRoomPresets = null,
            bool useFurniture = false, List<string> furniturePresets = null)
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
                
                // Log floor names
                if (customFloorNames != null && customFloorNames.Count > 0)
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Using specific floor names: {string.Join(", ", customFloorNames)}");
                }
                else
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] No specific floor names provided, checking all floors");
                }
                
                // Log room names
                if (!string.IsNullOrEmpty(targetRoomName))
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Looking for room with name: {targetRoomName}");
                }
                if (customRoomNames != null && customRoomNames.Count > 0)
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Looking for rooms with names from list: {string.Join(", ", customRoomNames)}");
                }
                
                // Log room presets
                if (!string.IsNullOrEmpty(customRoomPreset))
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Looking for room with preset: {customRoomPreset}");
                }
                if (customRoomPresets != null && customRoomPresets.Count > 0)
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Looking for rooms with presets from list: {string.Join(", ", customRoomPresets)}");
                }
                
                // Log sub-room names
                if (!string.IsNullOrEmpty(customSubRoomName))
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Looking for sub-room with name: {customSubRoomName}");
                }
                if (customSubRoomNames != null && customSubRoomNames.Count > 0)
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Looking for sub-rooms with names from list: {string.Join(", ", customSubRoomNames)}");
                }
                
                // Log sub-room presets
                if (!string.IsNullOrEmpty(customSubRoomPreset))
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Looking for sub-room with preset: {customSubRoomPreset}");
                }
                if (customSubRoomPresets != null && customSubRoomPresets.Count > 0)
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Looking for sub-rooms with presets from list: {string.Join(", ", customSubRoomPresets)}");
                }
                
                // Log furniture options
                if (useFurniture)
                {
                    if (furniturePresets != null && furniturePresets.Count > 0)
                    {
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Using furniture for item placement. Looking for furniture presets: {string.Join(", ", furniturePresets)}");
                    }
                    else
                    {
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Using furniture for item placement, but no specific furniture presets provided. Will try to use any available furniture.");
                    }
                }
                else
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Using node-based placement (not using furniture).");
                }

                // Find the custom building and spawn the item
                CoroutineHelper.StartCoroutine(SpawnItemInCustomBuildingCoroutine(interactablePresetItem, owner, recipient, presetName, 
                    targetRoomName, buildingPreset, customFloorNames, customSubRoomName, customRoomPreset, customSubRoomPreset,
                    customRoomNames, customRoomPresets, customSubRoomNames, customSubRoomPresets,
                    useFurniture, furniturePresets));
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
        
        private static IEnumerator SpawnItemInCustomBuildingCoroutine(InteractablePreset itemPreset, Human owner, Human recipient, string itemNameForLog, string targetRoomName = null, string buildingPreset = null, List<string> customFloorNames = null, string customSubRoomName = null, string customRoomPreset = null, string customSubRoomPreset = null, List<string> customRoomNames = null, List<string> customRoomPresets = null, List<string> customSubRoomNames = null, List<string> customSubRoomPresets = null, bool useFurniture = false, List<string> furniturePresets = null)
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

                    // --- Prioritized Preliminary Check ---
                    bool criteriaProvided = !string.IsNullOrEmpty(targetRoomName) ||
                                            !string.IsNullOrEmpty(customRoomPreset) ||
                                            (customFloorNames != null && customFloorNames.Count > 0) ||
                                            (customRoomNames != null && customRoomNames.Count > 0) ||
                                            (customRoomPresets != null && customRoomPresets.Count > 0);

                    bool preliminaryMatch = !criteriaProvided; // Pass if no specific criteria given

                    if (criteriaProvided)
                    {
                        // Check if room matches AT LEAST ONE provided criterion
                        // Single string room name check (legacy)
                        if (!string.IsNullOrEmpty(targetRoomName) && roomName.Contains(targetRoomName, StringComparison.OrdinalIgnoreCase))
                        {
                            preliminaryMatch = true;
                            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Preliminary match on Room Name: {roomName} contains {targetRoomName}");
                        }
                        // List of room names check
                        else if (customRoomNames != null && customRoomNames.Count > 0)
                        {
                            foreach (string customRoomName in customRoomNames)
                            {
                                if (!string.IsNullOrEmpty(customRoomName) && roomName.Contains(customRoomName, StringComparison.OrdinalIgnoreCase))
                                {
                                    preliminaryMatch = true;
                                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Preliminary match on Room Name (list): {roomName} contains {customRoomName}");
                                    break;
                                }
                            }
                        }
                        // Single string room preset check (legacy)
                        else if (!string.IsNullOrEmpty(customRoomPreset) && presetName.Contains(customRoomPreset, StringComparison.OrdinalIgnoreCase))
                        {
                            preliminaryMatch = true;
                            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Preliminary match on Room Preset: {presetName} contains {customRoomPreset}");
                        }
                        // List of room presets check
                        else if (customRoomPresets != null && customRoomPresets.Count > 0)
                        {
                            foreach (string roomPreset in customRoomPresets)
                            {
                                if (!string.IsNullOrEmpty(roomPreset) && presetName.Contains(roomPreset, StringComparison.OrdinalIgnoreCase))
                                {
                                    preliminaryMatch = true;
                                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Preliminary match on Room Preset (list): {presetName} contains {roomPreset}");
                                    break;
                                }
                            }
                        }
                        // Floor names check
                        else if (customFloorNames != null && customFloorNames.Count > 0)
                        {
                            foreach (string customFloorName in customFloorNames)
                            {
                                if (floorName.Contains(customFloorName, StringComparison.OrdinalIgnoreCase))
                                {
                                    preliminaryMatch = true;
                                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Preliminary match on Floor Name: {floorName} contains {customFloorName}");
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
                    bool roomPresetSpecified = !string.IsNullOrEmpty(customRoomPreset) || 
                                              (customRoomPresets != null && customRoomPresets.Count > 0);
                    
                    if (roomPresetSpecified)
                    {
                        isRoomPresetMatch = false; // Must explicitly match
                        
                        // Check single string room preset (legacy)
                        if (!string.IsNullOrEmpty(customRoomPreset))
                        {
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
                        
                        // Check list of room presets
                        if (!isRoomPresetMatch && customRoomPresets != null && customRoomPresets.Count > 0)
                        {
                            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Detailed Room preset list check - Room preset: {presetName}, Looking for matches in list of {customRoomPresets.Count} presets");
                            foreach (string roomPreset in customRoomPresets)
                            {
                                if (!string.IsNullOrEmpty(roomPreset) && presetName.Contains(roomPreset, StringComparison.OrdinalIgnoreCase))
                                {
                                    isRoomPresetMatch = true;
                                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ Detailed ROOM PRESET LIST MATCH: Room preset '{presetName}' contains '{roomPreset}'");
                                    break;
                                }
                            }
                            
                            if (!isRoomPresetMatch)
                            {
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✗ Detailed NO ROOM PRESET LIST MATCH: Room preset '{presetName}' does not match any preset in the list");
                            }
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
                    bool roomNameSpecified = !string.IsNullOrEmpty(targetRoomName) || 
                                           (customRoomNames != null && customRoomNames.Count > 0);
                                           
                    if (!roomNameSpecified)
                    {
                        isRoomMatch = true; // No specific room name requested
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ Detailed ANY ROOM: Using room '{roomName}' (no specific room requested)");
                    }
                    else
                    {
                        isRoomMatch = false; // Must explicitly match
                        
                        // Check single string room name (legacy)
                        if (!string.IsNullOrEmpty(targetRoomName))
                        {
                            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Detailed Room check - Room name: {roomName}, Looking for: '{targetRoomName}'");
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
                        
                        // Check list of room names
                        if (!isRoomMatch && customRoomNames != null && customRoomNames.Count > 0)
                        {
                            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Detailed Room name list check - Room name: {roomName}, Looking for matches in list of {customRoomNames.Count} names");
                            foreach (string customRoomName in customRoomNames)
                            {
                                if (!string.IsNullOrEmpty(customRoomName) && roomName.Contains(customRoomName, StringComparison.OrdinalIgnoreCase))
                                {
                                    isRoomMatch = true;
                                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ Detailed ROOM NAME LIST MATCH: Room '{roomName}' contains '{customRoomName}'");
                                    break;
                                }
                            }
                            
                            if (!isRoomMatch)
                            {
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✗ Detailed NO ROOM NAME LIST MATCH: Room '{roomName}' does not match any name in the list");
                            }
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
            bool subRoomSpecified = !string.IsNullOrEmpty(customSubRoomName) || (customSubRoomNames != null && customSubRoomNames.Count > 0);
            bool subRoomRequired = false; // By default, sub-rooms are optional even when specified
            if (subRoomSpecified)
            {
                string subRoomLogInfo = !string.IsNullOrEmpty(customSubRoomName) ? customSubRoomName : "from list";
                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Looking for sub-room: {subRoomLogInfo}");
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
                            bool isMatch = false;
                            bool hasCapitalLetters = false;
                            bool isSubRoomPresetMatch = true;
                            bool subRoomPresetSpecified = (customSubRoomPreset != null && !string.IsNullOrEmpty(customSubRoomPreset)) ||
                                                       (customSubRoomPresets != null && customSubRoomPresets.Count > 0);
                            
                            // Process single string sub-room name (legacy)
                            if (!string.IsNullOrEmpty(customSubRoomName))
                            {
                                hasCapitalLetters = customSubRoomName.Any(char.IsUpper);
                            }
                            // Process list of sub-room names
                            else if (customSubRoomNames != null && customSubRoomNames.Count > 0)
                            {
                                // We'll handle this in the matching section below
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Using list of {customSubRoomNames.Count} sub-room names for matching");
                            }
                                                       
                            if (subRoomPresetSpecified)
                            {
                                isSubRoomPresetMatch = false; // Must explicitly match
                                string roomPresetName = room.preset != null ? room.preset.name : "no preset";
                                
                                // Check single string sub-room preset (legacy)
                                if (customSubRoomPreset != null && !string.IsNullOrEmpty(customSubRoomPreset))
                                {
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
                                
                                // Check list of sub-room presets
                                if (!isSubRoomPresetMatch && customSubRoomPresets != null && customSubRoomPresets.Count > 0)
                                {
                                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Sub-room preset list check - Room preset: {roomPresetName}, Looking for matches in list of {customSubRoomPresets.Count} presets");
                                    foreach (string subRoomPreset in customSubRoomPresets)
                                    {
                                        if (!string.IsNullOrEmpty(subRoomPreset) && roomPresetName.Contains(subRoomPreset, StringComparison.OrdinalIgnoreCase))
                                        {
                                            isSubRoomPresetMatch = true;
                                            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ SUB-ROOM PRESET LIST MATCH: Room preset '{roomPresetName}' contains '{subRoomPreset}'");
                                            break;
                                        }
                                    }
                                    
                                    if (!isSubRoomPresetMatch)
                                    {
                                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✗ NO SUB-ROOM PRESET LIST MATCH: Room preset '{roomPresetName}' does not match any preset in the list");
                                    }
                                }
                            }
                            // Check single string sub-room name (legacy)
                            if (!string.IsNullOrEmpty(customSubRoomName))
                            {
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
                            }
                            
                            // Check list of sub-room names
                            if (!isMatch && customSubRoomNames != null && customSubRoomNames.Count > 0)
                            {
                                foreach (string subRoomName in customSubRoomNames)
                                {
                                    if (string.IsNullOrEmpty(subRoomName))
                                        continue;
                                        
                                    bool hasCapitalsInName = subRoomName.Any(char.IsUpper);
                                    
                                    if (hasCapitalsInName)
                                    {
                                        if (room.name.Contains(subRoomName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            isMatch = true;
                                            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ SUB-ROOM LIST CAPITAL MATCH: Found sub-room '{room.name}' containing '{subRoomName}'");
                                            break;
                                        }
                                    }
                                    else if (room.name.Contains(subRoomName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        isMatch = true;
                                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✓ SUB-ROOM LIST LOWERCASE MATCH: Found sub-room '{room.name}' containing '{subRoomName}'");
                                        break;
                                    }
                                    
                                    if (!isMatch && room.name.Contains(locationPrefix, StringComparison.OrdinalIgnoreCase) && 
                                        room.name.Contains(subRoomName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        isMatch = true;
                                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] SUB-ROOM LIST PREFIX MATCH: Found sub-room '{room.name}' containing both prefix '{locationPrefix}' and '{subRoomName}'");
                                        break;
                                    }
                                }
                                
                                if (!isMatch)
                                {
                                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] ✗ NO SUB-ROOM LIST MATCH: Room '{room.name}' does not match any name in the list");
                                }
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
                            // If we found matching sub-rooms, use one of them
                            int randomSubRoomIndex = UnityEngine.Random.Range(0, subRooms.Count);
                            selectedRoom = subRooms[randomSubRoomIndex];
                            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Selected sub-room: {selectedRoom.name}");
                        }
                        else if (!subRoomRequired)
                        {
                            // If sub-rooms are optional (default behavior), use the original room
                            string subRoomSearchInfo = !string.IsNullOrEmpty(customSubRoomName) ? $"'{customSubRoomName}'" : 
                                                      (customSubRoomNames != null && customSubRoomNames.Count > 0) ? $"list of {customSubRoomNames.Count} sub-room names" : "(no sub-room specified)";
                            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] No matching sub-rooms found for {subRoomSearchInfo} in building with prefix '{locationPrefix}'. Using original room instead (sub-rooms are optional).");
                            // Keep using the originally selected room
                        }
                        else
                        {
                            // If sub-rooms are required but none found, log a warning
                            string subRoomSearchInfo = !string.IsNullOrEmpty(customSubRoomName) ? $"'{customSubRoomName}'" : 
                                                      (customSubRoomNames != null && customSubRoomNames.Count > 0) ? $"list of {customSubRoomNames.Count} sub-room names" : "(no sub-room specified)";
                            Plugin.Log.LogWarning($"[SpawnItemCustomBuilding] No matching sub-rooms found for {subRoomSearchInfo} in building with prefix '{locationPrefix}'. Using original room instead.");
                        }
                    }
                }
                else
                {
                    Plugin.Log.LogWarning($"[SpawnItemCustomBuilding] Could not determine location prefix for sub-room matching. Using original room instead.");
                }
            }
            // Attempt to place on furniture if requested
            bool usedFurniture = false;
            FurnitureLocation selectedFurniture = null;
            FurniturePreset.SubObject selectedSubObject = null;
            
            if (useFurniture && selectedRoom != null)
            {
                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Attempting to find furniture in room {selectedRoom.name} for item placement");
                
                // Get all furniture in the room
                List<FurnitureLocation> matchingFurniture = new List<FurnitureLocation>();
                
                if (selectedRoom.individualFurniture != null && selectedRoom.individualFurniture.Count > 0)
                {
                    // Check each piece of furniture in the room
                    foreach (var furnitureLocation in selectedRoom.individualFurniture)
                    {
                        if (furnitureLocation == null || furnitureLocation.furniture == null) continue;
                        
                        // If specific furniture presets are specified, check if this furniture matches
                        bool isMatch = false;
                        if (furniturePresets != null && furniturePresets.Count > 0)
                        {
                            string furnitureName = furnitureLocation.furniture.name;
                            foreach (string presetName in furniturePresets)
                            {
                                if (string.IsNullOrEmpty(presetName)) continue;
                                
                                if (furnitureName.Contains(presetName, StringComparison.OrdinalIgnoreCase))
                                {
                                    isMatch = true;
                                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Found matching furniture: {furnitureName} contains '{presetName}'");
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // If no specific furniture presets are specified, use any furniture
                            isMatch = true;
                        }
                        
                        if (isMatch)
                        {
                            matchingFurniture.Add(furnitureLocation);
                        }
                    }
                }
                
                // If we found matching furniture, select one randomly
                if (matchingFurniture.Count > 0)
                {
                    int randomFurnitureIndex = UnityEngine.Random.Range(0, matchingFurniture.Count);
                    selectedFurniture = matchingFurniture[randomFurnitureIndex];
                    
                    // Check if the furniture has subobjects we can use for placement
                    if (selectedFurniture.furniture.subObjects != null && selectedFurniture.furniture.subObjects.Count > 0)
                    {
                        // Find suitable subobjects for placement
                        List<FurniturePreset.SubObject> suitableSubObjects = new List<FurniturePreset.SubObject>();
                        
                        foreach (var subObject in selectedFurniture.furniture.subObjects)
                        {
                            // For now, we'll use any subobject, but we could filter by type if needed
                            // For DisplayCabinet, we might want to check for specific subobject types
                            suitableSubObjects.Add(subObject);
                        }
                        
                        if (suitableSubObjects.Count > 0)
                        {
                            int randomSubObjectIndex = UnityEngine.Random.Range(0, suitableSubObjects.Count);
                            selectedSubObject = suitableSubObjects[randomSubObjectIndex];
                            
                            // Check if this subobject is already used by an interactable
                            bool alreadyUsed = false;
                            if (selectedFurniture.integratedInteractables != null)
                            {
                                foreach (var interactable in selectedFurniture.integratedInteractables)
                                {
                                    if (interactable != null && interactable.subObject == selectedSubObject)
                                    {
                                        alreadyUsed = true;
                                        break;
                                    }
                                }
                            }
                            
                            if (!alreadyUsed)
                            {
                                usedFurniture = true;
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Will place item on furniture: {selectedFurniture.furniture.name}, using subobject index: {suitableSubObjects.IndexOf(selectedSubObject)}");
                            }
                            else
                            {
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Selected subobject is already used by another interactable. Will try node placement instead.");
                            }
                        }
                        else
                        {
                            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] No suitable subobjects found on furniture {selectedFurniture.furniture.name}. Will try node placement instead.");
                        }
                    }
                    else
                    {
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Furniture {selectedFurniture.furniture.name} has no subobjects. Will try node placement instead.");
                    }
                }
                else
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] No matching furniture found in room {selectedRoom.name}. Will try node placement instead.");
                }
            }
            
            // IMPORTANT: Don't reset furniture variables here as they're needed for item placement
            
            // If furniture wasn't requested or we couldn't find suitable furniture, try to place on furniture again
            // This is a second attempt in case the first one failed
            if (!usedFurniture && useFurniture && selectedRoom != null && selectedRoom.individualFurniture != null && selectedRoom.individualFurniture.Count > 0)
            {
                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Attempting to find furniture in room {selectedRoom.name} for item placement");
                
                // Get all furniture in the room
                List<FurnitureLocation> matchingFurniture = new List<FurnitureLocation>();
                
                // Check each piece of furniture in the room
                foreach (var furnitureLocation in selectedRoom.individualFurniture)
                {
                    if (furnitureLocation == null || furnitureLocation.furniture == null) continue;
                    
                    // If specific furniture presets are specified, check if this furniture matches
                    bool isMatch = false;
                    if (furniturePresets != null && furniturePresets.Count > 0)
                    {
                        string furnitureName = furnitureLocation.furniture.name;
                        foreach (string presetName in furniturePresets)
                        {
                            if (string.IsNullOrEmpty(presetName)) continue;
                            
                            if (furnitureName.Contains(presetName, StringComparison.OrdinalIgnoreCase))
                            {
                                isMatch = true;
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Found matching furniture: {furnitureName} contains '{presetName}'");
                                break;
                            }
                        }
                    }
                    else
                    {
                        // If no specific furniture presets are specified, use any furniture
                        isMatch = true;
                    }
                    
                    if (isMatch)
                    {
                        matchingFurniture.Add(furnitureLocation);
                    }
                }
                
                // If we found matching furniture, select one randomly
                if (matchingFurniture.Count > 0)
                {
                    int randomFurnitureIndex = UnityEngine.Random.Range(0, matchingFurniture.Count);
                    selectedFurniture = matchingFurniture[randomFurnitureIndex];
                    
                    // Check if the furniture has subobjects we can use for placement
                    if (selectedFurniture.furniture.subObjects != null && selectedFurniture.furniture.subObjects.Count > 0)
                    {
                        // Find suitable subobjects for placement
                        List<FurniturePreset.SubObject> suitableSubObjects = new List<FurniturePreset.SubObject>();
                        
                        foreach (var subObject in selectedFurniture.furniture.subObjects)
                        {
                            // For now, we'll use any subobject, but we could filter by type if needed
                            // For DisplayCabinet, we might want to check for specific subobject types
                            suitableSubObjects.Add(subObject);
                        }
                        
                        if (suitableSubObjects.Count > 0)
                        {
                            int randomSubObjectIndex = UnityEngine.Random.Range(0, suitableSubObjects.Count);
                            selectedSubObject = suitableSubObjects[randomSubObjectIndex];
                            
                            // Check if this subobject is already used by an interactable
                            bool alreadyUsed = false;
                            if (selectedFurniture.integratedInteractables != null)
                            {
                                foreach (var interactable in selectedFurniture.integratedInteractables)
                                {
                                    if (interactable != null && interactable.subObject == selectedSubObject)
                                    {
                                        alreadyUsed = true;
                                        break;
                                    }
                                }
                            }
                            
                            if (!alreadyUsed)
                            {
                                usedFurniture = true;
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Will place item on furniture: {selectedFurniture.furniture.name}, using subobject index: {suitableSubObjects.IndexOf(selectedSubObject)}");
                            }
                            else
                            {
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Selected subobject is already used by another interactable. Will try node placement instead.");
                            }
                        }
                        else
                        {
                            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] No suitable subobjects found on furniture {selectedFurniture.furniture.name}. Will try node placement instead.");
                        }
                    }
                    else
                    {
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Furniture {selectedFurniture.furniture.name} has no subobjects. Will try node placement instead.");
                    }
                }
                else
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] No matching furniture found in room {selectedRoom.name}. Will try node placement instead.");
                }
            }
            
            // Fall back to node placement if we couldn't use furniture or it wasn't requested
            NewNode placementNode = null;
            Vector3 spawnPosition = Vector3.zero;
            
            // Skip node placement if we're using furniture
            if (usedFurniture)
            {
                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Using furniture placement, skipping node placement");
            }
            // Otherwise try to use node placement
            else if (selectedRoom.nodes != null && selectedRoom.nodes.Count > 0)
            {
                List<NewNode> nodesList = new List<NewNode>();
                foreach (var node in selectedRoom.nodes)
                {
                    if (node.isInaccessable || node.isObstacle) { Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Filtering out node: {node.name} (inaccessible)"); continue; }
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
            else if (!usedFurniture) // Only show this warning if we're not using furniture
            {
                Plugin.Log.LogWarning($"[SpawnItemCustomBuilding] No nodes found in selected room.");
                yield break;
            }
            spawnPosition.y += 0.00f;
            spawnPosition.x += UnityEngine.Random.Range(-0.1f, 0.1f);
            spawnPosition.z += UnityEngine.Random.Range(-0.1f, 0.1f);
            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Calculated spawn position: {spawnPosition}");
            // Create the interactable based on whether we're using furniture or node placement
            Interactable spawnedItem = null;
            
            try
            {
                // If we're using furniture placement
                if (usedFurniture && selectedFurniture != null && selectedSubObject != null)
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Creating item '{itemNameForLog}' on furniture {selectedFurniture.furniture.name} in room {selectedRoom.name}");
                    
                    // Create a list of passed variables for the room ID
                    Il2CppSystem.Collections.Generic.List<Interactable.Passed> furniturePassedVars = new Il2CppSystem.Collections.Generic.List<Interactable.Passed>();
                    furniturePassedVars.Add(new Interactable.Passed(Interactable.PassedVarType.roomID, selectedRoom.roomID, null));
                    
                    // Create the interactable on the furniture
                    spawnedItem = InteractableCreator.Instance.CreateFurnitureSpawnedInteractableThreadSafe(
                        itemPreset,                // The item preset
                        selectedRoom,              // The room
                        selectedFurniture,         // The furniture location
                        selectedSubObject,         // The subobject to place on
                        owner,                     // The owner of the item
                        owner,                     // The writer (same as owner)
                        null,                      // No passed object
                        furniturePassedVars,       // Passed variables with room ID
                        null,                      // No passed object
                        null,                      // No passed object
                        ""                         // No load GUID
                    );
                    
                    if (spawnedItem != null)
                    {
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Successfully spawned '{itemNameForLog}' on furniture {selectedFurniture.furniture.name} in {selectedRoom.name}");
                    }
                    else
                    {
                        Plugin.Log.LogError($"[SpawnItemCustomBuilding] Failed to create furniture-spawned interactable '{itemNameForLog}'");
                    }
                }
                // If we're using node placement
                else if (placementNode != null)
                {
                    // Create a list of passed variables for the room ID
                    Il2CppSystem.Collections.Generic.List<Interactable.Passed> passedVars = new Il2CppSystem.Collections.Generic.List<Interactable.Passed>();
                    passedVars.Add(new Interactable.Passed(Interactable.PassedVarType.roomID, placementNode.room.roomID, null));
                    
                    float randomYRotation = UnityEngine.Random.Range(0f, 360f);
                    Vector3 randomRotation = new Vector3(0f, randomYRotation, 0f);
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Using random rotation: {randomRotation}");
                    
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
                        spawnedItem.node = placementNode;
                        spawnedItem.UpdateWorldPositionAndNode(true, true);
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Successfully created item in custom location at node");
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Item position: {spawnedItem.wPos}, node: {(spawnedItem.node != null ? spawnedItem.node.ToString() : "null")}");
                    }
                    else
                    {
                        Plugin.Log.LogError($"[SpawnItemCustomBuilding] Failed to create node-based interactable '{itemNameForLog}'");
                    }
                }
                else
                {
                    Plugin.Log.LogWarning($"[SpawnItemCustomBuilding] Could not find a valid placement node or furniture in room {selectedRoom.name}");
                    yield break;
                }
                
                // Return the result
                if (spawnedItem != null)
                {
                    Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Item '{itemNameForLog}' successfully created in {selectedRoom.name}");
                    
                    // Log all furniture in the room for reference
                    if (selectedRoom != null && selectedRoom.individualFurniture != null && selectedRoom.individualFurniture.Count > 0)
                    {
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] === FURNITURE INVENTORY FOR ROOM: {selectedRoom.name} ===");
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Total furniture count: {selectedRoom.individualFurniture.Count}");
                        
                        Dictionary<string, int> furnitureTypes = new Dictionary<string, int>();
                        
                        foreach (var furniture in selectedRoom.individualFurniture)
                        {
                            if (furniture != null && furniture.furniture != null)
                            {
                                string furnitureName = furniture.furniture.name;
                                
                                // Count furniture types
                                if (furnitureTypes.ContainsKey(furnitureName))
                                {
                                    furnitureTypes[furnitureName]++;
                                }
                                else
                                {
                                    furnitureTypes[furnitureName] = 1;
                                }
                                
                                // Log subobject information for each furniture
                                int subObjectCount = furniture.furniture.subObjects != null ? furniture.furniture.subObjects.Count : 0;
                                Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] Furniture: {furnitureName}, SubObjects: {subObjectCount}");
                                
                                if (subObjectCount > 0)
                                {
                                    for (int i = 0; i < furniture.furniture.subObjects.Count; i++)
                                    {
                                        var subObject = furniture.furniture.subObjects[i];
                                        bool isUsed = false;
                                        
                                        // Check if this subobject is already used
                                        if (furniture.integratedInteractables != null)
                                        {
                                            foreach (var interactable in furniture.integratedInteractables)
                                            {
                                                if (interactable != null && interactable.subObject == subObject)
                                                {
                                                    isUsed = true;
                                                    break;
                                                }
                                            }
                                        }
                                        
                                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding]   - SubObject {i}: {(isUsed ? "USED" : "AVAILABLE")}");
                                    }
                                }
                            }
                        }
                        
                        // Log summary of furniture types
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] === FURNITURE SUMMARY ===");
                        foreach (var kvp in furnitureTypes.OrderByDescending(x => x.Value))
                        {
                            Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] {kvp.Key}: {kvp.Value} instances");
                        }
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] === END FURNITURE INVENTORY ===");
                    }
                    else
                    {
                        Plugin.Log.LogInfo($"[SpawnItemCustomBuilding] No furniture found in room {selectedRoom.name}");
                    }
                }
                else
                {
                    Plugin.Log.LogError($"[SpawnItemCustomBuilding] Failed to create item '{itemNameForLog}' in {selectedRoom.name}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemCustomBuilding] Error spawning item: {ex.Message}");
                Plugin.Log.LogError($"[SpawnItemCustomBuilding] Stack trace: {ex.StackTrace}");
            }
            
            yield break;
        }
    }
}