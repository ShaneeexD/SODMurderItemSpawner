using BepInEx;
using SOD.Common.BepInEx;
using System.Reflection;
using BepInEx.Configuration;
namespace MurderItemSpawner
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(SOD.Common.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginController<Plugin>
    {
        public const string PLUGIN_GUID = "ShaneeexD.MurderItemSpawner";
        public const string PLUGIN_NAME = "MurderItemSpawner";
        public const string PLUGIN_VERSION = "1.0.0";
        public static ConfigEntry<bool> showDebugLogs;

        public override void Load()
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            SaveGamerHandlers eventHandler = new SaveGamerHandlers();
            Log.LogInfo("MurderItemSpawner plugin is patched.");

            showDebugLogs = Config.Bind("General", "ShowDebugLogs", false, new ConfigDescription("Show debug logs."));

        }
        
        public static void LogDebug(string message)
        {
            if (showDebugLogs != null && showDebugLogs.Value)
            {
                Log.LogInfo(message);
            }
        }
    }
}