using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using static WearableItemsAPI.Plugin;

namespace WearableItemsAPI
{
    public static class PlayerControllerBExtensions
    {
        public static bool GrabGrabbableObject(this PlayerControllerB player, GrabbableObject grabbableObject)
        {
            if (player.twoHanded || player.sinkingValue > 0.73f) { return false; }

            player.currentlyGrabbingObject = grabbableObject;

            if (!GameNetworkManager.Instance.gameHasStarted && !player.currentlyGrabbingObject.itemProperties.canBeGrabbedBeforeGameStart && StartOfRound.Instance.testRoom == null) { return false; }

            player.grabInvalidated = false;

            if (player.currentlyGrabbingObject == null || player.inSpecialInteractAnimation || player.currentlyGrabbingObject.isHeld || player.currentlyGrabbingObject.isPocketed) { return false; }

            NetworkObject networkObject = player.currentlyGrabbingObject.NetworkObject;
            if (networkObject == null || !networkObject.IsSpawned) { return false; }

            player.currentlyGrabbingObject.InteractItem();

            if (!player.currentlyGrabbingObject.grabbable || player.FirstEmptyItemSlot(player.currentlyGrabbingObject) == -1) { return false; }

            player.playerBodyAnimator.SetBool("GrabInvalidated", value: false);
            player.playerBodyAnimator.SetBool("GrabValidated", value: false);
            player.playerBodyAnimator.SetBool("cancelHolding", value: false);
            player.playerBodyAnimator.ResetTrigger("Throw");
            player.SetSpecialGrabAnimationBool(setTrue: true);
            player.isGrabbingObjectAnimation = true;
            player.cursorIcon.enabled = false;
            player.cursorTip.text = "";
            player.twoHanded = player.currentlyGrabbingObject.itemProperties.twoHanded;
            player.carryWeight = Mathf.Clamp(player.carryWeight + (player.currentlyGrabbingObject.itemProperties.weight - 1f), 1f, 10f);
            StartOfRound.Instance.SendChangedWeightEvent();
            if (player.currentlyGrabbingObject.itemProperties.grabAnimationTime > 0f)
            {
                player.grabObjectAnimationTime = player.currentlyGrabbingObject.itemProperties.grabAnimationTime;
            }
            else
            {
                player.grabObjectAnimationTime = 0.4f;
            }
            if (!player.isTestingPlayer)
            {
                player.GrabObjectServerRpc(networkObject);
            }
            if (player.grabObjectCoroutine != null)
            {
                player.StopCoroutine(player.grabObjectCoroutine);
            }
            player.grabObjectCoroutine = player.StartCoroutine(player.GrabObject());

            return true;
        }
    }
}
