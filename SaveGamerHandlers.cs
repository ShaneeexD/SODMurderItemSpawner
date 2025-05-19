using SOD.Common;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;
using HarmonyLib;

namespace MurderItemSpawner
{
    public class SaveGamerHandlers : MonoBehaviour
    {
        public SaveGamerHandlers()
        {       
            Lib.SaveGame.OnAfterLoad += HandleGameLoaded;
            Lib.SaveGame.OnAfterNewGame += HandleNewGameStarted;
            Lib.SaveGame.OnBeforeNewGame += HandleGameBeforeNewGame;
            Lib.SaveGame.OnBeforeLoad += HandleGameBeforeLoad;
            Lib.SaveGame.OnBeforeDelete += HandleGameBeforeDelete;
            Lib.SaveGame.OnAfterDelete += HandleGameAfterDelete;
            Lib.SaveGame.OnAfterSave += HandleGameSaved;
        }

        private void HandleNewGameStarted(object sender, EventArgs e)
        {
            try
            {
                Plugin.Log.LogInfo("New game started, resetting configuration");
                
                // Reset any triggered rules
                ConfigManager.Instance.ResetTriggeredRules();
                
                // Reset tracking for a new game
                ConfigManager.Instance.ResetTracking();
                
                // Reload the configuration in case it was changed
                ConfigManager.Instance.LoadConfig();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error in HandleNewGameStarted: {ex.Message}");
            }     
        }

        private void HandleGameLoaded(object sender, EventArgs e)
        {
            try
            {
                // Reload the configuration in case it was changed
                ConfigManager.Instance.LoadConfig();
                
                // Load tracking data
                ConfigManager.Instance.LoadTrackingData();
                
                Plugin.Log.LogInfo("Game loaded, tracking data loaded");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error in HandleGameLoaded: {ex.Message}");
            }
        }

        private void HandleGameBeforeNewGame(object sender, EventArgs e)
        {

        }

        private void HandleGameBeforeLoad(object sender, EventArgs e)
        {

        }

        private void HandleGameBeforeDelete(object sender, EventArgs e)
        {

        }

        private void HandleGameAfterDelete(object sender, EventArgs e)
        {

        }
        
        private void HandleGameSaved(object sender, EventArgs e)
        {
            try
            {
                // Save tracking data when the game is saved
                ConfigManager.Instance.SaveTrackingData();
                Plugin.Log.LogInfo("Game saved, tracking data saved");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error in HandleGameSaved: {ex.Message}");
            }
        }

        private void HandleConfig(object sender, EventArgs e)
        {
            try
            {
                Plugin.Log.LogInfo("New game started, resetting configuration");
                
                // Reset any triggered rules
                ConfigManager.Instance.ResetTriggeredRules();
                
                // Reload the configuration in case it was changed
                ConfigManager.Instance.LoadConfig();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error in GameStartPatch: {ex.Message}");
            }
        }
    }
}