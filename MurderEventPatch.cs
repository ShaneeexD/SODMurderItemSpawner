using System;
using System.Collections.Generic;
using HarmonyLib;
using SOD.Common;
using UnityEngine;

namespace MurderItemSpawner
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

    [HarmonyPatch(typeof(MurderController), "PickNewVictim")]
    public class PickNewVictimPatch
    {
        // This method will be called when a new victim is picked
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
                ConfigManager.Instance.CheckRulesForEvent("PickNewVictim", murderType);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error in PickNewVictimPatch: {ex.Message}");
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


    [HarmonyPatch(typeof(MurderController), "TriggerCoverUpTelephoneCall")]
    public class TriggerCoverUpTelephoneCallPatch
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
                
                // Check if any rules should be triggered for this event
                ConfigManager.Instance.CheckRulesForEvent("TriggerCoverUpTelephoneCall", murderType);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error in TriggerCoverUpTelephoneCallPatch: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(MurderController), "OnCoverUpAccept")]
    public class OnCoverUpAcceptPatch
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
                
                // Check if any rules should be triggered for this event
                ConfigManager.Instance.CheckRulesForEvent("OnCoverUpAccept", murderType);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error in OnCoverUpAcceptPatch: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(MurderController), "OnCoverUpReject")]
    public class OnCoverUpRejectPatch
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
                
                // Check if any rules should be triggered for this event
                ConfigManager.Instance.CheckRulesForEvent("OnCoverUpReject", murderType);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error in OnCoverUpRejectPatch: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(MurderController), "TriggerKidnappingCase")]
    public class TriggerKidnappingCasePatch
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
                
                // Check if any rules should be triggered for this event
                ConfigManager.Instance.CheckRulesForEvent("TriggerKidnappingCase", murderType);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error in TriggerKidnappingCasePatch: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(MurderController), "TriggerRansomDelivery")]
    public class TriggerRansomDeliveryPatch
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
                
                // Check if any rules should be triggered for this event
                ConfigManager.Instance.CheckRulesForEvent("TriggerRansomDelivery", murderType);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error in TriggerRansomDeliveryPatch: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(MurderController), "KidnapperCollectsRansom")]
    public class KidnapperCollectsRansomPatch
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
                
                // Check if any rules should be triggered for this event
                ConfigManager.Instance.CheckRulesForEvent("KidnapperCollectsRansom", murderType);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error in KidnapperCollectsRansomPatch: {ex.Message}");
            }
        }
    }

   [HarmonyPatch(typeof(MurderController), "KidnapperCollectedRansom")]
    public class KidnapperCollectedRansomPatch
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
                
                // Check if any rules should be triggered for this event
                ConfigManager.Instance.CheckRulesForEvent("KidnapperCollectedRansom", murderType);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error in KidnapperCollectedRansomPatch: {ex.Message}");
            }
        }
    }

   [HarmonyPatch(typeof(MurderController), "TriggerRansomFail")]
    public class TriggerRansomFailPatch
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
                
                // Check if any rules should be triggered for this event
                ConfigManager.Instance.CheckRulesForEvent("TriggerRansomFail", murderType);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error in TriggerRansomFailPatch: {ex.Message}");
            }
        }
    }

   [HarmonyPatch(typeof(MurderController), "VictimFreed")]
    public class VictimFreedPatch
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
                
                // Check if any rules should be triggered for this event
                ConfigManager.Instance.CheckRulesForEvent("VictimFreed", murderType);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error in VictimFreedPatch: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(MurderController), "OnCaseSolved")]
    public class OnCaseSolvedPatch
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
                
                // Check if any rules should be triggered for this event
                ConfigManager.Instance.CheckRulesForEvent("OnCaseSolved", murderType);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error in OnCaseSolvedPatch: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(MurderController), "CitizenHasSeenBody", new Type[] { typeof(Human), typeof(Human) })]
    public class CitizenHasSeenBodyPatch
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
                
                // Check if any rules should be triggered for this event
                ConfigManager.Instance.CheckRulesForEvent("CitizenHasSeenBody", murderType);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error in CitizenHasSeenBodyPatch: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(MurderController), "CoverUpFailCheck", new Type[] { typeof(Human)})]
    public class CoverUpFailCheckPatch
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
                
                // Check if any rules should be triggered for this event
                ConfigManager.Instance.CheckRulesForEvent("CoverUpFailCheck", murderType);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error in CoverUpFailCheckPatch: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(MurderController), "TriggerSuccessfulCoverUp", new Type[] { typeof(Evidence)})]
    public class TriggerSuccessfulCoverUpPatch
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
                
                // Check if any rules should be triggered for this event
                ConfigManager.Instance.CheckRulesForEvent("TriggerSuccessfulCoverUp", murderType);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error in TriggerSuccessfulCoverUpPatch: {ex.Message}");
            }
        }
    }
}
