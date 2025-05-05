using SOD.Common;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;
using HarmonyLib;

namespace MurderCult
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
        }

        private void HandleNewGameStarted(object sender, EventArgs e)
        {
            HandleConfig(sender, e);
        }

        private void HandleGameLoaded(object sender, EventArgs e)
        {
            HandleConfig(sender, e);
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