using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace WearableItemsAPI
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(LethalCompanyInputUtils.MyPluginInfo.PLUGIN_GUID)]
    internal class Plugin : BaseUnityPlugin
    {
        internal static Plugin Instance = null!;

        internal static ManualLogSource logger = null!;

        private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        public static PlayerControllerB localPlayer { get { return StartOfRound.Instance.localPlayerController; } }

        public static ConfigEntry<bool> configCloseUIOnUnwear = null!;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            logger = Instance.Logger;

            harmony.PatchAll();

            InitializeNetworkBehaviours();

            configCloseUIOnUnwear = Config.Bind("General", "Close UI On Unwear", true, "If true, the UI will always close when unequiping a wearable");

            var assets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), "wearable_items_assets"));
            WearableUIController.prefab = assets.LoadAsset<GameObject>("Assets/ModAssets/WearableItemsUI.prefab");

            // Finished
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        private static void InitializeNetworkBehaviours()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
            logger.LogDebug("Finished initializing network behaviours");
        }
    }
}