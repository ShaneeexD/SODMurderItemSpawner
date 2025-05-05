using System;
using System.Collections.Generic;
using HarmonyLib;
using SOD.Common;
using UnityEngine;

namespace MurderCult
{
    // Patch to detect murder events
    [HarmonyPatch(typeof(MurderController), "OnVictimKilled")]
    public class OnVictimKilledPatch
    {
        // This method will be called when a victim is killed
        public static void Postfix()
        {
            try
            {
                // Get the current murder type
                string murderType = "";
                if (MurderController.Instance != null && MurderController.Instance.chosenMO != null)
                {
                    murderType = MurderController.Instance.chosenMO.name;
                }

                Plugin.Log.LogInfo($"Murder detected! Type: {murderType}");
                
                // Check if any rules should be triggered for this event
                ConfigManager.Instance.CheckRulesForEvent("OnVictimKilled", murderType);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error in OnVictimKilledPatch: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(MurderController), "OnVictimDiscovery")]
    public class OnVictimDiscoveryPatch
    {
        // This method will be called when a victim is discovered
        public static void Postfix()
        {
            try
            {
                // Get the current murder type
                string murderType = "";
                if (MurderController.Instance != null && MurderController.Instance.chosenMO != null)
                {
                    murderType = MurderController.Instance.chosenMO.name;
                }

                Plugin.Log.LogInfo($"Murder detected! Type: {murderType}");
                
                // Check if any rules should be triggered for this event
                ConfigManager.Instance.CheckRulesForEvent("OnVictimDiscovery", murderType);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error in OnVictimDiscoveryPatch: {ex.Message}");
            }
        }
    }
}
