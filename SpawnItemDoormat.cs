using System;
using System.Collections.Generic;
using SOD.Common;
using UnityEngine;

namespace MurderItemSpawner
{
    public class SpawnItemDoormat : MonoBehaviour
    {
        private static SpawnItemDoormat _instance;
        private static SpawnItemDoormat Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SpawnItemDoormat_Instance");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<SpawnItemDoormat>();
                }
                return _instance;
            }
        }

        // Method to spawn an item at a location in the lobby
        public static void SpawnItemAtLocation(Human owner, Human recipient, string presetName, float spawnChance = 1.0f,
            bool useMultipleOwners = false, List<BelongsTo> owners = null)
        {
            try
            {
                // Check if we should spawn based on chance
                float randomValue = UnityEngine.Random.Range(0f, 1f);
                if (randomValue > spawnChance)
                {
                    Plugin.LogDebug($"[SpawnItemDoormat] Skipping spawn of {presetName} due to chance (roll: {randomValue}, needed: <= {spawnChance})");
                    return;
                }

                // Get the interactable preset
                InteractablePreset interactablePresetItem = Toolbox.Instance.GetInteractablePreset(presetName);
                if (interactablePresetItem == null)
                {
                    Plugin.Log.LogError($"[SpawnItemDoormat] Could not find interactable preset with name {presetName}");
                    return;
                }

                // Get the recipient's address (where to spawn the item)
                if (recipient == null || recipient.home == null)
                {
                    Plugin.Log.LogWarning($"[SpawnItemDoormat] Recipient has no valid address. Cannot spawn {presetName}");
                    return;
                }

                NewAddress recipientAddress = recipient.home;
                Plugin.LogDebug($"[SpawnItemDoormat] Owner: {owner.name}, Recipient: {recipient.name}, Address: {recipientAddress.name}");

                // Spawn the item using the same approach as the game's SpawnSpareKey method
                Interactable spawnedItem = SpawnItemOnDoormat(recipientAddress, interactablePresetItem, owner, presetName);
                
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
                            Plugin.LogDebug($"[SpawnItemDoormat] Adding fingerprint for {ownerType}");
                            // Add the fingerprint with default life parameter
                            spawnedItem.AddNewDynamicFingerprint(additionalOwner, Interactable.PrintLife.timed);
                        }
                        else
                        {
                            Plugin.LogDebug($"[SpawnItemDoormat] Could not add fingerprint for {ownerType} - Human not found");
                        }
                    }
                    
                    Plugin.LogDebug($"[SpawnItemDoormat] Successfully added multiple owners to '{presetName}'");
                }
                else
                {
                    // Standard single owner
                    spawnedItem.SetOwner(owner);
                }

                if (spawnedItem != null)
                {
                    Plugin.LogDebug($"[SpawnItemDoormat] Successfully spawned '{presetName}' on doormat in lobby. Item node: {(spawnedItem.node != null ? spawnedItem.node.ToString() : "null")}");
                }
                else
                {
                    Plugin.Log.LogError($"[SpawnItemDoormat] Failed to create furniture spawned interactable '{presetName}'.");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[SpawnItemDoormat] Error spawning item {presetName}: {ex.Message}");
            }
        }

        // Method to spawn an item on a doormat, following the game's SpawnSpareKey approach exactly
        private static Interactable SpawnItemOnDoormat(NewAddress address, InteractablePreset itemPreset, Human owner, string itemNameForLog)
        {
            if (address == null)
            {
                Plugin.Log.LogWarning($"[SpawnItemDoormat] Address is null for {itemNameForLog}.");
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

                // Create a list of passed variables for the room ID, just like the game does
                Il2CppSystem.Collections.Generic.List<Interactable.Passed> passedVars = new Il2CppSystem.Collections.Generic.List<Interactable.Passed>();
                passedVars.Add(new Interactable.Passed(Interactable.PassedVarType.roomID, targetRoom.roomID, null));

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

                return spawnedItem;
            }

            Plugin.Log.LogWarning($"[SpawnItemDoormat] No doormats found near any entrances of {address.name} for item {itemNameForLog}.");
            return null;
        }
    }
}
