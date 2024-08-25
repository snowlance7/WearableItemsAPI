using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.UIElements;

namespace WearableItemsAPI
{
    [HarmonyPatch]
    internal class Patches
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.OpenQuickMenu))]
        private static bool OpenQuickMenuPatch()
        {
            if (WearableUIController.Instance == null) { return true; }
            if (WearableUIController.Instance.veMain == null) { logger.LogError("veMain is null!"); return true; }
            if (WearableUIController.Instance.veMain.style.display == DisplayStyle.Flex) { return false; }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
        private static void AwakePostfix()
        {
            WearableUIController.Init();
        }
    }
}