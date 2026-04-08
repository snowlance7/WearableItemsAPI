using HarmonyLib;
using System.Linq;
using UnityEngine;
using static WearableItemsAPI.Plugin;

/* bodyparts
 * 0 head
 * 1 right arm
 * 2 left arm
 * 3 right leg
 * 4 left leg
 * 5 chest
 * 6 feet
 * 7 right hip
 * 8 crotch
 * 9 left shoulder
 * 10 right shoulder */

namespace WearableItemsAPI
{
    [HarmonyPatch]
    public class TESTING : MonoBehaviour
    {
        public static bool localPlayerImmune = false;
        public static string currentAnim = "";

        [HarmonyPostfix, HarmonyPatch(typeof(HUDManager), nameof(HUDManager.PingScan_performed))]
        public static void PingScan_performedPostFix()
        {
            if (!Utils.isBeta) { return; }
            if (!Utils.testing) { return; }

            logger.LogDebug("PingScanTestPerformed");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(HUDManager), nameof(HUDManager.SubmitChat_performed))]
        public static void SubmitChat_performedPrefix(HUDManager __instance)
        {
            if (!Utils.isBeta) { return; }
            if (!IsServerOrHost) { return; }
            string msg = __instance.chatTextField.text;
            string[] args = msg.Split(" ");
            Plugin.logger.LogDebug(msg);

            switch (args[0])
            {
                default:
                    Utils.ChatCommand(args);
                    break;
            }
        }
    }
}