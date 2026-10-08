using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System.IO;
using UnityEngine;

namespace WearableItemsAPI.UI
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(LethalCompanyInputUtils.MyPluginInfo.PLUGIN_GUID)]
    internal class Plugin : BaseUnityPlugin
    {
        public static Plugin PluginInstance = null!;

        public static ManualLogSource logger => PluginInstance.Logger;

        private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        public static PlayerControllerB localPlayer => StartOfRound.Instance.localPlayerController;

        public void Awake()
        {
            if (PluginInstance == null)
            {
                PluginInstance = this;
            }

            harmony.PatchAll();

            var assets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), "wearable_items_assets"));
            WearableUIController.prefab = assets.LoadAsset<GameObject>("Assets/ModAssets/WearableItemsUI.prefab");

            // Finished
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }
    }
}