using BepInEx.Logging;
using GameNetcodeStuff;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Netcode;
using UnityEngine;
using static WearableItemsAPI.Plugin;

namespace WearableItemsAPI
{
    public abstract class WearableItem : PhysicsProp
    {
        public PlayerControllerB? playerWornBy { get; private set; }
        public PlayerControllerB? lastPlayerWornBy { get; private set; }

        public enum WearableSlot
        {
            None = -1,
            Head = 0,
            RightArm = 1,
            LeftArm = 2,
            RightLeg = 3,
            LeftLeg = 4,
            Chest = 5,
            Feet = 6,
            RightHip = 7,
            LeftHip = 8,
            LeftShoulder = 9,
            RightShoulder = 10
        }

        /* bodyparts
         * 0 head
         * 1 right arm
         * 2 left arm
         * 3 right leg
         * 4 left leg
         * 5 chest
         * 6 feet
         * 7 right hip
         * 8 left hip
         * 9 left shoulder
         * 10 right shoulder */

        public string Restriction = "";

        ScanNodeProperties? scanNode;
        public abstract WearableSlot WornSlot { get; }
        public Vector3 wornPositionOffset = Vector3.zero;
        public Vector3 wornRotationOffset = Vector3.zero;
        public bool showWearable = true;
        public bool showWearableOnClient = true;

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
                base.transform.Rotate(wornRotationOffset);
                base.transform.position = parentObject.position;
                Vector3 positionOffset = wornPositionOffset;
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

        public virtual void Wear(PlayerControllerB player)
        {
            logger.LogDebug("Wearing " + itemProperties.itemName);

            playerWornBy = player;
            playerWornBy.DiscardHeldObject(false, playerWornBy.NetworkObject);

            parentObject = WornSlot == WearableSlot.None ? playerWornBy.transform : playerWornBy.bodyParts[(int)WornSlot];
            base.gameObject.GetComponent<Collider>().enabled = false;
            bool _showWearable = localPlayer == playerWornBy ? showWearableOnClient : showWearable;
            EnableItemMeshes(_showWearable);
            scanNode?.gameObject.SetActive(false);

            HUDManager.Instance.DisplayTip("Wearable Items", $"Press {WearableItemsInputs.Instance.OpenUIKey.activeControl.displayName} to open the Wearable Items inventory", false, true, "WearableItemsTip1"); // TODO: Test this
        }

        public virtual void UnWear()
        {
            if (playerWornBy != null)
            {
                if (!playerWornBy.isPlayerControlled)
                {
                    logger.LogDebug("Player is dead, unwearing item");

                    parentObject = null;
                    if (playerWornBy.isInElevator)
                    {
                        base.transform.SetParent(playerWornBy.playersManager.elevatorTransform, true);
                    }
                    else
                    {
                        base.transform.SetParent(playerWornBy.playersManager.propsContainer, true);
                    }
                    playerWornBy.SetItemInElevator(playerWornBy.isInHangarShipRoom, playerWornBy.isInElevator, this);
                    EnablePhysics(true);
                    startFallingPosition = this.transform.parent.InverseTransformPoint(this.transform.position);
                    fallTime = 0f;
                    FallToGround(true);
                }

                if (playerWornBy == localPlayer)
                {
                    if (playerWornBy.isPlayerControlled)
                    {
                        logger.LogDebug("Grabbing " + itemProperties.itemName);
                        playerWornBy.GrabObjectServerRpc(NetworkObject);
                        parentObject = playerWornBy.localItemHolder;
                        GrabItemOnClient();
                    }
                }
            }

            base.gameObject.GetComponent<Collider>().enabled = true;
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
            PlayerControllerB player = PlayerFromId(clientId);
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
