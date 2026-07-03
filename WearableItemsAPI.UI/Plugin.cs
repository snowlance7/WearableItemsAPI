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
        public static Plugin Instance = null!;

        public static ManualLogSource logger = null!;

        private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        public static PlayerControllerB localPlayer { get { return StartOfRound.Instance.localPlayerController; } }

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            logger = Instance.Logger;

            harmony.PatchAll();

            var assets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), "wearable_items_assets"));
            WearableUIController.prefab = assets.LoadAsset<GameObject>("Assets/ModAssets/WearableItemsUI.prefab");

            // Finished
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }
    }
}