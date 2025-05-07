using System;
using System.Collections.Generic;
using SOD.Common;
using UnityEngine;

namespace MurderCult
{
    public class SpawnItemBuildingEntrance : MonoBehaviour
    {
        private static SpawnItemBuildingEntrance _instance;
        private static SpawnItemBuildingEntrance Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SpawnItemBuildingEntrance_Instance");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<SpawnItemBuildingEntrance>();
                }
                return _instance;
            }
        }

        // Method to spawn an item at a building entrance
        public static void SpawnItemAtLocation(Human owner, Human recipient, string presetName, float spawnChance = 1.0f)
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
                Interactable spawnedItem = SpawnItemAtBuildingEntrance(recipientAddress, interactablePresetItem, owner, recipient, presetName);
                
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
        private static Interactable SpawnItemAtBuildingEntrance(NewAddress address, InteractablePreset itemPreset, Human owner, Human recipient, string itemNameForLog)
        {
            if (address == null)
            {
                Plugin.Log.LogWarning($"[SpawnItemBuildingEntrance] Address is null for {itemNameForLog}.");
                return null;
            }

            // Find entrances to the building
            var entrances = address.entrances;
            
            if (entrances == null || entrances.Count == 0)
            {
                Plugin.Log.LogWarning($"[SpawnItemBuildingEntrance] No entrances found for {address.name}");
                return null;
            }
            
            // Get the first entrance wall
            NewWall entranceWall = entrances[0].wall;
            if (entranceWall == null)
            {
                Plugin.Log.LogWarning($"[SpawnItemBuildingEntrance] Entrance wall is null for {address.name}");
                return null;
            }
            
            Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Found entrance for {address.name}");
            
            // Get the node on the street side of the entrance
            NewNode entranceNode = entranceWall.otherWall.node;
            bool isStreetNode = false;
            
            // Check if the node is on a street (streets have different game location than the address)
            if (entranceNode != null && entranceNode.gameLocation != null && entranceNode.gameLocation != address)
            {
                isStreetNode = true;
            }
            
            // If not a street node, try the other side
            if (!isStreetNode)
            {
                entranceNode = entranceWall.node;
                if (entranceNode != null && entranceNode.gameLocation != null && entranceNode.gameLocation != address)
                {
                    isStreetNode = true;
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
            float offsetDistance = 1.0f; // Distance from the entrance
            Vector3 spawnPosition = entrancePosition + (wallDirection * offsetDistance);
            
            // Add a small height offset to ensure it's not on the ground
            spawnPosition.y += 0.0f;
            
            Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Entrance position: {entrancePosition}, Wall direction: {wallDirection}");
            Plugin.Log.LogInfo($"[SpawnItemBuildingEntrance] Calculated spawn position: {spawnPosition}");
            
            // Create a list of passed variables for the room ID
            Il2CppSystem.Collections.Generic.List<Interactable.Passed> passedVars = new Il2CppSystem.Collections.Generic.List<Interactable.Passed>();
            passedVars.Add(new Interactable.Passed(Interactable.PassedVarType.roomID, entranceNode.room.roomID, null));
            
            try
            {
                // Create the item at the entrance
                Interactable spawnedItem = InteractableCreator.Instance.CreateWorldInteractable(
                    itemPreset,                // The item preset
                    owner,                     // The owner of the item
                    owner,                     // The writer (same as owner)
                    recipient,                     // The receiver
                    spawnPosition,             // The position near the entrance
                    Quaternion.LookRotation(wallDirection).eulerAngles, // Rotation facing outward from the wall
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