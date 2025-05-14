using System;
using System.Collections;
using System.Collections.Generic;
using SOD.Common;
using UnityEngine;

namespace MurderItemSpawner
{
    public class SpawnItemWork
    {
        // Method to spawn an item in the recipient's workplace on furniture, but owned by the owner
        public static void SpawnItemAtLocation(Human owner, Human recipient, string presetName, float spawnChance,
            string targetRoomName = null, bool useFurniture = false, List<string> furniturePresets = null)
        {
            try
            {
                // Check if we should spawn based on chance
                float randomValue = UnityEngine.Random.Range(0f, 1f);
                if (randomValue > spawnChance)
                {
                    Plugin.Log.LogInfo($"[SpawnItemWork] Skipping spawn of {presetName} due to chance (roll: {randomValue}, needed: <= {spawnChance})");
                    return;
                }

                // Get the interactable preset
                InteractablePreset interactablePresetItem = Toolbox.Instance.GetInteractablePreset(presetName);
                if (interactablePresetItem == null)
                {
                    Plugin.Log.LogError($"[SpawnItemWork] Could not find interactable preset with name {presetName}");
                    return;
                }

                // Get the recipient's workplace address (where to spawn the item)
                if (recipient == null || recipient.job.employer.placeOfBusiness == null || recipient.job.employer.placeOfBusiness.thisAsAddress == null)
                {
                    Plugin.Log.LogWarning($"[SpawnItemWork] Recipient has no valid workplace. Cannot spawn {presetName}");
                    return;
                }

                NewAddress recipientWorkplace = recipient.job.employer.placeOfBusiness.thisAsAddress;
                Plugin.Log.LogInfo($"[SpawnItemWork] Owner: {owner.name}, Recipient: {recipient.name}, Workplace: {recipientWorkplace.name}");

                // Find the workplace and spawn the item
                CoroutineHelper.StartCoroutine(SpawnItemInWorkplaceCoroutine(interactablePresetItem, owner, recipient, presetName, 
                    recipientWorkplace, targetRoomName, useFurniture, furniturePresets));
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemWork] Error spawning item {presetName}: {ex.Message}");
                Plugin.Log.LogError($"[SpawnItemWork] Stack trace: {ex.StackTrace}");
            }
        }

        // Coroutine to handle the actual spawning process
        private static IEnumerator SpawnItemInWorkplaceCoroutine(InteractablePreset itemPreset, Human owner, Human recipient, 
            string itemNameForLog, NewAddress recipientWorkplace, string targetRoomName = null, 
            bool useFurniture = false, List<string> furniturePresets = null)
        {
            if (recipientWorkplace == null)
            {
                Plugin.Log.LogWarning($"[SpawnItemWork] Recipient workplace is null for {itemNameForLog}.");
                yield break;
            }

            // Get the building from the address
            NewBuilding building = recipientWorkplace.building;
            if (building == null)
            {
                Plugin.Log.LogWarning($"[SpawnItemWork] Could not find building for workplace {recipientWorkplace.name}");
                yield break;
            }

            Plugin.Log.LogInfo($"[SpawnItemWork] Found building for workplace: {building.name}");

            // Get all rooms in the building
            List<NewRoom> allRooms = new List<NewRoom>();
            
            // Get the specific workplace office/area from the recipient's workplace address
            string addressName = recipientWorkplace.name;
            Plugin.Log.LogInfo($"[SpawnItemWork] Recipient's workplace: {addressName}");
            
            // Extract the office/area prefix from the address name (e.g., "Office 3" or "Police Station")
            string workplacePrefix = "";
            if (!string.IsNullOrEmpty(addressName))
            {
                // Try to extract the workplace prefix - this might need customization based on naming conventions
                int spaceIndex = addressName.IndexOf(' ');
                if (spaceIndex > 0)
                {
                    workplacePrefix = addressName.Substring(0, spaceIndex).Trim();
                    Plugin.Log.LogInfo($"[SpawnItemWork] Extracted workplace prefix: {workplacePrefix}");
                }
                else
                {
                    // If there's no space, use the whole name as the prefix
                    workplacePrefix = addressName;
                    Plugin.Log.LogInfo($"[SpawnItemWork] Using full workplace name as prefix: {workplacePrefix}");
                }
            }
            
            // If we have the recipient's floor, get all rooms on that floor
            if (recipientWorkplace.floor != null)
            {
                Plugin.Log.LogInfo($"[SpawnItemWork] Searching for rooms on floor: {recipientWorkplace.floor.name}");
                
                // Search for rooms in this floor
                foreach (var room in CityData.Instance.roomDirectory)
                {
                    if (room != null && room.floor != null && 
                        room.floor.floorID == recipientWorkplace.floor.floorID)
                    {
                        // For workplaces, we might not have a strict naming convention like apartments
                        // So we'll check if the room name contains the workplace name or is on the same floor
                        if (!string.IsNullOrEmpty(workplacePrefix) && 
                            room.name != null && 
                            room.name.Contains(workplacePrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            allRooms.Add(room);
                            Plugin.Log.LogInfo($"[SpawnItemWork] Added room: {room.name} (matches workplace prefix)");
                        }
                        // If we can't determine the workplace prefix, fall back to just using rooms on the same floor
                        else if (string.IsNullOrEmpty(workplacePrefix) || 
                                (room.name != null && room.name.Contains(addressName, StringComparison.OrdinalIgnoreCase)))
                        {
                            allRooms.Add(room);
                            Plugin.Log.LogInfo($"[SpawnItemWork] Added room: {room.name} (same floor or contains workplace name)");
                        }
                    }
                }
            }
            
            if (allRooms.Count == 0)
            {
                Plugin.Log.LogWarning($"[SpawnItemWork] No rooms found in building {building.name}");
                yield break;
            }
            
            Plugin.Log.LogInfo($"[SpawnItemWork] Found {allRooms.Count} rooms in building {building.name}");
            
            // Filter rooms based on targetRoomName if provided
            List<NewRoom> targetRooms = new List<NewRoom>();
            if (!string.IsNullOrEmpty(targetRoomName))
            {
                foreach (var room in allRooms)
                {
                    if (room.name != null && room.name.Contains(targetRoomName, StringComparison.OrdinalIgnoreCase))
                    {
                        targetRooms.Add(room);
                        Plugin.Log.LogInfo($"[SpawnItemWork] Found matching room: {room.name}");
                    }
                }
                
                if (targetRooms.Count == 0)
                {
                    Plugin.Log.LogWarning($"[SpawnItemWork] No rooms found matching name '{targetRoomName}'. Will use any available room.");
                    targetRooms = allRooms;
                }
            }
            else
            {
                targetRooms = allRooms;
            }
            
            // If we're using furniture, prioritize rooms that have the specified furniture
            List<NewRoom> roomsWithFurniture = new List<NewRoom>();
            
            if (useFurniture)
            {
                // Check each target room for matching furniture
                foreach (var room in targetRooms)
                {
                    // Try to find matching furniture in this room
                    List<FurnitureLocation> matchingFurniture = new List<FurnitureLocation>();
                    
                    if (room.individualFurniture != null && room.individualFurniture.Count > 0)
                    {
                        // Check each piece of furniture in the room
                        foreach (var furnitureLocation in room.individualFurniture)
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
                    
                    if (matchingFurniture.Count > 0)
                    {
                        roomsWithFurniture.Add(room);
                        Plugin.Log.LogInfo($"[SpawnItemWork] Room {room.name} has {matchingFurniture.Count} matching furniture pieces");
                    }
                }
                
                // If we found rooms with matching furniture, only use those
                if (roomsWithFurniture.Count > 0)
                {
                    Plugin.Log.LogInfo($"[SpawnItemWork] Found {roomsWithFurniture.Count} rooms with matching furniture");
                    targetRooms = roomsWithFurniture;
                }
                else
                {
                    Plugin.Log.LogWarning($"[SpawnItemWork] No rooms found with matching furniture. Will use any available room.");
                }
            }
            
            // Select a random room from the target rooms
            if (targetRooms.Count == 0)
            {
                Plugin.Log.LogWarning($"[SpawnItemWork] No target rooms found");
                yield break;
            }
            
            int randomRoomIndex = UnityEngine.Random.Range(0, targetRooms.Count);
            NewRoom selectedRoom = targetRooms[randomRoomIndex];
            Plugin.Log.LogInfo($"[SpawnItemWork] Selected room: {selectedRoom.name}");
            
            // Try to place the item on furniture if requested
            bool usedFurniture = false;
            FurnitureLocation selectedFurniture = null;
            FurniturePreset.SubObject selectedSubObject = null;
            
            if (useFurniture && selectedRoom != null && selectedRoom.individualFurniture != null && selectedRoom.individualFurniture.Count > 0)
            {
                Plugin.Log.LogInfo($"[SpawnItemWork] Attempting to find furniture in room {selectedRoom.name} for item placement");
                
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
                                Plugin.Log.LogInfo($"[SpawnItemWork] Found matching furniture: {furnitureName} contains '{presetName}'");
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
                                Plugin.Log.LogInfo($"[SpawnItemWork] Will place item on furniture: {selectedFurniture.furniture.name}, using subobject index: {suitableSubObjects.IndexOf(selectedSubObject)}");
                            }
                            else
                            {
                                Plugin.Log.LogInfo($"[SpawnItemWork] Selected subobject is already used by another interactable. Will try node placement instead.");
                            }
                        }
                        else
                        {
                            Plugin.Log.LogInfo($"[SpawnItemWork] No suitable subobjects found on furniture {selectedFurniture.furniture.name}. Will try node placement instead.");
                        }
                    }
                    else
                    {
                        Plugin.Log.LogInfo($"[SpawnItemWork] Furniture {selectedFurniture.furniture.name} has no subobjects. Will try node placement instead.");
                    }
                }
                else
                {
                    Plugin.Log.LogInfo($"[SpawnItemWork] No matching furniture found in room {selectedRoom.name}. Will try node placement instead.");
                }
            }
            
            // Fall back to node placement if we couldn't use furniture or it wasn't requested
            NewNode placementNode = null;
            Vector3 spawnPosition = Vector3.zero;
            
            // Skip node placement if we're using furniture
            if (usedFurniture)
            {
                Plugin.Log.LogInfo($"[SpawnItemWork] Using furniture placement, skipping node placement");
            }
            // Otherwise try to use node placement
            else if (selectedRoom.nodes != null && selectedRoom.nodes.Count > 0)
            {
                List<NewNode> nodesList = new List<NewNode>();
                foreach (var node in selectedRoom.nodes)
                {
                    if (node.isInaccessable || node.isObstacle) { Plugin.Log.LogInfo($"[SpawnItemWork] Filtering out node: {node.name} (inaccessible)"); continue; }
                    nodesList.Add(node);
                }
                if (nodesList.Count > 0)
                {
                    int randomNodeIndex = UnityEngine.Random.Range(0, nodesList.Count);
                    placementNode = nodesList[randomNodeIndex];
                    spawnPosition = placementNode.position;
                    Plugin.Log.LogInfo($"[SpawnItemWork] Using node in room: {placementNode}");
                }
                else
                {
                    Plugin.Log.LogWarning($"[SpawnItemWork] No nodes found in selected room.");
                    yield break;
                }
            }
            else if (!usedFurniture) // Only show this warning if we're not using furniture
            {
                Plugin.Log.LogWarning($"[SpawnItemWork] No nodes found in selected room.");
                yield break;
            }
            
            if (!usedFurniture)
            {
                spawnPosition.y += 0.0f;
                spawnPosition.x += UnityEngine.Random.Range(-0.1f, 0.1f);
                spawnPosition.z += UnityEngine.Random.Range(-0.1f, 0.1f);
                Plugin.Log.LogInfo($"[SpawnItemWork] Calculated spawn position: {spawnPosition}");
            }
            
            // Create the interactable based on whether we're using furniture or node placement
            Interactable spawnedItem = null;
            
            try
            {
                // If we're using furniture placement
                if (usedFurniture && selectedFurniture != null && selectedSubObject != null)
                {
                    Plugin.Log.LogInfo($"[SpawnItemWork] Creating item '{itemNameForLog}' on furniture {selectedFurniture.furniture.name} in room {selectedRoom.name}");
                    
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
                        // Explicitly set the owner
                        spawnedItem.SetOwner(owner);
                        
                        Plugin.Log.LogInfo($"[SpawnItemWork] Successfully created furniture-spawned interactable '{itemNameForLog}'");
                        Plugin.Log.LogInfo($"[SpawnItemWork] Item position: {spawnedItem.wPos}, furniture: {selectedFurniture.furniture.name}");
                    }
                    else
                    {
                        Plugin.Log.LogError($"[SpawnItemWork] Failed to create furniture-spawned interactable '{itemNameForLog}'");
                    }
                }
                // If we're using node placement
                else if (placementNode != null)
                {
                    // Instead of trying to check for furniture collisions directly,
                    // let's use a different approach - check if the node is marked as an obstacle
                    // or is inaccessible, which often indicates furniture is there
                    bool nodeIsOccupied = false;
                    
                    if (placementNode.isObstacle || placementNode.isInaccessable)
                    {
                        Plugin.Log.LogInfo($"[SpawnItemWork] Node at {placementNode.position} is marked as {(placementNode.isObstacle ? "an obstacle" : "inaccessible")}");
                        nodeIsOccupied = true;
                    }
                    
                    // Add a small random offset to avoid spawning directly on the node
                    // This helps prevent items from spawning inside furniture even if the node isn't marked properly
                    if (!nodeIsOccupied)
                    {
                        spawnPosition = placementNode.position;
                        spawnPosition.y += 0.0f;
                        Plugin.Log.LogInfo($"[SpawnItemWork] Using node position with small Y offset: {spawnPosition}");
                    }
                    
                    if (nodeIsOccupied)
                    {
                        Plugin.Log.LogWarning($"[SpawnItemWork] Skipping node placement because node is occupied by furniture");
                        yield break;
                    }
                    
                    // Create a list of passed variables for the room ID
                    Il2CppSystem.Collections.Generic.List<Interactable.Passed> passedVars = new Il2CppSystem.Collections.Generic.List<Interactable.Passed>();
                    passedVars.Add(new Interactable.Passed(Interactable.PassedVarType.roomID, placementNode.room.roomID, null));
                    
                    float randomYRotation = UnityEngine.Random.Range(0f, 360f);
                    Vector3 randomRotation = new Vector3(0f, randomYRotation, 0f);
                    Plugin.Log.LogInfo($"[SpawnItemWork] Using random rotation: {randomRotation}");
                    
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
                        // Explicitly set the owner
                        spawnedItem.SetOwner(owner);
                        
                        spawnedItem.node = placementNode;
                        spawnedItem.UpdateWorldPositionAndNode(true, true);
                        Plugin.Log.LogInfo($"[SpawnItemWork] Successfully created item in workplace at node");
                        Plugin.Log.LogInfo($"[SpawnItemWork] Item position: {spawnedItem.wPos}, node: {(spawnedItem.node != null ? spawnedItem.node.ToString() : "null")}");
                    }
                    else
                    {
                        Plugin.Log.LogError($"[SpawnItemWork] Failed to create node-based interactable '{itemNameForLog}'");
                    }
                }
                else
                {
                    Plugin.Log.LogWarning($"[SpawnItemWork] Could not find a valid placement node or furniture in room {selectedRoom.name}");
                    yield break;
                }
                
                if (spawnedItem != null)
                {
                    Plugin.Log.LogInfo($"[SpawnItemWork] Item '{itemNameForLog}' successfully created in {selectedRoom.name}");
                    
                    // Log all furniture in the room for reference
                    if (selectedRoom != null && selectedRoom.individualFurniture != null && selectedRoom.individualFurniture.Count > 0)
                    {
                        Plugin.Log.LogInfo($"[SpawnItemWork] Furniture in room {selectedRoom.name}:");
                        foreach (var furniture in selectedRoom.individualFurniture)
                        {
                            if (furniture != null && furniture.furniture != null)
                            {
                                Plugin.Log.LogInfo($"[SpawnItemWork] - {furniture.furniture.name}");
                            }
                        }
                    }
                    else
                    {
                        Plugin.Log.LogInfo($"[SpawnItemWork] No furniture found in room {selectedRoom.name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemWork] Error spawning item: {ex.Message}");
                Plugin.Log.LogError($"[SpawnItemWork] Stack trace: {ex.StackTrace}");
            }
        }
    }
}
