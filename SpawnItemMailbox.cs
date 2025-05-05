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

namespace MurderCult
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

        // Timer functionality for delayed spawns
        public static void StartTimer(float seconds, Action callback)
        {
            Instance.StartCoroutine(Instance.TimerCoroutine(seconds, callback));
        }

        private IEnumerator TimerCoroutine(float seconds, Action callback)
        {
            yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }

        // Original method - kept for backward compatibility
        public static void SpawnItemInMailbox(string presetName)
        {
            // Get the current murderer
            Human currentMurderer = MurderController.Instance.currentMurderer;
            if (currentMurderer == null)
            {
                Plugin.Log.LogInfo("Cannot spawn item: No active murder or murderer");
                return;
            }

            // Get the mailbox
            Interactable mailbox = Toolbox.Instance.GetMailbox(currentMurderer);
            if (mailbox == null)
            {
                Plugin.Log.LogInfo("Cannot spawn item: Murderer has no mailbox");
                return;
            }

            // Use the new method with default values
            SpawnItemAtLocation(
                currentMurderer,
                mailbox,
                presetName,
                new Vector3(0.2f, 0.0f, 0.12f),
                true
            );
        }

        // New modular method for spawning items at any location
        public static void SpawnItemAtLocation(
            Human owner,
            Interactable targetLocation,
            string presetName,
            Vector3 positionOffset,
            bool showPositionMessageDebug = true,
            bool unlockMailbox = true)
        {
            try
            {
                // Validate parameters
                if (owner == null)
                {
                    Plugin.Log.LogInfo("Cannot spawn item: No owner specified");
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
                    // Try fallback items if the specified one doesn't exist
                    string[] fallbackItems = { "Pencil", "Note", "Knife" };
                    foreach (string fallbackItem in fallbackItems)
                    {
                        interactablePresetItem = Toolbox.Instance.GetInteractablePreset(fallbackItem);
                        if (interactablePresetItem != null)
                        {
                            Plugin.Log.LogInfo($"Using fallback item: {fallbackItem}");
                            break;
                        }
                    }

                    if (interactablePresetItem == null)
                    {
                        Plugin.Log.LogInfo("Cannot spawn item: No valid preset found");
                        return;
                    }
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
                    spawnPosition = owner.transform.position + Vector3.up * 0.5f;
                }

                // Create the item
                Interactable spawnedItem = null;
                try
                {
                    // Spawn temporarily high above the player to avoid collision issues
                    Vector3 tempSpawnPos = Player.Instance.transform.position + Vector3.up * 50f;
                    
                    spawnedItem = InteractableCreator.Instance.CreateWorldInteractable(
                        interactablePresetItem,
                        Player.Instance,
                        null,
                        null,
                        tempSpawnPos,
                        Vector3.zero,
                        null,
                        null,
                        ""
                    );
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogInfo("Error creating item: " + ex.Message);
                    return;
                }

                if (spawnedItem != null)
                {
                    Plugin.Log.LogInfo("Successfully spawned item");

                    // Set the owner
                    spawnedItem.SetOwner(owner, true);

                    // Try to place the item at the target location
                    try
                    {
                        // Apply the position offset
                        Vector3 finalPosition = spawnPosition + positionOffset;

                        // Apply the position to the item
                        spawnedItem.wPos = finalPosition;
                        spawnedItem.UpdateWorldPositionAndNode(true, false);

                        Plugin.Log.LogInfo("Moved item to position: " + spawnedItem.wPos.ToString());
                        
                        if (showPositionMessageDebug)
                        {
                            Lib.GameMessage.ShowPlayerSpeech("Item at: " + spawnedItem.wPos.ToString(), 2, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.LogInfo("Error placing item: " + ex.Message);
                    }
                }
                else
                {
                    Plugin.Log.LogInfo("Failed to spawn item");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogInfo("Error in SpawnItemAtLocation: " + ex.Message);
            }
        }
    }
}