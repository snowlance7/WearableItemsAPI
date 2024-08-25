using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace WearableItemsAPI
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(LethalCompanyInputUtils.PluginInfo.PLUGIN_GUID)]
    internal class Plugin : BaseUnityPlugin
    {
        public const string modGUID = "Snowlance.WearableItemsAPI";
        public const string modName = "WearableItemsAPI";
        public const string modVersion = "0.1.0";

        internal static Plugin PluginInstance;
        internal static ManualLogSource LoggerInstance;
        private readonly Harmony harmony = new Harmony(modGUID);
        internal static PlayerControllerB localPlayer { get { return StartOfRound.Instance.localPlayerController; } }

        public static AssetBundle? ModAssets;

        public static ConfigEntry<int> configUIPositionX;
        public static ConfigEntry<int> configUIPositionY;
        public static ConfigEntry<int> configUIWidth;
        public static ConfigEntry<int> configUIHeight;
        public static ConfigEntry<bool> configArmSpacing;
        public static ConfigEntry<int> configSpaceBetweenArms;
        public static ConfigEntry<int> configSpaceBetweenHeadAndChest;
        public static ConfigEntry<int> configSpaceBetweenChestAndLegs;
        public static ConfigEntry<int> configSpaceBetweenLegsAndFeet;
        

        private void Awake()
        {
            if (PluginInstance == null)
            {
                PluginInstance = this;
            }

            LoggerInstance = PluginInstance.Logger;

            harmony.PatchAll();

            InitializeNetworkBehaviours();

            WearableItemsInputs.Init();

            // Configs
            configUIPositionX = Config.Bind("UI Settings", "UIPositionX", 0, "X Position of the UI");
            configUIPositionY = Config.Bind("UI Settings", "UIPositionY", 100, "Y Position of the UI");
            configUIWidth = Config.Bind("UI Settings", "UIWidth", 55, "Width of the UI");
            configUIHeight = Config.Bind("UI Settings", "UIHeight", 55, "Height of the UI");
            configArmSpacing = Config.Bind("UI Settings", "ArmSpacing", false, "Whether to add a gap between arm buttons");
            configSpaceBetweenArms = Config.Bind("UI Settings", "SpaceBetweenArms", 2, "Extra spacing between arm buttons");
            configSpaceBetweenHeadAndChest = Config.Bind("UI Settings", "SpaceBetweenHeadAndChest", 0, "Space between head and chest");
            configSpaceBetweenChestAndLegs = Config.Bind("UI Settings", "SpaceBetweenChestAndLegs", 0, "Space between chest and legs");
            configSpaceBetweenLegsAndFeet = Config.Bind("UI Settings", "SpaceBetweenLegsAndFeet", 0, "Space between legs and feet");


            // Loading Assets
            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            ModAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), "wearable_items_assets"));
            if (ModAssets == null)
            {
                Logger.LogError($"Failed to load custom assets.");
                return;
            }
            LoggerInstance.LogDebug($"Got AssetBundle at: {Path.Combine(sAssemblyLocation, "wearable_items_assets")}");

            // Finished
            Logger.LogInfo($"{modGUID} v{modVersion} has loaded!");
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
            LoggerInstance.LogDebug("Finished initializing network behaviours");
        }
    }
}