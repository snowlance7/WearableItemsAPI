using BepInEx.Logging;
using GameNetcodeStuff;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static WearableItemsAPI.Plugin;

namespace WearableItemsAPI
{
    public class WearableItem : PhysicsProp
    {
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

        public PlayerControllerB? playerWornBy;

        private Transform? wornPos;

        public enum WearableSlot
        {
            None,
            Head,
            Chest,
            RightArm,
            LeftArm,
            BothArms,
            Legs,
            Feet
        }

        public WearableSlot WearSlot;
        public Vector3 wearablePositionOffset;
        public Vector3 wearableRotationOffset;
        public bool showWearable;
        public bool showWearableOnClient;

        //public ScanNodeProperties ScanNode;

        public override void Update()
        {
            base.Update();
            if (playerWornBy != null)
            {
                if (playerWornBy.isPlayerDead)
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
                base.transform.Rotate(wearableRotationOffset);
                base.transform.position = parentObject.position;
                Vector3 positionOffset = wearablePositionOffset;
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

        public virtual void Wear() // TODO: Make sure showWearableOnClient works
        {
            LoggerInstance.LogDebug("Wearing " + itemProperties.itemName);

            if (playerHeldBy != null)
            {
                playerWornBy = playerHeldBy;
            }
            else
            {
                LoggerInstance.LogError("No player holding object found and no player was provided");
                return;
            }

            SetWearSlot(WearSlot, this);

            playerHeldBy.DiscardHeldObject(false, playerHeldBy.NetworkObject);

            parentObject = wornPos;
            base.gameObject.GetComponent<Collider>().enabled = false;
            //ScanNode.enabled = false; // TODO: This isnt working, figure out how to make it unscannable to other players

            WearServerRpc(playerWornBy.actualClientId, WearSlot, showWearable, wearablePositionOffset, wearableRotationOffset);
        }

        public virtual void Wear(PlayerControllerB player) // TODO: Make sure showWearableOnClient works
        {
            playerWornBy = player;

            SetWearSlot(WearSlot, this);

            if (playerHeldBy != null)
            {
                playerHeldBy.DiscardHeldObject(false, playerHeldBy.NetworkObject);
            }

            parentObject = wornPos;
            base.gameObject.GetComponent<Collider>().enabled = false;
            //ScanNode.enabled = false; // TODO: This isnt working, figure out how to make it unscannable to other players

            WearServerRpc(playerWornBy.actualClientId, WearSlot, showWearable, wearablePositionOffset, wearableRotationOffset);
        }

        public virtual void UnWear(bool grabItem = true)
        {
            UnwearServerRpc(grabItem);
        }

        public void SetWearSlot(WearableSlot slot, GrabbableObject? itemToSlot = null)
        {
            if (playerWornBy == null)
            {
                LoggerInstance.LogError("PlayerWornBy is null");
                return;
            }

            WearSlot = slot;
            wornPos = null;

            switch (WearSlot)
            {
                case WearableSlot.Head:
                    if (itemToSlot != null) { wornPos = playerWornBy.bodyParts[0]; }
                    if (playerWornBy == localPlayer) { WearableUIController.Instance.HeadItem = itemToSlot; }
                    break;
                case WearableSlot.RightArm:
                    if (itemToSlot != null) { wornPos = playerWornBy.bodyParts[1]; }
                    if (playerWornBy == localPlayer) { WearableUIController.Instance.RightArmItem = itemToSlot; }
                    break;
                case WearableSlot.LeftArm:
                    if (itemToSlot != null) { wornPos = playerWornBy.bodyParts[2]; }
                    if (playerWornBy == localPlayer) { WearableUIController.Instance.LeftArmItem = itemToSlot; }
                    break;
                case WearableSlot.BothArms:
                    //bodyPos = playerWornBy.bodyParts[];
                    break;
                case WearableSlot.Chest:
                    if (itemToSlot != null) { wornPos = playerWornBy.bodyParts[5]; }
                    if (playerWornBy == localPlayer) { WearableUIController.Instance.ChestItem = itemToSlot; }
                    break;
                case WearableSlot.Legs:
                    if (itemToSlot != null) { wornPos = playerWornBy.bodyParts[8]; }
                    if (playerWornBy == localPlayer) { WearableUIController.Instance.LegsItem = itemToSlot; }
                    break;
                case WearableSlot.Feet:
                    if (itemToSlot != null) { wornPos = playerWornBy.bodyParts[6]; }
                    if (playerWornBy == localPlayer) { WearableUIController.Instance.FeetItem = itemToSlot; }
                    break;
                case WearableSlot.None:
                    if (itemToSlot != null) { wornPos = playerWornBy.transform; }
                    EnableItemMeshes(false);
                    break;
                default:
                    LoggerInstance.LogError("Wearslot not found");
                    break;
            }
        }

        // RPCs
        [ServerRpc(RequireOwnership = false)]
        private void WearServerRpc(ulong clientId, WearableSlot slot, bool _showWearable, Vector3 _wearablePositionOffset, Vector3 _wearableRotationOffset)
        {
            if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
            {
                WearClientRpc(clientId, slot, _showWearable, _wearablePositionOffset, _wearableRotationOffset);
            }
        }

        [ClientRpc]
        private void WearClientRpc(ulong clientId, WearableSlot slot, bool _showWearable, Vector3 _wearablePositionOffset, Vector3 _wearableRotationOffset)
        {
            if (localPlayer.actualClientId == clientId) { return; }

            playerWornBy = StartOfRound.Instance.allPlayerScripts.Where(x => x.actualClientId == clientId).First();
            SetWearSlot(slot, this);
            EnableItemMeshes(_showWearable);
            wearablePositionOffset = _wearablePositionOffset;
            wearableRotationOffset = _wearableRotationOffset;

            parentObject = wornPos;
            base.gameObject.GetComponent<Collider>().enabled = false;
            //ScanNode.enabled = false;
        }

        [ServerRpc(RequireOwnership = false)]
        private void UnwearServerRpc(bool grabItem = true)
        {
            if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
            {
                UnwearClientRpc(grabItem);
            }
        }

        [ClientRpc]
        private void UnwearClientRpc(bool grabItem = true)
        {
            parentObject = null;

            if (playerWornBy != null && playerWornBy == localPlayer)
            {
                SetWearSlot(WearSlot, null);

                if (!playerWornBy.isPlayerDead && grabItem)
                {
                    LoggerInstance.LogDebug("Grabbing " + itemProperties.itemName);
                    playerWornBy.GrabObjectServerRpc(NetworkObject);
                    parentObject = playerWornBy.localItemHolder;
                    GrabItemOnClient();
                }
            }

            base.gameObject.GetComponent<Collider>().enabled = true;
            //ScanNode.enabled = true;
            EnableItemMeshes(true);
            playerWornBy = null;
            wornPos = null;
        }
    }
}
