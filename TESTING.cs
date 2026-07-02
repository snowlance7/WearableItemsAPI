using HarmonyLib;
using UnityEngine;
using static WearableItemsAPI.Plugin;

namespace WearableItemsAPI
{
    [HarmonyPatch]
    internal class TESTING : MonoBehaviour
    {
        [HarmonyPostfix, HarmonyPatch(typeof(HUDManager), nameof(HUDManager.PingScan_performed))]
        public static void PingScan_performedPostFix()
        {
            try
            {

            }
            catch (System.Exception e)
            {
                logger.LogError(e);
                return;
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(HUDManager), nameof(HUDManager.SubmitChat_performed))]
        public static void SubmitChat_performedPrefix(HUDManager __instance)
        {
            try
            {
                string msg = __instance.chatTextField.text;
                string[] args = msg.Split(" ");

                switch (args[0])
                {
                    case "/wi_transforms":
                        foreach (Transform t in localPlayer.playerBodyAnimator.gameObject.transform.Find("spine").GetComponentsInChildren<Transform>(includeInactive: true))
                        {
                            logger.LogDebug(t.name);
                        }
                        break;
                    case "/wi_transformslocal":
                        foreach (Transform t in localPlayer.playerBodyAnimator.gameObject.transform.Find("ScavengerModelArmsOnly").GetComponentsInChildren<Transform>(includeInactive: true))
                        {
                            logger.LogDebug(t.name);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (System.Exception e)
            {
                logger.LogError(e);
                return;
            }
        }
    }
}