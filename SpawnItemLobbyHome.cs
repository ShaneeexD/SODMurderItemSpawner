using System;
using System.Collections.Generic;
using SOD.Common;
using UnityEngine;
using UnityEngine.UIElements.StyleSheets;

namespace MurderItemSpawner
{
    public class SpawnItemLobbyHome : MonoBehaviour
    {
        private static SpawnItemLobbyHome _instance;
        private static SpawnItemLobbyHome Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SpawnItemLobbyHome_Instance");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<SpawnItemLobbyHome>();
                }
                return _instance;
            }
        }

        // Method to spawn an item at a location in the lobby
        public static void SpawnItemAtLocation(Human owner, Human recipient, string presetName, float spawnChance = 1.0f)
        {
            try
            {
                // Check if we should spawn based on chance
                float randomValue = UnityEngine.Random.Range(0f, 1f);
                if (randomValue > spawnChance)
                {
                    Plugin.Log.LogInfo($"[SpawnItemLobby] Skipping spawn of {presetName} due to chance (roll: {randomValue}, needed: <= {spawnChance})");
                    return;
                }

                // Get the interactable preset
                InteractablePreset interactablePresetItem = Toolbox.Instance.GetInteractablePreset(presetName);
                if (interactablePresetItem == null)
                {
                    Plugin.Log.LogError($"[SpawnItemLobby] Could not find interactable preset with name {presetName}");
                    return;
                }

                // Get the recipient's address (where to spawn the item)
                if (recipient == null || recipient.home == null)
                {
                    Plugin.Log.LogWarning($"[SpawnItemLobby] Recipient has no valid address. Cannot spawn {presetName}");
                    return;
                }

                NewAddress recipientAddress = recipient.home;
                Plugin.Log.LogInfo($"[SpawnItemLobby] Owner: {owner.name}, Recipient: {recipient.name}, Address: {recipientAddress.name}");

                // Spawn the item using the same approach as the game's SpawnSpareKey method
                Interactable spawnedItem = SpawnItemOnDoormat(recipientAddress, interactablePresetItem, owner, presetName);
                
                spawnedItem.SetOwner(owner);

                if (spawnedItem != null)
                {
                    Plugin.Log.LogInfo($"[SpawnItemLobby] Successfully spawned '{presetName}' on doormat in lobby. Item node: {(spawnedItem.node != null ? spawnedItem.node.ToString() : "null")}");
                    Plugin.Log.LogInfo($"[SpawnItemLobby] Item '{presetName}' final world position: {spawnedItem.wPos}");
                }
                else
                {
                    Plugin.Log.LogError($"[SpawnItemLobby] Failed to create furniture spawned interactable '{presetName}'.");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemLobby] Error spawning item {presetName}: {ex.Message}");
            }
        }

        // Method to spawn an item on a doormat, following the game's SpawnSpareKey approach exactly
        private static Interactable SpawnItemOnDoormat(NewAddress address, InteractablePreset itemPreset, Human owner, string itemNameForLog)
        {
            if (address == null)
            {
                Plugin.Log.LogWarning($"[SpawnItemLobby] Address is null for {itemNameForLog}.");
                return null;
            }

            // Following SpawnSpareKey pattern exactly - initialize lists
            List<NewRoom> lobbyRooms = new List<NewRoom>();
            List<NewRoom> apartmentRooms = new List<NewRoom>();
            List<NewNode> entranceNodes = new List<NewNode>();
            List<FurnitureLocation> doormatLocations = new List<FurnitureLocation>();
            List<FurniturePreset.SubObject> doormatSubObjects = new List<FurniturePreset.SubObject>();
            List<NewRoom> doormatRooms = new List<NewRoom>();

            // Find all lobby entrances to the address
            foreach (NewNode.NodeAccess nodeAccess in address.entrances)
            {
                if (nodeAccess.wall != null)
                {
                    // Check if the node is in a lobby (not the apartment) and is a lobby
                    if (nodeAccess.wall.node.gameLocation != address && nodeAccess.wall.node.gameLocation.isLobby)
                    {
                        if (!lobbyRooms.Contains(nodeAccess.wall.node.room))
                        {
                            lobbyRooms.Add(nodeAccess.wall.node.room);
                            apartmentRooms.Add(nodeAccess.wall.otherWall.node.room);
                            entranceNodes.Add(nodeAccess.wall.node);
                        }
                    }
                    // Check the other side of the wall
                    else if (nodeAccess.wall.otherWall.node.gameLocation != address && 
                             nodeAccess.wall.otherWall.node.gameLocation.isLobby && 
                             !lobbyRooms.Contains(nodeAccess.wall.otherWall.node.room))
                    {
                        lobbyRooms.Add(nodeAccess.wall.otherWall.node.room);
                        apartmentRooms.Add(nodeAccess.wall.node.room);
                        entranceNodes.Add(nodeAccess.wall.otherWall.node);
                    }
                }
            }

            // Find doormats near these entrances
            for (int i = 0; i < lobbyRooms.Count; i++)
            {
                NewRoom lobbyRoom = lobbyRooms[i];
                NewRoom apartmentRoom = apartmentRooms[i];
                NewNode entranceNode = entranceNodes[i];

                int j = 0;
                while (j < lobbyRoom.individualFurniture.Count)
                {
                    FurnitureLocation furnitureLocation = null;
                    try
                    {
                        furnitureLocation = lobbyRoom.individualFurniture[j];
                    }
                    catch
                    {
                        j++;
                        continue;
                    }

                    if (furnitureLocation == null)
                    {
                        j++;
                        continue;
                    }

                    // Check if this furniture has doormat subobjects and is close to the entrance
                    bool hasDoormatSubObjects = false;
                    for (int k = 0; k < furnitureLocation.furniture.subObjects.Count; k++)
                    {
                        if (furnitureLocation.furniture.subObjects[k].preset == InteriorControls.Instance.keyHidingPlace)
                        {
                            hasDoormatSubObjects = true;
                            break;
                        }
                    }

                    if (hasDoormatSubObjects && Vector3.Distance(furnitureLocation.anchorNode.nodeCoord, entranceNode.nodeCoord) <= 2.1f)
                    {
                        // Find all doormat subobjects that aren't already used
                        for (int k = 0; k < furnitureLocation.furniture.subObjects.Count; k++)
                        {
                            FurniturePreset.SubObject subObject = furnitureLocation.furniture.subObjects[k];
                            if (subObject.preset == InteriorControls.Instance.keyHidingPlace)
                            {
                                // Check if this subobject is already used by an interactable
                                bool alreadyUsed = false;
                                for (int m = 0; m < furnitureLocation.integratedInteractables.Count; m++)
                                {
                                    if (furnitureLocation.integratedInteractables[m].subObject == subObject)
                                    {
                                        alreadyUsed = true;
                                        break;
                                    }
                                }

                                if (!alreadyUsed)
                                {
                                    doormatSubObjects.Add(subObject);
                                    doormatLocations.Add(furnitureLocation);
                                    doormatRooms.Add(apartmentRoom);
                                }
                            }
                        }
                    }
                    j++;
                }
            }

            // If we found doormat subobjects, spawn the item on one of them
            if (doormatSubObjects.Count > 0)
            {
                // Select a doormat - for consistency, use the first one
                // The game uses a pseudorandom approach for keys
                int doormatIndex = 0;
                FurniturePreset.SubObject targetSubObject = doormatSubObjects[doormatIndex];
                FurnitureLocation targetLocation = doormatLocations[doormatIndex];
                NewRoom targetRoom = doormatRooms[doormatIndex];

                // Try to find the closest entrance node to calculate rotation
                NewNode entranceNode = null;
                float closestDistance = float.MaxValue;
                foreach (NewNode.NodeAccess nodeAccess in address.entrances)
                {
                    if (nodeAccess.wall != null)
                    {
                        // Check both sides of the wall
                        if (nodeAccess.wall.node.gameLocation != address && nodeAccess.wall.node.gameLocation.isLobby)
                        {
                            float distance = Vector3.Distance(nodeAccess.wall.node.nodeCoord, targetLocation.anchorNode.nodeCoord);
                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                entranceNode = nodeAccess.wall.node;
                            }
                        }
                        else if (nodeAccess.wall.otherWall.node.gameLocation != address && nodeAccess.wall.otherWall.node.gameLocation.isLobby)
                        {
                            float distance = Vector3.Distance(nodeAccess.wall.otherWall.node.nodeCoord, targetLocation.anchorNode.nodeCoord);
                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                entranceNode = nodeAccess.wall.otherWall.node;
                            }
                        }
                    }
                }
                
                // Calculate rotation based on the direction from doormat to entrance
                Quaternion doormatRotation = Quaternion.identity;
                Vector3 doormatPosition = targetLocation.anchorNode.position;
                
                if (entranceNode != null)
                {
                    Vector3 entrancePosition = entranceNode.position;
                    Vector3 directionToEntrance = (entrancePosition - doormatPosition).normalized;
                    doormatRotation = Quaternion.LookRotation(directionToEntrance);
                    Plugin.Log.LogInfo($"[SpawnItemLobby] Calculated rotation based on entrance direction: {doormatRotation.eulerAngles}");
                }
                else
                {
                    Plugin.Log.LogWarning($"[SpawnItemLobby] Could not find an entrance node, using identity rotation");
                }
                
                // Extract rotation vectors
                Vector3 doormatForward = doormatRotation * Vector3.forward;
                Vector3 doormatRight = doormatRotation * Vector3.right;
                Vector3 doormatUp = doormatRotation * Vector3.up;
                // Define offset values
                float forwardOffset = 1.0f;    // Z - forward from doormat
                float rightOffset = UnityEngine.Random.Range(-1.0f, 1.0f);     // X - to the side
                float upOffset = 0.00f;        // Y - same height
                float rotationOffset = UnityEngine.Random.Range(0, 360); // x - rotation
                doormatRotation = Quaternion.Euler(doormatRotation.eulerAngles + new Vector3(rotationOffset, 0, 0));

                // Calculate the offset position
                Vector3 offsetPosition = doormatPosition + 
                                        (doormatForward * forwardOffset) + 
                                        (doormatRight * rightOffset) + 
                                        (doormatUp * upOffset);
                
                Plugin.Log.LogInfo($"[SpawnItemLobby] Original doormat position: {doormatPosition}");
                Plugin.Log.LogInfo($"[SpawnItemLobby] Calculated offset position: {offsetPosition}");
                
                // Create a list of passed variables for the room ID, just like the game does
                Il2CppSystem.Collections.Generic.List<Interactable.Passed> passedVars = new Il2CppSystem.Collections.Generic.List<Interactable.Passed>();
                passedVars.Add(new Interactable.Passed(Interactable.PassedVarType.roomID, targetLocation.anchorNode.room.roomID, null));

                try {
                    // Spawn the item at the original doormat location first
                    Interactable spawnedItem = InteractableCreator.Instance.CreateFurnitureSpawnedInteractableThreadSafe(
                        itemPreset,
                        targetLocation.anchorNode.room,
                        targetLocation,
                        targetSubObject,
                        owner,
                        owner,
                        null,
                        passedVars,
                        null,
                        null,
                        ""
                    );
                    
                    if (spawnedItem != null)
                    {
                        // Now directly update the spawned item's position properties
                        Plugin.Log.LogInfo($"[SpawnItemLobby] Item spawned successfully. Original position: {spawnedItem.wPos}");
                        
                        // Update the item's position directly, similar to how RaiseLightswitch does it
                        Vector3 worldPosition = offsetPosition;
                        
                        // Calculate local position offset from the node's position
                        Vector3 localOffset = worldPosition - doormatPosition;
                        
                        // Update all position properties
                        spawnedItem.lPos = localOffset;  // Local position relative to anchor
                        spawnedItem.wPos = worldPosition; // World position
                        spawnedItem.spWPos = worldPosition; // Saved/serialized world position
                        spawnedItem.wEuler = doormatRotation.eulerAngles;
                        Plugin.Log.LogInfo($"[SpawnItemLobby] Item repositioned to offset. New position: {spawnedItem.wPos}");
                    }
                    else
                    {
                        Plugin.Log.LogError($"[SpawnItemLobby] Failed to spawn item at doormat location");
                    }
                    
                    return spawnedItem;
                } catch (Exception ex) {
                    Plugin.Log.LogError($"[SpawnItemLobby] Error during spawning with modified node: {ex.Message}");
                    return null; // Return null to indicate failure
                }
            }

            Plugin.Log.LogWarning($"[SpawnItemLobby] No doormats found near any entrances of {address.name} for item {itemNameForLog}.");
            return null;
        }
    }
}
