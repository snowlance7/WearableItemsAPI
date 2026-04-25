using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using static WearableItemsAPI.Plugin;

namespace WearableItemsAPI
{
    [HarmonyPatch]
    internal class Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.OpenQuickMenu))]
        private static bool QuickMenuManager_OpenQuickMenuPrefix()
        {
            try
            {
                if (WearableUIController.Instance != null && WearableUIController.Instance.ui != null) { return false; }
                return true;
            }
            catch (System.Exception e)
            {
                logger.LogError(e);
                return true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.CanUseItem))]
        private static bool PlayerControllerB_CanUseItemPrefix()
        {
            try
            {
                if (WearableUIController.Instance != null && WearableUIController.Instance.ui != null) { return false; }
                return true;
            }
            catch (System.Exception e)
            {
                logger.LogError(e);
                return true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
        private static void PlayerControllerB_ConnectClientToPlayerObjectPostfix()
        {
            try
            {
                WearableUIController.Init();
            }
            catch (System.Exception e)
            {
                logger.LogError(e);
                return;
            }
        }
    }
}