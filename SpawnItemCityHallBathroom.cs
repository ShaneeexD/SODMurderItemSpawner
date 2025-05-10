using System;
using System.Collections.Generic;
using SOD.Common;
using UnityEngine;
using System.Linq;

namespace MurderItemSpawner
{
    public class SpawnItemCityHallBathroom : MonoBehaviour
    {
        private static SpawnItemCityHallBathroom _instance;
        private static SpawnItemCityHallBathroom Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SpawnItemCityHallBathroom_Instance");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<SpawnItemCityHallBathroom>();
                }
                return _instance;
            }
        }

        // Method to spawn an item in a City Hall bathroom
        public static void SpawnItemAtLocation(Human owner, Human recipient, string presetName, float spawnChance = 1.0f)
        {
            try
            {
                // Check if we should spawn based on chance
                float randomValue = UnityEngine.Random.Range(0f, 1f);
                if (randomValue > spawnChance)
                {
                    Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Skipping spawn of {presetName} due to chance (roll: {randomValue}, needed: <= {spawnChance})");
                    return;
                }

                // Get the interactable preset
                InteractablePreset interactablePresetItem = Toolbox.Instance.GetInteractablePreset(presetName);
                if (interactablePresetItem == null)
                {
                    Plugin.Log.LogError($"[SpawnItemCityHallBathroom] Could not find interactable preset with name {presetName}");
                    return;
                }

                Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Owner: {owner.name}, Recipient: {recipient.name}");

                // Find the City Hall and spawn the item
                Interactable spawnedItem = SpawnItemInCityHallBathroom(interactablePresetItem, owner, recipient, presetName);
                
                if (spawnedItem != null)
                {
                    // Ensure the item is owned by the correct person
                    spawnedItem.SetOwner(owner);
                    
                    Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Successfully spawned '{presetName}' in City Hall bathroom. Item node: {(spawnedItem.node != null ? spawnedItem.node.ToString() : "null")}");
                    Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Item '{presetName}' final world position: {spawnedItem.wPos}");
                }
                else
                {
                    Plugin.Log.LogError($"[SpawnItemCityHallBathroom] Failed to create item '{presetName}' in City Hall bathroom.");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemCityHallBathroom] Error spawning item {presetName}: {ex.Message}");
                Plugin.Log.LogError($"[SpawnItemCityHallBathroom] Stack trace: {ex.StackTrace}");
            }
        }

        // Method to spawn an item in a City Hall bathroom
        private static Interactable SpawnItemInCityHallBathroom(InteractablePreset itemPreset, Human owner, Human recipient, string itemNameForLog)
        {
            // Instead of trying to find the City Hall specifically, we'll just pick a random bathroom in any public building
            // This is more reliable and will work even if the City Hall can't be found
            
            // Get all addresses in the city
            List<NewAddress> publicBuildings = new List<NewAddress>();
            
            // Find all public buildings (non-apartment buildings)
            CityData cityData = CityData.Instance;
            foreach (var newLoc in cityData.gameLocationDirectory)
            {
                if (newLoc.name.Contains("City Hall") || 
                        newLoc.name.Contains("CityHall") || 
                        newLoc.name.Contains("Hospital"))
                {
                    NewAddress address = newLoc.thisAsAddress;
                    publicBuildings.Add(address);
                    Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Found public building: {address.name}");
                }
            }
            
            if (publicBuildings.Count == 0)
            {
                Plugin.Log.LogWarning($"[SpawnItemCityHallBathroom] Could not find any public buildings.");
                return null;
            }
            
            // Choose a random public building, preferring City Hall if available
            NewAddress selectedBuilding = null;
            
            // First try to find City Hall
            foreach (var building in publicBuildings)
            {
                if (building.name.Contains("CityHall") || building.name.Contains("City Hall") || building.name.Contains("Hospital"))
                {
                    selectedBuilding = building;
                    Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Selected City Hall: {building.name}");
                    break;
                }
            }
            
            // If City Hall wasn't found, pick a random public building
            if (selectedBuilding == null)
            {
                int randomIndex = UnityEngine.Random.Range(0, publicBuildings.Count);
                selectedBuilding = publicBuildings[randomIndex];
                Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Selected random public building: {selectedBuilding.name}");
            }

            // Find bathroom rooms in the selected building
            List<NewRoom> bathroomRooms = new List<NewRoom>();
            List<NewRoom> allRooms = new List<NewRoom>();
            
            // Check if the building has a floor
            if (selectedBuilding.floor != null)
            {
                // Get all rooms in the building
                var il2cppRooms = selectedBuilding.rooms;
                // Convert Il2CppSystem.Collections.Generic.List to System.Collections.Generic.List
                if (il2cppRooms != null)
                {
                    Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Building {selectedBuilding.name} has {il2cppRooms.Count} rooms");
                    for (int i = 0; i < il2cppRooms.Count; i++)
                    {
                        allRooms.Add(il2cppRooms[i]);
                    }
                }
                
                // Log all rooms and their presets
                Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] === LISTING ALL ROOMS IN {selectedBuilding.name} ===");
                foreach (var room in allRooms)
                {
                    string presetName = room.preset != null ? room.preset.name : "null";
                    Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Room: {room.name}, Preset: {presetName}");
                }
                Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] === END OF ROOM LIST ===");
                
                foreach (var room in allRooms)
                {
                    // Skip rooms that aren't in this building
                    if (room == null || room.name == null || room.floor != selectedBuilding.floor) continue;
                    
                    // Check if this is a public bathroom by checking its preset
                    string presetName = room.preset != null ? room.preset.name : "null";
                    
                    // Log the room we're checking
                    Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Checking room: {room.name}, Preset: {presetName}");
                    
                    if (room.preset != null && room.preset.name != null && 
                        (room.preset.name.Contains("BathroomFemale") || 
                         room.preset.name.Contains("BathroomMale") || 
                         room.preset.name.Contains("Building Bathroom") || 
                         room.preset.name.Contains("PublicBathroom") || 
                         room.preset.name.Contains("Bathroom") || 
                         room.preset.name.Contains("bathroom") || 
                         room.preset.name.Contains("Toilet") || 
                         room.preset.name.Contains("toilet") ||
                         room.preset.name.Contains("WC") ||
                         room.preset.name.Contains("Restroom")))
                    {
                        Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Found bathroom room: {room.name}");
                        bathroomRooms.Add(room);
                    }
                }
            }

            // If we couldn't find bathrooms by preset, try finding them by name as a fallback
            if (bathroomRooms.Count == 0)
            {
                Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] No bathroom rooms found by preset, trying by name...");
                
                // Try again with all rooms, but check by name
                foreach (var room in allRooms)
                {
                    if (room == null || room.name == null) continue;
                    
                    // Check if this is a bathroom by name
                    if (room.name.ToLower().Contains("bathroom") || 
                        room.name.ToLower().Contains("restroom") || 
                        room.name.ToLower().Contains("toilet") ||
                        room.name.ToLower().Contains("wc") ||
                        room.name.ToLower().Contains("public bathrooms"))
                    {
                        Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Found bathroom room by name: {room.name}");
                        bathroomRooms.Add(room);
                    }
                }
            }
            
            // If we still couldn't find any bathrooms, try to find ANY room as a last resort
            if (bathroomRooms.Count == 0)
            {
                Plugin.Log.LogWarning($"[SpawnItemCityHallBathroom] Could not find any bathroom rooms in the selected building. Using any available room.");
                
                // Just use any room in the building as a last resort
                if (allRooms.Count > 0)
                {
                    // Pick a random room
                    int randomIndex = UnityEngine.Random.Range(0, allRooms.Count);
                    bathroomRooms.Add(allRooms[randomIndex]);
                    Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Using random room as fallback: {allRooms[randomIndex].name}");
                }
                else
                {
                    Plugin.Log.LogError($"[SpawnItemCityHallBathroom] No rooms found in the building at all.");
                    return null;
                }
            }

            // Choose a random bathroom room
            int randomRoomIndex = UnityEngine.Random.Range(0, bathroomRooms.Count);
            NewRoom selectedRoom = bathroomRooms[randomRoomIndex];
            
            Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Selected bathroom room: {selectedRoom.name}");
            
            // Find a node in the bathroom room
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
                    // Pick a random node in the bathroom
                    int randomNodeIndex = UnityEngine.Random.Range(0, nodesList.Count);
                    placementNode = nodesList[randomNodeIndex];
                    spawnPosition = placementNode.position;
                    Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Using node in bathroom room: {placementNode}");
                }
                else
                {
                    Plugin.Log.LogWarning($"[SpawnItemCityHallBathroom] No nodes found in selected room.");
                    return null;
                }
            }
            else
            {
                Plugin.Log.LogWarning($"[SpawnItemCityHallBathroom] No nodes found in selected room.");
                return null;
            }
            
            // Add a small offset to ensure it's visible
            spawnPosition.y += 0.05f;
            
            // Add some randomization to the position
            spawnPosition.x += UnityEngine.Random.Range(-0.1f, 0.1f);
            spawnPosition.z += UnityEngine.Random.Range(-0.1f, 0.1f);

            Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Calculated spawn position: {spawnPosition}");

            // Make sure we have a valid node for placement
            if (placementNode == null)
            {
                Plugin.Log.LogWarning($"[SpawnItemCityHallBathroom] Could not find a valid node for placement.");
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
                
                Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Using random rotation: {randomRotation}");
                
                // Create the item in the bathroom cubicle
                Interactable spawnedItem = InteractableCreator.Instance.CreateWorldInteractable(
                    itemPreset,                // The item preset
                    owner,                     // The owner of the item
                    owner,                     // The writer (same as owner)
                    recipient,                 // The receiver
                    spawnPosition,             // The position on the cubicle
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
                    
                    Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Successfully created item in City Hall bathroom");
                    Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Item position: {spawnedItem.wPos}, node: {(spawnedItem.node != null ? spawnedItem.node.ToString() : "null")}");
                    
                    return spawnedItem;
                }
                else
                {
                    Plugin.Log.LogError($"[SpawnItemCityHallBathroom] Failed to create item in City Hall bathroom");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemCityHallBathroom] Error creating item: {ex.Message}");
                Plugin.Log.LogError($"[SpawnItemCityHallBathroom] Stack trace: {ex.StackTrace}");
                return null;
            }
        }
    }
}
