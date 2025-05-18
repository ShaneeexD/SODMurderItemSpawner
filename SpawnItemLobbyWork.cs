using System;
using System.Collections.Generic;
using SOD.Common;
using UnityEngine;

namespace MurderItemSpawner
{
    public class SpawnItemLobbyWork : MonoBehaviour
    {
        private static SpawnItemLobbyWork _instance;
        private static SpawnItemLobbyWork Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SpawnItemLobbyWork_Instance");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<SpawnItemLobbyWork>();
                }
                return _instance;
            }
        }

        // Method to spawn an item in a workplace lobby
        public static void SpawnItemAtLocation(Human owner, Human recipient, string presetName, float spawnChance = 1.0f, SubLocationTypeBuildingEntrances subLocationTypeBuildingEntrances = SubLocationTypeBuildingEntrances.Inside,
            bool useMultipleOwners = false, List<BelongsTo> owners = null)
        {
            try
            {
                // Check if we should spawn based on chance
                float randomValue = UnityEngine.Random.Range(0f, 1f);
                if (randomValue > spawnChance)
                {
                    Plugin.LogDebug($"[SpawnItemLobbyWork] Skipping spawn of {presetName} due to chance (roll: {randomValue}, needed: <= {spawnChance})");
                    return;
                }

                // Get the interactable preset
                InteractablePreset interactablePresetItem = Toolbox.Instance.GetInteractablePreset(presetName);
                if (interactablePresetItem == null)
                {
                    Plugin.Log.LogError($"[SpawnItemLobbyWork] Could not find interactable preset with name {presetName}");
                    return;
                }

                // Get the recipient's workplace address (where to spawn the item)
                if (recipient == null)
                {
                    Plugin.Log.LogWarning($"[SpawnItemLobbyWork] Recipient is null. Cannot spawn {presetName}");
                    return;
                }
                
                // Check if recipient has a job with a valid workplace address
                if (recipient.job == null || recipient.job.employer == null || recipient.job.employer.placeOfBusiness == null || recipient.job.employer.placeOfBusiness.thisAsAddress == null)
                {
                    // Fallback to home lobby if workplace is not available
                    Plugin.LogDebug($"[SpawnItemLobbyWork] Recipient {recipient.name} has no valid workplace. Falling back to home lobby.");
                    
                    // Call the home lobby spawner instead
                    SpawnItemLobbyHome.SpawnItemAtLocation(owner, recipient, presetName, spawnChance);
                    return;
                }

                NewAddress workplaceAddress = recipient.job.employer.placeOfBusiness.thisAsAddress;
                Plugin.LogDebug($"[SpawnItemLobbyWork] Owner: {owner.name}, Recipient: {recipient.name}, Workplace: {workplaceAddress.name}");

                // Find the lobby and spawn the item
                Interactable spawnedItem = SpawnItemInLobby(workplaceAddress, interactablePresetItem, owner, recipient, presetName, subLocationTypeBuildingEntrances);
                
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
                                Plugin.LogDebug($"[SpawnItemLobbyWork] Adding fingerprint for {ownerType}");
                                // Add the fingerprint with default life parameter
                                spawnedItem.AddNewDynamicFingerprint(additionalOwner, Interactable.PrintLife.timed);
                            }
                            else
                            {
                                Plugin.LogDebug($"[SpawnItemLobbyWork] Could not add fingerprint for {ownerType} - Human not found");
                            }
                        }
                        
                        Plugin.LogDebug($"[SpawnItemLobbyWork] Successfully added multiple owners to '{presetName}'");
                    }
                    else
                    {
                        // Standard single owner
                        spawnedItem.SetOwner(owner);
                    }
                    
                    Plugin.LogDebug($"[SpawnItemLobbyWork] Successfully spawned '{presetName}' in workplace lobby. Item node: {(spawnedItem.node != null ? spawnedItem.node.ToString() : "null")}");
                    Plugin.LogDebug($"[SpawnItemLobbyWork] Item '{presetName}' final world position: {spawnedItem.wPos}");
                }
                else
                {
                    Plugin.Log.LogError($"[SpawnItemLobbyWork] Failed to create item '{presetName}' in workplace lobby.");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemLobbyWork] Error spawning item {presetName}: {ex.Message}");
                Plugin.Log.LogError($"[SpawnItemLobbyWork] Stack trace: {ex.StackTrace}");
            }
        }

        // Method to spawn an item in a workplace lobby
        private static Interactable SpawnItemInLobby(NewAddress address, InteractablePreset itemPreset, Human owner, Human recipient, string itemNameForLog, SubLocationTypeBuildingEntrances subLocationTypeBuildingEntrances)
        {
            if (address == null)
            {
                Plugin.Log.LogWarning($"[SpawnItemLobbyWork] Address is null for {itemNameForLog}.");
                return null;
            }

            // Try to find the lobby area in the workplace
            NewNode lobbyNode = null;
            
            // Try to find a suitable node for the lobby
            // First, check if we can find a node from the entrances
            if (address.entrances != null && address.entrances.Count > 0)
            {
                // Get the first entrance's inside node
                var entrance = address.entrances[0];
                if (entrance != null && entrance.wall != null && entrance.wall.node != null)
                {
                    lobbyNode = entrance.wall.node;
                    Plugin.LogDebug($"[SpawnItemLobbyWork] Using entrance node as lobby for workplace {address.name}");
                }
            }
            
            // If we didn't find a specific lobby node, try to find a node near the entrance
            if (lobbyNode == null && address.entrances != null && address.entrances.Count > 0)
            {
                // Get the first entrance wall
                NewWall entranceWall = address.entrances[0].wall;
                if (entranceWall != null && entranceWall.node != null)
                {
                    lobbyNode = entranceWall.node;
                    Plugin.LogDebug($"[SpawnItemLobbyWork] Using entrance node as lobby for workplace {address.name}");
                }
            }
            
            // If we still don't have a node and the address has a floor
            if (lobbyNode == null && address.floor != null)
            {
                // If we can't find a specific node, we'll create an item at the floor's center position
                // We'll use the entrance node but with a modified position
                if (address.entrances != null && address.entrances.Count > 0 && 
                    address.entrances[0].wall != null && address.entrances[0].wall.node != null)
                {
                    lobbyNode = address.entrances[0].wall.node;
                    Plugin.LogDebug($"[SpawnItemLobbyWork] Using entrance node with modified position as fallback for workplace {address.name}");
                }
            }
            
            // If we couldn't find any suitable node, return null
            if (lobbyNode == null)
            {
                Plugin.Log.LogWarning($"[SpawnItemLobbyWork] Could not find a suitable node for workplace {address.name}");
                return null;
            }
            
            // Calculate spawn position in the lobby
            Vector3 spawnPosition = lobbyNode.position;
            
            // Add some randomization to the position

            spawnPosition.y += 0.0f;
            spawnPosition.z += 0.0f;
            spawnPosition.x += UnityEngine.Random.Range(-0.0f, 0.0f);

            
            Plugin.LogDebug($"[SpawnItemLobbyWork] Lobby node position: {lobbyNode.position}");
            Plugin.LogDebug($"[SpawnItemLobbyWork] Calculated spawn position: {spawnPosition}");
            
            // Create a list of passed variables for the room ID
            Il2CppSystem.Collections.Generic.List<Interactable.Passed> passedVars = new Il2CppSystem.Collections.Generic.List<Interactable.Passed>();
            passedVars.Add(new Interactable.Passed(Interactable.PassedVarType.roomID, lobbyNode.room.roomID, null));
            
            try
            {
                // Create a random rotation (0-360 degrees on Y axis)
                float randomYRotation = UnityEngine.Random.Range(0f, 360f);
                Vector3 randomRotation = new Vector3(0f, randomYRotation, 0f);
                
                Plugin.LogDebug($"[SpawnItemLobbyWork] Using random rotation: {randomRotation}");
                
                // Create the item in the lobby
                Interactable spawnedItem = InteractableCreator.Instance.CreateWorldInteractable(
                    itemPreset,                // The item preset
                    owner,                     // The owner of the item
                    owner,                     // The writer (same as owner)
                    recipient,                 // The receiver
                    spawnPosition,             // The position in the lobby
                    randomRotation,            // Random rotation on Y axis (0-360 degrees)
                    passedVars,                // Passed variables with room ID
                    null,                      // No passed object
                    ""                         // No load GUID
                );
                
                if (spawnedItem != null)
                {
                    // Set the node to the lobby node
                    spawnedItem.node = lobbyNode;
                    
                    // Update the item's position and node
                    spawnedItem.UpdateWorldPositionAndNode(true, true);
                    
                    Plugin.LogDebug($"[SpawnItemLobbyWork] Successfully created item in workplace lobby");
                    Plugin.LogDebug($"[SpawnItemLobbyWork] Item position: {spawnedItem.wPos}, node: {(spawnedItem.node != null ? spawnedItem.node.ToString() : "null")}");
                    
                    return spawnedItem;
                }
                else
                {
                    Plugin.Log.LogError($"[SpawnItemLobbyWork] Failed to create item in workplace lobby");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemLobbyWork] Error creating item: {ex.Message}");
                Plugin.Log.LogError($"[SpawnItemLobbyWork] Stack trace: {ex.StackTrace}");
                return null;
            }
        }
    }
}
