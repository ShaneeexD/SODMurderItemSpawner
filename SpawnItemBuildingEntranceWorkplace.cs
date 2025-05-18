using System;
using System.Collections.Generic;
using SOD.Common;
using UnityEngine;

namespace MurderItemSpawner
{
    public class SpawnItemBuildingEntranceWorkplace : MonoBehaviour
    {
        private static SpawnItemBuildingEntranceWorkplace _instance;
        private static Vector3 spawnPosition;
        private static SpawnItemBuildingEntranceWorkplace Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SpawnItemBuildingEntranceWorkplace_Instance");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<SpawnItemBuildingEntranceWorkplace>();
                }
                return _instance;
            }
        }

        // Method to spawn an item at a workplace building entrance
        public static void SpawnItemAtLocation(Human owner, Human recipient, string presetName, float spawnChance, SubLocationTypeBuildingEntrances subLocationTypeBuildingEntrances,
            bool useMultipleOwners = false, List<BelongsTo> owners = null)
        {
            try
            {
                // Check if we should spawn based on chance
                float randomValue = UnityEngine.Random.Range(0f, 1f);
                if (randomValue > spawnChance)
                {
                    Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Skipping spawn of {presetName} due to chance (roll: {randomValue}, needed: <= {spawnChance})");
                    return;
                }

                // Get the interactable preset
                InteractablePreset interactablePresetItem = Toolbox.Instance.GetInteractablePreset(presetName);
                if (interactablePresetItem == null)
                {
                    Plugin.Log.LogError($"[SpawnItemBuildingEntranceWorkplace] Could not find interactable preset with name {presetName}");
                    return;
                }

                // Get the recipient's workplace address (where to spawn the item)
                if (recipient == null)
                {
                    Plugin.Log.LogWarning($"[SpawnItemBuildingEntranceWorkplace] Recipient is null. Cannot spawn {presetName}");
                    return;
                }
                
                // Check if recipient has a job with a valid workplace address
                if (recipient.job == null || recipient.job.employer == null || recipient.job.employer.address == null)
                {
                    // Fallback to home entrance if workplace is not available
                    Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Recipient {recipient.name} has no valid workplace. Falling back to home building entrance.");
                    
                    // Call the home building entrance spawner instead
                    SpawnItemBuildingEntranceHome.SpawnItemAtLocation(owner, recipient, presetName, spawnChance, subLocationTypeBuildingEntrances,
                        useMultipleOwners, owners);
                    return;
                }

                NewAddress workplaceAddress = recipient.job.employer.address;
                Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Owner: {owner.name}, Recipient: {recipient.name}, Workplace: {workplaceAddress.name}");

                // Find the building entrance and spawn the item
                Interactable spawnedItem = SpawnItemAtBuildingEntrance(workplaceAddress, interactablePresetItem, owner, recipient, presetName, spawnChance, subLocationTypeBuildingEntrances);
                
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
                                Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Adding fingerprint for {ownerType}");
                                // Add the fingerprint with default life parameter
                                spawnedItem.AddNewDynamicFingerprint(additionalOwner, Interactable.PrintLife.timed);
                            }
                            else
                            {
                                Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Could not add fingerprint for {ownerType} - Human not found");
                            }
                        }
                        
                        Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Successfully added multiple owners to '{presetName}'");
                    }
                    else
                    {
                        // Standard single owner
                        spawnedItem.SetOwner(owner);
                    }
                    
                    Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Successfully spawned '{presetName}' at workplace building entrance. Item node: {(spawnedItem.node != null ? spawnedItem.node.ToString() : "null")}");
                    Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Item '{presetName}' final world position: {spawnedItem.wPos}");
                }
                else
                {
                    Plugin.Log.LogError($"[SpawnItemBuildingEntranceWorkplace] Failed to create item '{presetName}' at workplace building entrance.");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemBuildingEntranceWorkplace] Error spawning item {presetName}: {ex.Message}");
                Plugin.Log.LogError($"[SpawnItemBuildingEntranceWorkplace] Stack trace: {ex.StackTrace}");
            }
        }

        // Method to spawn an item at a workplace building entrance
        private static Interactable SpawnItemAtBuildingEntrance(NewAddress address, InteractablePreset itemPreset, Human owner, Human recipient, string itemNameForLog, float spawnChance, SubLocationTypeBuildingEntrances subLocationTypeBuildingEntrances)
        {
            if (address == null)
            {
                Plugin.Log.LogWarning($"[SpawnItemBuildingEntranceWorkplace] Address is null for {itemNameForLog}.");
                return null;
            }

            // First, try to get the building directly from the address
            var building = address.building;
            if (building != null)
            {
                Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Found building directly from workplace address: {building.name}");
            }
            else
            {
                Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] No building found directly from workplace address, trying floor");
                
                // Fall back to getting the floor
                if (address.floor != null)
                {
                    Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Found floor for workplace address {address.name}");
                }
                else
                {
                    Plugin.Log.LogWarning($"[SpawnItemBuildingEntranceWorkplace] Could not find floor for workplace address {address.name}");
                    return null;
                }
            }
            
            // Now find the building's main entrance that connects to a street
            NewWall entranceWall = null;
            NewWall mainEntranceWall = null;
            
            // First check if we can get the main entrance directly from the building
            if (building != null && building.mainEntrance != null)
            {
                mainEntranceWall = building.mainEntrance;
                Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Found main entrance directly from workplace building");
            }
            // If not, check additional entrances from the building
            else if (building != null && building.additionalEntrances != null && building.additionalEntrances.Count > 0)
            {
                Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Checking {building.additionalEntrances.Count} additional entrances from workplace building");
                
                // Try to find an entrance that connects to a street
                foreach (var entrance in building.additionalEntrances)
                {
                    if (entrance == null) continue;
                    
                    // Check if this entrance connects to a street
                    NewNode streetNode = entrance.otherWall.node;
                    if (streetNode != null && streetNode.gameLocation != null && 
                        streetNode.gameLocation.thisAsStreet != null)
                    {
                        mainEntranceWall = entrance;
                        Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Found street entrance in workplace building's additional entrances");
                        break;
                    }
                }
            }
            
            // If we still don't have a main entrance, look for building entrances on the floor
            if (mainEntranceWall == null && address.floor != null && address.floor.buildingEntrances != null && address.floor.buildingEntrances.Count > 0)
            {
                Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Checking {address.floor.buildingEntrances.Count} entrances from workplace floor");
                
                // Try to find an entrance that connects to a street
                foreach (var entrance in address.floor.buildingEntrances)
                {
                    if (entrance == null) continue;
                    
                    // Check if this entrance connects to a street
                    NewNode streetNode = entrance.otherWall.node;
                    if (streetNode != null && streetNode.gameLocation != null && 
                        streetNode.gameLocation.thisAsStreet != null)
                    {
                        mainEntranceWall = entrance;
                        Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Found street entrance in workplace building entrances");
                        break;
                    }
                }
            }
            
            // If we still don't have a main entrance, fall back to the address entrances
            if (mainEntranceWall == null)
            {
                var entrances = address.entrances;
                
                if (entrances == null || entrances.Count == 0)
                {
                    Plugin.Log.LogWarning($"[SpawnItemBuildingEntranceWorkplace] No entrances found for workplace {address.name}");
                    return null;
                }
                
                // Try to find an entrance that connects to a street
                foreach (var entrance in entrances)
                {
                    if (entrance.wall == null) continue;
                    
                    // Check if this entrance connects to a street
                    NewNode streetNode = entrance.wall.otherWall.node;
                    if (streetNode != null && streetNode.gameLocation != null && 
                        streetNode.gameLocation.thisAsStreet != null)
                    {
                        mainEntranceWall = entrance.wall;
                        Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Found street entrance from workplace address entrances");
                        break;
                    }
                }
            }
            
            // If we found a main entrance to a street, use it
            if (mainEntranceWall != null)
            {
                entranceWall = mainEntranceWall;
                Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Using main entrance to street for workplace building");
            }
            // Otherwise fall back to the first entrance of the address
            else if (address.entrances != null && address.entrances.Count > 0)
            {
                entranceWall = address.entrances[0].wall;
                Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] No street entrance found, using first workplace address entrance");
            }
            else
            {
                Plugin.Log.LogWarning($"[SpawnItemBuildingEntranceWorkplace] No entrances found for workplace building or address");
                return null;
            }
            
            if (entranceWall == null)
            {
                Plugin.Log.LogWarning($"[SpawnItemBuildingEntranceWorkplace] Entrance wall is null for workplace {address.name}");
                return null;
            }
            
            Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Using entrance for workplace {address.name}");
            
            // Get the node on the street side of the entrance
            NewNode entranceNode = entranceWall.otherWall.node;
            bool isStreetNode = false;
            
            // Check if the node is on a street (streets have different game location than the address)
            if (entranceNode != null && entranceNode.gameLocation != null)
            {
                // Specifically check if this is a street node
                if (entranceNode.gameLocation.thisAsStreet != null)
                {
                    isStreetNode = true;
                    Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Found street node on otherWall side for workplace {address.name}");
                }
                // Also accept if it's just outside the address
                else if (entranceNode.gameLocation != address)
                {
                    isStreetNode = true;
                    Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Found outside node on otherWall side for workplace {address.name}");
                }
            }
            
            // If not a street node, try the other side
            if (!isStreetNode)
            {
                entranceNode = entranceWall.node;
                if (entranceNode != null && entranceNode.gameLocation != null)
                {
                    // Specifically check if this is a street node
                    if (entranceNode.gameLocation.thisAsStreet != null)
                    {
                        isStreetNode = true;
                        Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Found street node on wall side for workplace {address.name}");
                    }
                    // Also accept if it's just outside the address
                    else if (entranceNode.gameLocation != address)
                    {
                        isStreetNode = true;
                        Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Found outside node on wall side for workplace {address.name}");
                    }
                }
            }
            
            // If we still don't have a valid node outside the building
            if (!isStreetNode)
            {
                Plugin.Log.LogWarning($"[SpawnItemBuildingEntranceWorkplace] Could not find a valid street node for the entrance of workplace {address.name}");
                return null;
            }
            
            Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Found entrance node at {entranceNode.nodeCoord} for workplace {address.name}");
            
            // Calculate spawn position - slightly offset from the entrance
            Vector3 entrancePosition = entranceNode.position;
            
            // Calculate direction vector from the wall (from inside to outside)
            Vector3 wallDirection = (entranceWall.otherWall.position - entranceWall.position).normalized;
            if (wallDirection == Vector3.zero)
            {
                // Fallback direction if the calculation fails
                wallDirection = Vector3.forward;
            }
            
            // Calculate an offset position in front of the entrance
            if (subLocationTypeBuildingEntrances == SubLocationTypeBuildingEntrances.Inside)
            {
                float offsetDistance = -3.0f; // Distance from the entrance
                spawnPosition = entrancePosition + (wallDirection * offsetDistance);
                spawnPosition.y += 0.0f;
                spawnPosition.z += UnityEngine.Random.Range(-0.5f, 0.5f);
                spawnPosition.x += UnityEngine.Random.Range(-0.5f, 0.5f);
            }else if (subLocationTypeBuildingEntrances == SubLocationTypeBuildingEntrances.Outside)
            {
                float offsetDistance = 1.0f; // Distance from the entrance
                spawnPosition = entrancePosition + (wallDirection * offsetDistance);
                spawnPosition.y += 0.05f;
                spawnPosition.z += UnityEngine.Random.Range(-0.5f, 0.5f);
                spawnPosition.x += UnityEngine.Random.Range(-0.5f, 0.3f);
            }
            
            Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Entrance position: {entrancePosition}, Wall direction: {wallDirection}");
            Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Calculated spawn position: {spawnPosition}");
            
            // Create a list of passed variables for the room ID
            Il2CppSystem.Collections.Generic.List<Interactable.Passed> passedVars = new Il2CppSystem.Collections.Generic.List<Interactable.Passed>();
            passedVars.Add(new Interactable.Passed(Interactable.PassedVarType.roomID, entranceNode.room.roomID, null));
            
            try
            {
                // Create a random rotation (0-360 degrees on Y axis)
                float randomYRotation = UnityEngine.Random.Range(0f, 360f);
                Vector3 randomRotation = new Vector3(0f, randomYRotation, 0f);
                
                Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Using random rotation: {randomRotation}");
                
                // Create the item at the entrance
                Interactable spawnedItem = InteractableCreator.Instance.CreateWorldInteractable(
                    itemPreset,                // The item preset
                    owner,                     // The owner of the item
                    owner,                     // The writer (same as owner)
                    recipient,                 // The receiver
                    spawnPosition,             // The position near the entrance
                    randomRotation,            // Random rotation on Y axis (0-360 degrees)
                    passedVars,                // Passed variables with room ID
                    null,                      // No passed object
                    ""                         // No load GUID
                );
                
                if (spawnedItem != null)
                {
                    // Set the node to the entrance node
                    spawnedItem.node = entranceNode;
                    
                    // Update the item's position and node
                    spawnedItem.UpdateWorldPositionAndNode(true, true);
                    
                    Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Successfully created item at workplace building entrance");
                    Plugin.LogDebug($"[SpawnItemBuildingEntranceWorkplace] Item position: {spawnedItem.wPos}, node: {(spawnedItem.node != null ? spawnedItem.node.ToString() : "null")}");
                    
                    return spawnedItem;
                }
                else
                {
                    Plugin.Log.LogError($"[SpawnItemBuildingEntranceWorkplace] Failed to create item at workplace building entrance");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemBuildingEntranceWorkplace] Error creating item: {ex.Message}");
                Plugin.Log.LogError($"[SpawnItemBuildingEntranceWorkplace] Stack trace: {ex.StackTrace}");
                return null;
            }
        }
    }
}
