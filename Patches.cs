using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;

namespace WearableItemsAPI
{
    [HarmonyPatch]
    internal class Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.OpenQuickMenu))]
        private static bool OpenQuickMenuPatch()
        {
            if (WearableUIController.Instance == null) { return true; }
            if (WearableUIController.Instance.ui != null) { return false; }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
        private static void ConnectClientToPlayerObjectPostfix()
        {
            WearableUIController.Init();
        }
    }
}