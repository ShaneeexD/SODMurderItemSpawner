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
        public static void SpawnItemAtLocation(Human owner, Human recipient, string presetName, float SpawnChance)
        {
            try
            {
                // Check if we should spawn based on chance
                float randomValue = UnityEngine.Random.Range(0f, 1f);
                if (randomValue > SpawnChance)
                {
                    Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Skipping spawn of {presetName} due to chance (roll: {randomValue}, needed: <= {SpawnChance})");
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
            // Find bathrooms in the Public bathrooms building
            List<NewRoom> bathroomRooms = new List<NewRoom>();
            
            // Get all locations in the city
            CityData cityData = CityData.Instance;
            Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Searching for City Hall bathrooms");
            
            // Find the Public bathrooms building
            NewAddress publicBathrooms = null;
            
            foreach (var location in cityData.gameLocationDirectory)
            {
                if (location == null || location.thisAsAddress == null) continue;
                
                // Look specifically for the Public bathrooms building
                if (location.name != null && location.name.Contains("Public bathrooms"))
                {
                    publicBathrooms = location.thisAsAddress;
                    Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Found Public bathrooms building: {publicBathrooms.name}");
                    break;
                }
            }
            
            // If we couldn't find the Public bathrooms building, try to find City Hall
            if (publicBathrooms == null)
            {
                Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Public bathrooms building not found, looking for City Hall");
                
                foreach (var location in cityData.gameLocationDirectory)
                {
                    if (location == null || location.thisAsAddress == null) continue;
                    
                    if (location.name != null && (location.name.Contains("City Hall") || location.name.Contains("CityHall")))
                    {
                        publicBathrooms = location.thisAsAddress;
                        Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Found City Hall: {publicBathrooms.name}");
                        break;
                    }
                }
            }
            
            // If we still couldn't find either building, give up
            if (publicBathrooms == null)
            {
                Plugin.Log.LogError($"[SpawnItemCityHallBathroom] Could not find Public bathrooms or City Hall.");
                return null;
            }
            
            // Check if the building has rooms
            if (publicBathrooms.rooms == null || publicBathrooms.rooms.Count == 0)
            {
                Plugin.Log.LogError($"[SpawnItemCityHallBathroom] Building {publicBathrooms.name} has no rooms.");
                return null;
            }
            
            // Find all bathrooms in the building
            Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Checking {publicBathrooms.rooms.Count} rooms in {publicBathrooms.name}");
            
            for (int i = 0; i < publicBathrooms.rooms.Count; i++)
            {
                var room = publicBathrooms.rooms[i];
                if (room == null) continue;
                
                string roomName = room.name != null ? room.name : "unnamed";
                string presetName = room.preset != null ? room.preset.name : "no preset";
                string floorName = room.floor != null ? room.floor.name : "unknown floor";
                
                // Check if this is a City Hall ground floor bathroom
                bool isBathroom = false;
                bool isCorrectFloor = false;
                
                // Check if it's the City Hall ground floor
                if (floorName.Contains("CityHall_GroundFloor"))
                {
                    isCorrectFloor = true;
                }
                
                // Only proceed if it's the correct floor
                if (isCorrectFloor)
                {
                    // Check by preset
                    if (room.preset != null && (
                        presetName.Contains("BuildingBathroomMale") || 
                        presetName.Contains("BuildingBathroomFemale") ||
                        presetName.Contains("CorporateCorridoor") ||
                        IsBathroomByPreset(presetName)))
                    {
                        isBathroom = true;
                    }
                    // Check by name - include the corridor
                    else if (roomName.Contains("Public bathrooms") || 
                             roomName.Contains("Bathroom") || 
                             roomName.Contains("Corridor"))
                    {
                        isBathroom = true;
                    }
                }
                
                // Log the room
                Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Room: {roomName}, Preset: {presetName}, Floor: {floorName}, IsBathroom: {isBathroom}, IsCorrectFloor: {isCorrectFloor}");
                
                // Add bathrooms to our list
                if (isBathroom)
                {
                    bathroomRooms.Add(room);
                }
            }
            
            // Log summary
            Plugin.Log.LogInfo($"[SpawnItemCityHallBathroom] Found {bathroomRooms.Count} bathroom rooms in {publicBathrooms.name}");
            
            // If we couldn't find any bathrooms, give up
            if (bathroomRooms.Count == 0)
            {
                Plugin.Log.LogError($"[SpawnItemCityHallBathroom] No bathroom rooms found in {publicBathrooms.name}.");
                return null;
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
            spawnPosition.y += 0.00f;
            
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
        
        // Helper method to check if a preset name indicates a bathroom
        private static bool IsBathroomByPreset(string presetName)
        {
            if (string.IsNullOrEmpty(presetName)) return false;
            
            string lowerName = presetName.ToLower();
            return lowerName.Contains("bathroom") || 
                   lowerName.Contains("bathroomfemale") || 
                   lowerName.Contains("bathroommale") || 
                   lowerName.Contains("building bathroom") || 
                   lowerName.Contains("publicbathroom") || 
                   lowerName.Contains("toilet") || 
                   lowerName.Contains("wc") || 
                   lowerName.Contains("restroom");
        }
    }
}
