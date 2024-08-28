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

        private static ManualLogSource logger = Plugin.LoggerInstance;

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

        public override void DiscardItem()
        {
            base.DiscardItem();

            if (playerWornBy != null)
            {
                UnWear(discard: true);
            }
        }

        public virtual void Wear() // TODO: Make sure showWearableOnClient works
        {
            logger.LogDebug("Wearing " + itemProperties.itemName);

            if (playerHeldBy != null)
            {
                playerWornBy = playerHeldBy;
            }
            else
            {
                logger.LogError("No player holding object found and no player was provided");
                return;
            }

            SetWearSlot(WearSlot);

            playerHeldBy.DiscardHeldObject(false, playerHeldBy.NetworkObject);

            parentObject = wornPos;
            base.gameObject.GetComponent<Collider>().enabled = false;
            //ScanNode.enabled = false; // TODO: This isnt working, figure out how to make it unscannable to other players

            WearServerRpc(playerWornBy.actualClientId, WearSlot, showWearable, wearablePositionOffset, wearableRotationOffset);
        }

        public virtual void Wear(PlayerControllerB player) // TODO: Make sure showWearableOnClient works
        {
            logger.LogDebug("Wearing " + itemProperties.itemName);

            playerWornBy = player;

            SetWearSlot(WearSlot);

            if (playerHeldBy != null)
            {
                playerHeldBy.DiscardHeldObject(false, playerHeldBy.NetworkObject);
            }

            parentObject = wornPos;
            base.gameObject.GetComponent<Collider>().enabled = false;
            //ScanNode.enabled = false; // TODO: This isnt working, figure out how to make it unscannable to other players

            WearServerRpc(playerWornBy.actualClientId, WearSlot, showWearable, wearablePositionOffset, wearableRotationOffset);
        }

        public virtual void UnWear(bool discard = false)
        {
            logger.LogDebug("Unwearing " + itemProperties.itemName);

            UnwearServerRpc();

            base.gameObject.GetComponent<Collider>().enabled = true;
            //ScanNode.enabled = true;
            EnableItemMeshes(true);

            switch (WearSlot)
            {
                case WearableSlot.Head:
                    WearableUIController.Instance.HeadItem = null;
                    break;
                case WearableSlot.RightArm:
                    WearableUIController.Instance.RightArmItem = null;
                    break;
                case WearableSlot.LeftArm:
                    WearableUIController.Instance.LeftArmItem = null;
                    break;
                case WearableSlot.BothArms:
                    WearableUIController.Instance.BothArmsItem = null;
                    break;
                case WearableSlot.Chest:
                    WearableUIController.Instance.ChestItem = null;
                    break;
                case WearableSlot.Legs:
                    WearableUIController.Instance.LegsItem = null;
                    break;
                case WearableSlot.Feet:
                    WearableUIController.Instance.FeetItem = null;
                    break;
                default:
                    break;
            }

            if (playerWornBy != null && !playerWornBy.isPlayerDead && !discard)
            {
                playerWornBy.GrabObjectServerRpc(NetworkObject);
                parentObject = playerWornBy.localItemHolder;
                GrabItemOnClient();
            }
            else { parentObject = null; }

            wornPos = null;
            playerWornBy = null;
        }

        public void SetWearSlot(WearableSlot slot)
        {
            if (playerWornBy == null)
            {
                logger.LogError("PlayerWornBy is null");
                return;
            }

            WearSlot = slot;

            switch (WearSlot)
            {
                case WearableSlot.Head:
                    wornPos = playerWornBy.bodyParts[0];
                    if (playerWornBy == localPlayer) { WearableUIController.Instance.HeadItem = this; }
                    break;
                case WearableSlot.RightArm:
                    wornPos = playerWornBy.bodyParts[1];
                    if (playerWornBy == localPlayer) { WearableUIController.Instance.RightArmItem = this; }
                    break;
                case WearableSlot.LeftArm:
                    wornPos = playerWornBy.bodyParts[2];
                    if (playerWornBy == localPlayer) { WearableUIController.Instance.LeftArmItem = this; }
                    break;
                case WearableSlot.BothArms:
                    //bodyPos = playerWornBy.bodyParts[];
                    break;
                case WearableSlot.Chest:
                    wornPos = playerWornBy.bodyParts[5];
                    if (playerWornBy == localPlayer) { WearableUIController.Instance.ChestItem = this; }
                    break;
                case WearableSlot.Legs:
                    wornPos = playerWornBy.bodyParts[8];
                    if (playerWornBy == localPlayer) { WearableUIController.Instance.LegsItem = this; }
                    break;
                case WearableSlot.Feet:
                    wornPos = playerWornBy.bodyParts[6];
                    if (playerWornBy == localPlayer) { WearableUIController.Instance.FeetItem = this; }
                    break;
                case WearableSlot.None:
                    wornPos = playerWornBy.transform;
                    EnableItemMeshes(false);
                    break;
                default:
                    logger.LogError("Wearslot not found");
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
            SetWearSlot(slot);
            EnableItemMeshes(_showWearable);
            wearablePositionOffset = _wearablePositionOffset;
            wearableRotationOffset = _wearableRotationOffset;

            parentObject = wornPos;
            base.gameObject.GetComponent<Collider>().enabled = false;
            //ScanNode.enabled = false;
        }

        [ServerRpc(RequireOwnership = false)]
        private void UnwearServerRpc()
        {
            if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
            {
                UnwearClientRpc();
            }
        }

        [ClientRpc]
        private void UnwearClientRpc()
        {
            if (playerWornBy == localPlayer) { return; }
            base.gameObject.GetComponent<Collider>().enabled = true;
            //ScanNode.enabled = true;
            EnableItemMeshes(true);
            playerWornBy = null;
            parentObject = null;
        }
    }
}
