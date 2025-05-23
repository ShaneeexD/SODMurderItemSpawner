using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SOD.Common;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using System.IO;
using SOD.Common.Extensions;
using BepInEx;
using HarmonyLib;

namespace MurderItemSpawner
{
    public class SpawnItemMailbox : MonoBehaviour
    {
        // Singleton instance for coroutines
        private static SpawnItemMailbox _instance;
        private static SpawnItemMailbox Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Create a new GameObject to host our MonoBehaviour
                    GameObject go = new GameObject("SpawnItemMailbox");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<SpawnItemMailbox>();
                }
                return _instance;
            }
        }

        public static void SpawnItemAtLocation(
            Human owner,               // The owner of the item (from BelongsTo)
            Human recipient,           // The recipient used for spawn location (from ItemRecipient)
            Interactable targetLocation,
            string presetName,
            bool unlockMailbox,
            float spawnChance,
            bool useMultipleOwners = false,
            List<BelongsTo> owners = null)
        {
            try
            {
                float randomValue = UnityEngine.Random.Range(0f, 1f);
                if (randomValue > spawnChance)
                {
                    Plugin.LogDebug($"Cannot spawn item: Spawn chance not met (Random value: {randomValue}, Required chance: {spawnChance})");
                    return;
                }
                
                Plugin.LogDebug($"Spawn chance check passed (Random value: {randomValue}, Required chance: {spawnChance})");
                
                // Validate parameters
                if (owner == null)
                {
                    Plugin.LogDebug("Cannot spawn item: No owner specified");
                    return;
                }

                // If target location is a mailbox and unlockMailbox is true, unlock it
                if (targetLocation != null && targetLocation.preset != null && 
                    targetLocation.preset.presetName.Contains("Mailbox") && unlockMailbox)
                {
                    targetLocation.SetLockedState(false, null, false, true);
                }

                // Get the interactable preset
                InteractablePreset interactablePresetItem = Toolbox.Instance.GetInteractablePreset(presetName);

                if (interactablePresetItem == null)
                {
                    Plugin.LogDebug("Cannot spawn item: No valid preset found");
                    return;
                }
                


                // Determine spawn position
                Vector3 spawnPosition;
                if (targetLocation != null)
                {
                    // Use the target location's position
                    spawnPosition = targetLocation.wPos;
                }
                else
                {
                    // If no target location, spawn near the owner
                    spawnPosition = recipient.transform.position + Vector3.up * 0.5f;
                }

                // Get the mailbox for placement
                Interactable targetMailbox = Toolbox.Instance.GetMailbox(recipient);
                // Create the item
                Interactable spawnedItem = null;
                try
                {
                    // Check if we have a valid mailbox with a game location
                    if (targetMailbox != null && targetMailbox.node != null && targetMailbox.node.gameLocation != null)
                    {
                        // Get the mailbox's game location
                        NewGameLocation mailboxLocation = targetMailbox.node.gameLocation;
                        
                        Plugin.LogDebug($"Creating item in mailbox location: {mailboxLocation.name}");
                        
                        // Add detailed debugging before attempting to place the object
                        Plugin.LogDebug($"Mailbox details: Type={targetMailbox.GetType().Name}");
                        Plugin.LogDebug($"Mailbox position: {targetMailbox.wPos}, rotation: {targetMailbox.wEuler}");
                        Plugin.LogDebug($"Mailbox node: {(targetMailbox.node != null ? targetMailbox.node.ToString() : "null")}");
                        Plugin.LogDebug($"Mailbox game location: {mailboxLocation.name}");
                        Plugin.LogDebug($"Item preset: {interactablePresetItem.name}");
                        Plugin.LogDebug($"Owner: {(owner != null ? owner.name : "null")}");
                        Plugin.LogDebug($"Recipient: {(recipient != null ? recipient.name : "null")}");

                        // Check if the mailbox is locked and try to unlock it
                        Plugin.LogDebug($"Mailbox locked state: {targetMailbox.locked}");
                        if (targetMailbox.locked)
                        {
                            Plugin.LogDebug("Attempting to unlock mailbox...");
                            targetMailbox.SetLockedState(false, null, false, true);
                            Plugin.LogDebug($"Mailbox locked state after unlock attempt: {targetMailbox.locked}");
                        }

                        // Log detailed debug info about the mailbox rotation
                        Plugin.LogDebug("=== Mailbox Rotation Info ====");
                        Plugin.LogDebug($"Mailbox rotation (wEuler): {targetMailbox.wEuler}");
                        
                        // Get rotation as quaternion and extract forward/right/up vectors
                        Quaternion mailboxRotation = Quaternion.Euler(targetMailbox.wEuler);
                        Vector3 mailboxForward = mailboxRotation * Vector3.forward;
                        Vector3 mailboxRight = mailboxRotation * Vector3.right;
                        Vector3 mailboxUp = mailboxRotation * Vector3.up;
                        
                        Plugin.LogDebug($"Mailbox forward: {mailboxForward}, right: {mailboxRight}, up: {mailboxUp}");
                        
                        // Calculate position with offset based on mailbox rotation
                        Vector3 spawnPos = targetMailbox.wPos;
                        
                        // Define local space offsets
                        float forwardOffset = -0.15f;   // Z - depth inside mailbox
                        float rightOffset = 0.1f;      // X - centered
                        float upOffset = 0.00f;        // Y - slightly above bottom
                        
                        // Apply offsets in local space
                        spawnPos += mailboxForward * forwardOffset;
                        spawnPos += mailboxRight * rightOffset;
                        spawnPos += mailboxUp * upOffset;
                        
                        Plugin.LogDebug($"Base position: {targetMailbox.wPos}, Offset position: {spawnPos}");
                        
                        // Create the item using CreateWorldInteractable
                        Plugin.LogDebug("Creating item using CreateWorldInteractable with rotation-based offset...");
                        spawnedItem = InteractableCreator.Instance.CreateWorldInteractable(
                            interactablePresetItem,  // The item preset
                            owner,                   // The owner of the item
                            owner,                   // The writer (same as owner in this case)
                            recipient,         // The receiver (the player)
                            spawnPos,                // The position with rotation-based offset
                            targetMailbox.wEuler,    // The rotation (mailbox rotation)
                            null,                    // No passed variables
                            null,                    // No passed object
                            ""                       // No load GUID
                        );
                        
                        if (spawnedItem != null)
                        {
                            // Set the item's node to the mailbox's node
                           // spawnedItem.node = targetMailbox.node;
                            
                            // Update the item's position and node
                        //    spawnedItem.UpdateWorldPositionAndNode(true, true);
                            
                            // Handle multiple owners if enabled
                            if (useMultipleOwners && owners != null && owners.Count > 0)
                            {
                                // Add additional fingerprints for each owner in the list
                                foreach (BelongsTo ownerType in owners)
                                {
                                    // Get the Human object for this owner type
                                    Human additionalOwner = ConfigManager.Instance.GetOwnerForFingerprint(ownerType);
                                    
                                    if (additionalOwner != null)
                                    {
                                        Plugin.LogDebug($"[SpawnItemMailbox] Adding fingerprint for {ownerType}");
                                        // Add the fingerprint with default life parameter
                                        spawnedItem.AddNewDynamicFingerprint(additionalOwner, Interactable.PrintLife.timed);
                                    }
                                    else
                                    {
                                        Plugin.LogDebug($"[SpawnItemMailbox] Could not add fingerprint for {ownerType} - Human not found");
                                    }
                                }
                                
                                Plugin.LogDebug($"[SpawnItemMailbox] Successfully added multiple owners to '{presetName}'");
                            }
                            
                            Plugin.LogDebug($"Successfully created item using CreateWorldInteractable");
                            Plugin.LogDebug($"Item position: {spawnedItem.wPos}, node: {(spawnedItem.node != null ? spawnedItem.node.ToString() : "null")}");
                        }
                        else
                        {
                            Plugin.Log.LogError("Failed to create item using CreateWorldInteractable");
                        }
                    }
                }
                catch (Exception placeEx)
                {
                    Plugin.Log.LogError($"Exception in PlaceObject: {placeEx.Message}");
                    Plugin.Log.LogError($"Stack trace: {placeEx.StackTrace}");
                }              
            }catch(Exception ex)
            {
                Plugin.Log.LogError($"Exception in SpawnItemAtLocation: {ex.Message}");
                Plugin.Log.LogError($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
   
