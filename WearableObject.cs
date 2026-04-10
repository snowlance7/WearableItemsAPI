using BepInEx.Logging;
using GameNetcodeStuff;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        public static List<WearableObject> wornItems = new List<WearableObject>();

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
                    UnWear();
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

        public void RequestWear(PlayerControllerB player)
        {
            if (playerWornBy != null) { return; }
            WearServerRpc(player.actualClientId);
        }

        public virtual void Wear(PlayerControllerB player)
        {
            logger.LogDebug("Wearing " + itemProperties.itemName);

            playerWornBy = player;
            playerWornBy.DiscardHeldObject(false, playerWornBy.NetworkObject);

            parentObject = wearableItemProperties.slot == WearableSlot.None ? playerWornBy.transform : playerWornBy.bodyParts[(int)wearableItemProperties.slot];
            base.gameObject.GetComponent<Collider>().enabled = false;
            bool _showWearable = localPlayer == playerWornBy ? wearableItemProperties.showWearableOnClient : wearableItemProperties.showWearable;
            EnableItemMeshes(_showWearable);
            scanNode?.gameObject.SetActive(false);

            wornItems.Add(this);

            HUDManager.Instance.DisplayTip("WearableItemsAPI", $"Press {WearableItemsInputs.Instance.OpenUIKey.activeControl.displayName} to open the Wearable Items UI", false, true, "WearableItemsAPITip1"); // TODO: Test this
        }

        public virtual void UnWear()
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

            if (playerWornBy == localPlayer)
                wornItems.Remove(this);

            GetComponent<Collider>().enabled = true;
            scanNode?.gameObject.SetActive(true);
            EnableItemMeshes(true);
            playerWornBy = null;
        }

        // RPCs
        [ServerRpc(RequireOwnership = false)]
        public void WearServerRpc(ulong clientId)
        {
            if (!IsServer) { return; }
            WearClientRpc(clientId);
        }

        [ClientRpc]
        public void WearClientRpc(ulong clientId)
        {
            PlayerControllerB? player = PlayerFromId(clientId);
            if (player == null) { logger.LogError("Couldn't get player from player client id"); return; }
            Wear(player);
        }

        [ServerRpc(RequireOwnership = false)]
        public void UnwearServerRpc()
        {
            if (!IsServer) { return; }
            UnwearClientRpc();
        }

        [ClientRpc]
        public void UnwearClientRpc()
        {
            UnWear();
        }
    }
}
