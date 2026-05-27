using GameNetcodeStuff;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static WearableItemsAPI.Plugin;
using static WearableItemsAPI.WearableItem;
//using SnowyLib;

namespace WearableItemsAPI
{
    public abstract class WearableObject : PhysicsProp
    {
        public PlayerControllerB? playerWornBy { get; private set; }
        public PlayerControllerB? lastPlayerWornBy { get; private set; }

        public WearableItem wearableItemProperties = null!;

        ScanNodeProperties? scanNode;

        public override void Start()
        {
            base.Start();
            scanNode = base.gameObject.GetComponentInChildren<ScanNodeProperties>();
        }

        public override void Update()
        {
            base.Update();
            if (playerWornBy != null)
            {
                lastPlayerWornBy = playerWornBy;

                if (!playerWornBy.isPlayerControlled && IsServer)
                {
                    UnwearServerRpc();
                    return;
                }
            }
        }

        public override void LateUpdate()
        {
            if (parentObject != null && playerWornBy != null)
            {
                base.transform.rotation = parentObject.rotation;
                base.transform.Rotate(wearableItemProperties.wornRotationOffset);
                base.transform.position = parentObject.position;
                Vector3 positionOffset = wearableItemProperties.wornPositionOffset;
                positionOffset = parentObject.rotation * positionOffset;
                base.transform.position += positionOffset;

                if (radarIcon != null)
                {
                    radarIcon.position = base.transform.position;
                }
            }
            else
            {
                base.LateUpdate();
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown && wearableItemProperties.wearOnUse)
                WearItem(playerHeldBy);
        }

        public void WearItem(PlayerControllerB player)
        {
            if (playerWornBy != null || !CanWear()) { return; }
            WearServerRpc(player.actualClientId);
        }

        public void UnWearItem()
        {
            if (playerWornBy == null) { return; }
            UnwearServerRpc();
        }

        bool CanWear()
        {
            var current = wearableItemProperties;

            foreach (var item in localPlayer.GetWornItems())
            {
                var other = item.wearableItemProperties;

                if (((other.restrictSlot || current.restrictSlot) && other.slot == current.slot) || (current.restrictions.Any(r => other.restrictions.Contains(r))))
                {
                    HUDManager.Instance.DisplayTip("Can't wear item", $"'{item.itemProperties.itemName}' is preventing you from wearing this item", true);
                    return false;
                }
            }

            return true;
        }

        public virtual void OnWear() { }

        public virtual void OnUnWear() { }

        // RPCs
        [ServerRpc(RequireOwnership = false)]
        internal void WearServerRpc(ulong clientId)
        {
            if (!IsServer) { return; }
            WearClientRpc(clientId);
        }

        [ClientRpc]
        internal void WearClientRpc(ulong clientId)
        {
            if (playerWornBy != null) { logger.LogDebug("Player already wearing item"); return; }
            PlayerControllerB? player = StartOfRound.Instance.allPlayerScripts.Where(x => x.actualClientId == clientId).FirstOrDefault();
            if (player == null) { logger.LogError("Couldn't get player from player client id"); return; }

            logger.LogDebug(player.playerUsername + " wearing " + itemProperties.itemName);

            playerWornBy = player;
            playerWornBy.DiscardHeldObject(false, playerWornBy.NetworkObject);

            parentObject = wearableItemProperties.slot == WearableSlot.None ? playerWornBy.transform : playerWornBy.bodyParts[(int)wearableItemProperties.slot];
            base.gameObject.GetComponent<Collider>().enabled = false;
            bool _showWearable = localPlayer == playerWornBy ? wearableItemProperties.showWearableOnClient : wearableItemProperties.showWearable;
            EnableItemMeshes(_showWearable);
            scanNode?.gameObject.SetActive(false);

            player.AddWearable(this);
            OnWear();

            HUDManager.Instance.DisplayTip("WearableItemsAPI", $"Press {WearableItemsInputs.OpenUIKeybind} to open the Wearable Items UI", false, true, "WearableItemsAPITip1"); // TODO: Test this
        }

        [ServerRpc(RequireOwnership = false)]
        internal void UnwearServerRpc()
        {
            if (!IsServer) { return; }
            UnwearClientRpc();
        }

        [ClientRpc]
        internal void UnwearClientRpc()
        {
            if (playerWornBy == null) return;
            PlayerControllerB playerUnwearing = playerWornBy;

            parentObject = null;

            Transform targetParent = playerUnwearing.isInElevator
                ? playerUnwearing.playersManager.elevatorTransform
                : playerUnwearing.playersManager.propsContainer;

            transform.SetParent(targetParent, true);

            playerUnwearing.SetItemInElevator(
                playerUnwearing.isInHangarShipRoom,
                playerUnwearing.isInElevator,
                this
            );

            EnablePhysics(true);
            startFallingPosition = transform.parent.InverseTransformPoint(transform.position);
            fallTime = 0f;
            FallToGround(true);

            if (playerUnwearing == localPlayer)
            {
                //localPlayer.GrabGrabbableObject(this);
            }

            playerWornBy.RemoveWearable(this);

            GetComponent<Collider>().enabled = true;
            scanNode?.gameObject.SetActive(true);
            EnableItemMeshes(true);
            OnUnWear();
            playerWornBy = null;
        }
    }
}
