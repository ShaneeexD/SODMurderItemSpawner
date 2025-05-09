using System;
using System.Collections.Generic;
using SOD.Common;
using UnityEngine;

namespace MurderItemSpawner
{
    public class SpawnItemBuildingEntranceHome : MonoBehaviour
    {
        private static SpawnItemBuildingEntranceHome _instance;
        private static Vector3 spawnPosition;
        private static SpawnItemBuildingEntranceHome Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SpawnItemBuildingEntranceHome_Instance");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<SpawnItemBuildingEntranceHome>();
                }
                return _instance;
            }
        }

        // Method to spawn an item at a building entrance
        public static void SpawnItemAtLocation(Human owner, Human recipient, string presetName, float spawnChance, SubLocationTypeBuildingEntrances subLocationTypeBuildingEntrances)
        {
            try
            {
                // Check if we should spawn based on chance
                float randomValue = UnityEngine.Random.Range(0f, 1f);
                if (randomValue > spawnChance)
                {
                    Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Skipping spawn of {presetName} due to chance (roll: {randomValue}, needed: <= {spawnChance})");
                    return;
                }

                // Get the interactable preset
                InteractablePreset interactablePresetItem = Toolbox.Instance.GetInteractablePreset(presetName);
                if (interactablePresetItem == null)
                {
                    Plugin.Log.LogError($"[SpawnItemBuildingEntrance] Could not find interactable preset with name {presetName}");
                    return;
                }
                // Get the recipient's address (where to spawn the item)
                if (recipient == null || recipient.home == null)
                {
                    Plugin.Log.LogWarning($"[SpawnItemBuildingEntrance] Recipient has no valid address. Cannot spawn {presetName}");
                    return;
                }

                NewAddress recipientAddress = recipient.home;
                Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Owner: {owner.name}, Recipient: {recipient.name}, Address: {recipientAddress.name}");

                // Find the building entrance and spawn the item
                Interactable spawnedItem = SpawnItemAtBuildingEntrance(recipientAddress, interactablePresetItem, owner, recipient, presetName, spawnChance, subLocationTypeBuildingEntrances);
                
                if (spawnedItem != null)
                {
                    // Ensure the item is owned by the correct person
                    spawnedItem.SetOwner(owner);
                    Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Successfully spawned '{presetName}' at building entrance. Item node: {(spawnedItem.node != null ? spawnedItem.node.ToString() : "null")}");
                    Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Item '{presetName}' final world position: {spawnedItem.wPos}");
                }
                else
                {
                    Plugin.Log.LogError($"[SpawnItemBuildingEntrance] Failed to create item '{presetName}' at building entrance.");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemBuildingEntrance] Error spawning item {presetName}: {ex.Message}");
                Plugin.Log.LogError($"[SpawnItemBuildingEntrance] Stack trace: {ex.StackTrace}");
            }
        }

        // Method to spawn an item at a building entrance
        private static Interactable SpawnItemAtBuildingEntrance(NewAddress address, InteractablePreset itemPreset, Human owner, Human recipient, string itemNameForLog, float spawnChance, SubLocationTypeBuildingEntrances subLocationTypeBuildingEntrances)
        {
            if (address == null)
            {
                Plugin.Log.LogWarning($"[SpawnItemBuildingEntrance] Address is null for {itemNameForLog}.");
                return null;
            }

            // First, try to get the building directly from the address
            var building = address.building;
            if (building != null)
            {
                Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Found building directly from address: {building.name}");
            }
            else
            {
                Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] No building found directly from address, trying floor");
                
                // Fall back to getting the floor
                if (address.floor != null)
                {
                    Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Found floor for address {address.name}");
                }
                else
                {
                    Plugin.Log.LogWarning($"[SpawnItemBuildingEntrance] Could not find floor for address {address.name}");
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
                Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Found main entrance directly from building");
            }
            // If not, check additional entrances from the building
            else if (building != null && building.additionalEntrances != null && building.additionalEntrances.Count > 0)
            {
                Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Checking {building.additionalEntrances.Count} additional entrances from building");
                
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
                        Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Found street entrance in building's additional entrances");
                        break;
                    }
                }
            }
            
            // If we still don't have a main entrance, look for building entrances on the floor
            if (mainEntranceWall == null && address.floor != null && address.floor.buildingEntrances != null && address.floor.buildingEntrances.Count > 0)
            {
                Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Checking {address.floor.buildingEntrances.Count} entrances from floor");
                
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
                        Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Found street entrance in building entrances");
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
                    Plugin.Log.LogWarning($"[SpawnItemBuildingEntrance] No entrances found for {address.name}");
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
                        Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Found street entrance from address entrances for {address.name}");
                        break;
                    }
                }
            }
            
            // If we found a main entrance to a street, use it
            if (mainEntranceWall != null)
            {
                entranceWall = mainEntranceWall;
                Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Using main entrance to street for building");
            }
            // Otherwise fall back to the first entrance of the address
            else if (address.entrances != null && address.entrances.Count > 0)
            {
                entranceWall = address.entrances[0].wall;
                Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] No street entrance found, using first address entrance for {address.name}");
            }
            else
            {
                Plugin.Log.LogWarning($"[SpawnItemBuildingEntrance] No entrances found for building or address");
                return null;
            }
            
            if (entranceWall == null)
            {
                Plugin.Log.LogWarning($"[SpawnItemBuildingEntrance] Entrance wall is null for {address.name}");
                return null;
            }
            
            Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Using entrance for {address.name}");
            
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
                    Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Found street node on otherWall side for {address.name}");
                }
                // Also accept if it's just outside the address
                else if (entranceNode.gameLocation != address)
                {
                    isStreetNode = true;
                    Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Found outside node on otherWall side for {address.name}");
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
                        Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Found street node on wall side for {address.name}");
                    }
                    // Also accept if it's just outside the address
                    else if (entranceNode.gameLocation != address)
                    {
                        isStreetNode = true;
                        Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Found outside node on wall side for {address.name}");
                    }
                }
            }
            
            // If we still don't have a valid node outside the building
            if (!isStreetNode)
            {
                Plugin.Log.LogWarning($"[SpawnItemBuildingEntrance] Could not find a valid street node for the entrance of {address.name}");
                return null;
            }
            
            Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Found entrance node at {entranceNode.nodeCoord} for {address.name}");
            
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
                float offsetDistance = -2.0f; // Distance from the entrance
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
                spawnPosition.x += UnityEngine.Random.Range(-0.5f, 0.5f);
            }
            
            // Add a small height offset to ensure it's not on the ground
            
            
            Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Entrance position: {entrancePosition}, Wall direction: {wallDirection}");
            Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Calculated spawn position: {spawnPosition}");
            
            // Create a list of passed variables for the room ID
            Il2CppSystem.Collections.Generic.List<Interactable.Passed> passedVars = new Il2CppSystem.Collections.Generic.List<Interactable.Passed>();
            passedVars.Add(new Interactable.Passed(Interactable.PassedVarType.roomID, entranceNode.room.roomID, null));
            
            try
            {
                // Create a random rotation (0-360 degrees on Y axis)
                float randomYRotation = UnityEngine.Random.Range(0f, 360f);
                Vector3 randomRotation = new Vector3(0f, randomYRotation, 0f);
                
                Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Using random rotation: {randomRotation}");
                
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
                    
                    Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Successfully created item at building entrance");
                    Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Item position: {spawnedItem.wPos}, node: {(spawnedItem.node != null ? spawnedItem.node.ToString() : "null")}");
                    
                    return spawnedItem;
                }
                else
                {
                    Plugin.Log.LogError($"[SpawnItemBuildingEntrance] Failed to create item at building entrance");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemBuildingEntrance] Error creating item: {ex.Message}");
                Plugin.Log.LogError($"[SpawnItemBuildingEntrance] Stack trace: {ex.StackTrace}");
                return null;
            }
        }
    }
}