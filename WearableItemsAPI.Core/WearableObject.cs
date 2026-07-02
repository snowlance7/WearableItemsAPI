using GameNetcodeStuff;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static WearableItemsAPI.Core.WearableItem;

namespace WearableItemsAPI.Core
{
    public abstract class WearableObject : PhysicsProp
    {
        internal static PlayerControllerB localPlayer { get { return StartOfRound.Instance.localPlayerController; } }
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
                    Debug.Log($"Player ({playerWornBy.playerUsername}) died or disconnected, unwearing item {itemProperties.itemName}");
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
                base.transform.Rotate(wearableItemProperties.useLocalOffsets && localPlayer == playerWornBy ? wearableItemProperties.wornRotationOffsetLocal : wearableItemProperties.wornRotationOffset);
                base.transform.position = parentObject.position;
                Vector3 positionOffset = wearableItemProperties.useLocalOffsets && localPlayer == playerWornBy ? wearableItemProperties.wornPositionOffsetLocal : wearableItemProperties.wornPositionOffset;
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

        public void WearItem()
        {
            if (playerWornBy != null || !CanWear(localPlayer)) { return; }
            WearServerRpc(localPlayer.actualClientId);
        }

        public void WearItem(PlayerControllerB player)
        {
            if (playerWornBy != null || !CanWear(player)) { return; }
            WearServerRpc(player.actualClientId);
        }

        public void UnWearItem()
        {
            if (playerWornBy == null) { return; }
            UnwearServerRpc();
        }

        public bool CanWear(PlayerControllerB player)
        {
            var current = wearableItemProperties;

            foreach (var item in localPlayer.GetWearables())
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

        internal Transform GetWearableParentObject()
        {
            var p = wearableItemProperties;

            Transform root = playerWornBy!.playerBodyAnimator.transform;

            if (localPlayer == playerWornBy)
            {
                if (!string.IsNullOrWhiteSpace(p.boneTransformLocal))
                {
                    Transform? t = root.Find("ScavengerModelArmsOnly")?.Find(p.boneTransformLocal);

                    if (t != null)
                        return t;
                }
            }

            if (!string.IsNullOrWhiteSpace(p.boneTransform))
            {
                Transform? t = root.Find("spine")?.Find(p.boneTransform);

                if (t != null)
                    return t;
            }

            if (p.slot != WearableSlot.None)
                return playerWornBy.bodyParts[(int)p.slot];

            return playerWornBy.transform;
        }

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
            if (playerWornBy != null) { Debug.Log("Player already wearing item"); return; }
            PlayerControllerB? player = StartOfRound.Instance.allPlayerScripts.Where(x => x.actualClientId == clientId).FirstOrDefault();
            if (player == null) { Debug.LogError("Couldn't get player from player client id"); return; }

            Debug.Log(player.playerUsername + " wearing " + itemProperties.itemName);

            playerWornBy = player;
            playerWornBy.DiscardHeldObject(false, playerWornBy.NetworkObject);

            parentObject = GetWearableParentObject();

            base.gameObject.GetComponent<Collider>().enabled = false;
            bool _showWearable = localPlayer == playerWornBy ? wearableItemProperties.showWearableOnClient : wearableItemProperties.showWearable;
            EnableItemMeshes(_showWearable);
            scanNode?.gameObject.SetActive(false);

            player.AddWearable(this);
            OnWear();
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
            FallToGround();

            if (playerUnwearing == localPlayer)
            {
                localPlayer.GrabGrabbableObject(this);
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
