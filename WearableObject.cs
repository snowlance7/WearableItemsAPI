using GameNetcodeStuff;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static WearableItemsAPI.Plugin;
using static WearableItemsAPI.WearableItem;

namespace WearableItemsAPI
{
    public abstract class WearableObject : PhysicsProp
    {
        public PlayerControllerB? playerWornBy { get; private set; }
        public PlayerControllerB? lastPlayerWornBy { get; private set; }

        //public static List<WearableObject> wornItems = new List<WearableObject>();
        //public static Dictionary<PlayerControllerB, List<WearableObject>> wornItems = new Dictionary<PlayerControllerB, List<WearableObject>>();

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

                if (!playerWornBy.isPlayerControlled)
                {
                    OnUnWear();
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

        public void UnWearItem()
        {
            if (playerWornBy == null) { return; }
            UnwearServerRpc();
        }

        public virtual void OnWear(PlayerControllerB playerWearing)
        {
            if (playerWornBy != null) { logger.LogDebug("Player already wearing item"); return; }
            logger.LogDebug(playerWearing.playerUsername + " wearing " + itemProperties.itemName);

            playerWornBy = playerWearing;
            playerWornBy.DiscardHeldObject(false, playerWornBy.NetworkObject);

            parentObject = wearableItemProperties.slot == WearableSlot.None ? playerWornBy.transform : playerWornBy.bodyParts[(int)wearableItemProperties.slot];
            base.gameObject.GetComponent<Collider>().enabled = false;
            bool _showWearable = localPlayer == playerWornBy ? wearableItemProperties.showWearableOnClient : wearableItemProperties.showWearable;
            EnableItemMeshes(_showWearable);
            scanNode?.gameObject.SetActive(false);

            playerWearing.AddWearable(this);

            HUDManager.Instance.DisplayTip("WearableItemsAPI", $"Press {WearableItemsInputs.OpenUIKeybind} to open the Wearable Items UI", false, true, "WearableItemsAPITip1"); // TODO: Test this
        }

        public virtual void OnUnWear() // TODO: Set this up so that it puts item in empty item slot, or if full drops it on the ground. also switch equiped item to the wearable when its unworn
        {
            if (playerWornBy == null) return;

            if (!playerWornBy.isPlayerControlled)
            {
                logger.LogDebug("Player is dead, unwearing item");

                parentObject = null;

                Transform targetParent = playerWornBy.isInElevator
                    ? playerWornBy.playersManager.elevatorTransform
                    : playerWornBy.playersManager.propsContainer;

                transform.SetParent(targetParent, true);

                playerWornBy.SetItemInElevator(
                    playerWornBy.isInHangarShipRoom,
                    playerWornBy.isInElevator,
                    this
                );

                EnablePhysics(true);
                startFallingPosition = transform.parent.InverseTransformPoint(transform.position);
                fallTime = 0f;
                FallToGround(true);
            }
            else if (playerWornBy == localPlayer)
            {
                logger.LogDebug("Grabbing " + itemProperties.itemName);

                playerWornBy.GrabObjectServerRpc(NetworkObject);
                parentObject = playerWornBy.localItemHolder;
                GrabItemOnClient();
            }

            playerWornBy.RemoveWearable(this);

            GetComponent<Collider>().enabled = true;
            scanNode?.gameObject.SetActive(true);
            EnableItemMeshes(true);
            playerWornBy = null;
        }

        // RPCs
        [ServerRpc(RequireOwnership = false)]
        protected void WearServerRpc(ulong clientId)
        {
            if (!IsServer) { return; }
            WearClientRpc(clientId);
        }

        [ClientRpc]
        protected void WearClientRpc(ulong clientId)
        {
            PlayerControllerB? player = StartOfRound.Instance.allPlayerScripts.Where(x => x.actualClientId == clientId).FirstOrDefault();
            if (player == null) { logger.LogError("Couldn't get player from player client id"); return; }
            OnWear(player);
        }

        [ServerRpc(RequireOwnership = false)]
        protected void UnwearServerRpc()
        {
            if (!IsServer) { return; }
            UnwearClientRpc();
        }

        [ClientRpc]
        protected void UnwearClientRpc()
        {
            OnUnWear();
        }
    }
}
