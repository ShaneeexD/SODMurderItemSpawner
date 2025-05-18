using System;
using System.Collections.Generic;
using SOD.Common;
using UnityEngine;
using System.Linq;

namespace MurderItemSpawner
{
    public class SpawnItemHotelRooftopBar : MonoBehaviour
    {
        private static SpawnItemHotelRooftopBar _instance;
        private static SpawnItemHotelRooftopBar Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SpawnItemHotelRooftopBar_Instance");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<SpawnItemHotelRooftopBar>();
                }
                return _instance;
            }
        }

        // Method to spawn an item in a City Hall bathroom
        public static void SpawnItemAtLocation(Human owner, Human recipient, string presetName, float SpawnChance, List<string> HotelRooftopBarSubLocations,
            bool useMultipleOwners = false, List<BelongsTo> owners = null)
        {
            try
            {
                // Check if we should spawn based on chance
                float randomValue = UnityEngine.Random.Range(0f, 1f);
                if (randomValue > SpawnChance)
                {
                    Plugin.LogDebug($"[SpawnItemHotelRooftopBar] Skipping spawn of {presetName} due to chance (roll: {randomValue}, needed: <= {SpawnChance})");
                    return;
                }

                // Get the interactable preset
                InteractablePreset interactablePresetItem = Toolbox.Instance.GetInteractablePreset(presetName);
                if (interactablePresetItem == null)
                {
                    Plugin.Log.LogError($"[SpawnItemHotelRooftopBar] Could not find interactable preset with name {presetName}");
                    return;
                }

                Plugin.LogDebug($"[SpawnItemHotelRooftopBar] Owner: {owner.name}, Recipient: {recipient.name}");

                // Find the City Hall and spawn the item
                Interactable spawnedItem = SpawnItemInHotelRooftopBar(interactablePresetItem, owner, recipient, presetName, HotelRooftopBarSubLocations);
                
                if (spawnedItem != null)
                {
                    // Handle ownership based on whether multiple owners are used
                    if (useMultipleOwners && owners != null && owners.Count > 0)
                    {
                        // Set the primary owner first
                        spawnedItem.SetOwner(owner);
                        
                        // Add additional fingerprints for each owner in the list
                        foreach (BelongsTo ownerType in owners)
                        {
                            // Get the Human object for this owner type
                            Human additionalOwner = ConfigManager.Instance.GetOwnerForFingerprint(ownerType);
                            
                            if (additionalOwner != null)
                            {
                                Plugin.LogDebug($"[SpawnItemHotelRooftopBar] Adding fingerprint for {ownerType}");
                                // Add the fingerprint with default life parameter
                                spawnedItem.AddNewDynamicFingerprint(additionalOwner, Interactable.PrintLife.timed);
                            }
                            else
                            {
                                Plugin.LogDebug($"[SpawnItemHotelRooftopBar] Could not add fingerprint for {ownerType} - Human not found");
                            }
                        }
                        
                        Plugin.LogDebug($"[SpawnItemHotelRooftopBar] Successfully added multiple owners to '{presetName}'");
                    }
                    else
                    {
                        // Standard single owner
                        spawnedItem.SetOwner(owner);
                    }
                    
                    Plugin.LogDebug($"[SpawnItemHotelRooftopBar] Successfully spawned '{presetName}' in Hotel Rooftop Bar. Item node: {(spawnedItem.node != null ? spawnedItem.node.ToString() : "null")}");
                    Plugin.LogDebug($"[SpawnItemHotelRooftopBar] Item '{presetName}' final world position: {spawnedItem.wPos}");
                }
                else
                {
                    Plugin.Log.LogError($"[SpawnItemHotelRooftopBar] Failed to create item '{presetName}' in Hotel Rooftop Bar.");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemHotelRooftopBar] Error spawning item {presetName}: {ex.Message}");
                Plugin.Log.LogError($"[SpawnItemHotelRooftopBar] Stack trace: {ex.StackTrace}");
            }
        }

        // Method to spawn an item in a Hotel Rooftop Bar
        private static Interactable SpawnItemInHotelRooftopBar(InteractablePreset itemPreset, Human owner, Human recipient, string itemNameForLog, List<string> HotelRooftopBarSubLocations)
        {
            // Find bathrooms in the Public bathrooms building
            List<NewRoom> rooftopBarRooms = new List<NewRoom>();
            
            // Get all locations in the city
            CityData cityData = CityData.Instance;
            Plugin.LogDebug($"[SpawnItemHotelRooftopBar] Searching for Hotel Rooftop Bar");
            
            // Search for the rooftop bar in all locations
            foreach (var location in cityData.gameLocationDirectory)
            {
                if (location == null || location.thisAsAddress == null) continue;
                
                NewAddress building = location.thisAsAddress;
                Plugin.LogDebug($"[SpawnItemHotelRooftopBar] Checking location: {location.name}");
                
                // Check if this building has rooms
                if (building.rooms == null || building.rooms.Count == 0) continue;
                
                // Check all rooms in this building
                for (int i = 0; i < building.rooms.Count; i++)
                {
                    var room = building.rooms[i];
                    if (room == null) continue;
                    
                    string roomName = room.name != null ? room.name : "unnamed";
                    string presetName = room.preset != null ? room.preset.name : "no preset";
                    string floorName = room.floor != null ? room.floor.name : "unknown floor";
                    string buildingName = building.name != null ? building.name : "unknown building";
                    
                    bool isRooftopBar = false;
                    
                    // Use the custom presets passed from the config
                    List<string> customPresets = HotelRooftopBarSubLocations;
                    
                    // Check if this room's preset matches our criteria
                    if (room.preset != null && IsRooftopBarByPreset(presetName, customPresets))
                    {
                        isRooftopBar = true;
                    }
                    
                    // Log the room
                    Plugin.LogDebug($"[SpawnItemHotelRooftopBar] Room: {roomName}, Preset: {presetName}, Floor: {floorName}, Building: {buildingName}, IsRooftopBar: {isRooftopBar}");
                    
                    // Add rooftop bar rooms to our list
                    if (isRooftopBar)
                    {
                        rooftopBarRooms.Add(room);
                        Plugin.LogDebug($"[SpawnItemHotelRooftopBar] *** FOUND ROOFTOP BAR: {roomName} in {buildingName} ***");
                    }
                }
            }
            
            // Log summary
            Plugin.LogDebug($"[SpawnItemHotelRooftopBar] Found {rooftopBarRooms.Count} Rooftop Bar rooms across all hotel locations");
            
            // If we couldn't find any Rooftop Bar, give up
            if (rooftopBarRooms.Count == 0)
            {
                Plugin.Log.LogError($"[SpawnItemHotelRooftopBar] No Rooftop Bar rooms found in any hotel location.");
                return null;
            }
            
            // Choose a random Rooftop Bar room
            int randomRoomIndex = UnityEngine.Random.Range(0, rooftopBarRooms.Count);
            NewRoom selectedRoom = rooftopBarRooms[randomRoomIndex];
            
            Plugin.LogDebug($"[SpawnItemHotelRooftopBar] Selected Rooftop Bar room: {selectedRoom.name}");
            
            // Find a node in the Rooftop Bar room
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
                    // Pick a random node in the Rooftop Bar
                    int randomNodeIndex = UnityEngine.Random.Range(0, nodesList.Count);
                    placementNode = nodesList[randomNodeIndex];
                    spawnPosition = placementNode.position;
                    Plugin.LogDebug($"[SpawnItemHotelRooftopBar] Using node in Rooftop Bar room: {placementNode}");
                }
                else
                {
                    Plugin.Log.LogWarning($"[SpawnItemHotelRooftopBar] No nodes found in selected room.");
                    return null;
                }
            }
            else
            {
                Plugin.Log.LogWarning($"[SpawnItemHotelRooftopBar] No nodes found in selected room.");
                return null;
            }
            
            // Add a small offset to ensure it's visible
            spawnPosition.y += 0.00f;
            
            // Add some randomization to the position
            spawnPosition.x += UnityEngine.Random.Range(-0.1f, 0.1f);
            spawnPosition.z += UnityEngine.Random.Range(-0.1f, 0.1f);

            Plugin.LogDebug($"[SpawnItemCityHallBathroom] Calculated spawn position: {spawnPosition}");

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
                
                Plugin.LogDebug($"[SpawnItemHotelRooftopBar] Using random rotation: {randomRotation}");
                
                // Create the item in the Rooftop Bar cubicle
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
                    
                    Plugin.LogDebug($"[SpawnItemHotelRooftopBar] Successfully created item in Hotel Rooftop Bar");
                    Plugin.LogDebug($"[SpawnItemHotelRooftopBar] Item position: {spawnedItem.wPos}, node: {(spawnedItem.node != null ? spawnedItem.node.ToString() : "null")}");
                    
                    return spawnedItem;
                }
                else
                {
                    Plugin.Log.LogError($"[SpawnItemHotelRooftopBar] Failed to create item in Hotel Rooftop Bar");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemHotelRooftopBar] Error creating item: {ex.Message}");
                Plugin.Log.LogError($"[SpawnItemHotelRooftopBar] Stack trace: {ex.StackTrace}");
                return null;
            }
        }
        
        // Helper method to check if a preset name indicates a Rooftop Bar
        private static bool IsRooftopBarByPreset(string presetName, List<string> customPresets)
        {
            // First check if we have custom presets defined
            if (customPresets != null && customPresets.Count > 0)
            {
                // Check if the preset name is in our custom list
                return customPresets.Contains(presetName);
            }
            
            // Default presets if no custom ones are defined
            return presetName == "RooftopBar" || presetName == "BarDiningRoom";
        }
    }
}
