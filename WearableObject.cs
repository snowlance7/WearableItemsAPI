using GameNetcodeStuff;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Authentication.Generated;
using UnityEngine;
using static WearableItemsAPI.Plugin;
using static WearableItemsAPI.WearableItem;

namespace WearableItemsAPI
{
    public class WearableObject : PhysicsProp
    {
        internal static PlayerControllerB localPlayer { get { return StartOfRound.Instance.localPlayerController; } }

        /// <remarks>
        /// This is automatically set by the API and should not be set manually.
        /// </remarks>
        /// <returns>
        /// The player currently wearing this item. Returns null if no player is wearing it.
        /// </returns>
        public PlayerControllerB? playerWornBy { get; private set; }

        /// <remarks>
        /// This is automatically set by the API and should not be set manually.
        /// </remarks>
        /// <returns>
        /// The last player who wore this item. Returns null if no player has worn it.
        /// </returns>
        public PlayerControllerB? lastPlayerWornBy { get; private set; }

        /// <summary>
        /// Information about how the item should be worn, such as its position, rotation, and other wearable-specific settings.
        /// </summary>
        /// <remarks>
        /// This should be set in Unity or in the Awake method.
        /// </remarks>
        [Tooltip("Information about how the item should be worn, such as its position, rotation, and other wearable-specific settings.")]
        public WearableItem wearableItemProperties = null!;

        ScanNodeProperties? scanNode;

        public override void Start()
        {
            base.Start();
            scanNode = gameObject.GetComponentInChildren<ScanNodeProperties>();
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
                    UnwearRpc();
                    return;
                }
            }
        }

        public override void LateUpdate()
        {
            if (parentObject != null && playerWornBy != null)
            {
                transform.rotation = parentObject.rotation;
                transform.Rotate(wearableItemProperties.useLocalOffsets && localPlayer == playerWornBy ? wearableItemProperties.wornRotationOffsetLocal : wearableItemProperties.wornRotationOffset);
                transform.position = parentObject.position;
                Vector3 positionOffset = wearableItemProperties.useLocalOffsets && localPlayer == playerWornBy ? wearableItemProperties.wornPositionOffsetLocal : wearableItemProperties.wornPositionOffset;
                positionOffset = parentObject.rotation * positionOffset;
                transform.position += positionOffset;

                if (radarIcon != null)
                {
                    radarIcon.position = transform.position;
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

        /// <summary>
        /// Wears the item on the local player.
        /// </summary>
        public void WearItem()
        {
            if (playerWornBy != null || !CanWear(localPlayer)) { return; }
            WearRpc(localPlayer.actualClientId);
        }

        /// <summary>
        /// Wears the item on a provided player.
        /// </summary>
        /// <param name="player">The player the wearable should be worn on.</param>
        public void WearItem(PlayerControllerB player)
        {
            if (playerWornBy != null || !CanWear(player)) { return; }
            WearRpc(player.actualClientId);
        }

        /// <summary>
        /// Unwears the item for the player wearing it.
        /// </summary>
        /// <remarks>
        /// Does nothing if a player isn't wearing this item.
        /// </remarks>
        public void UnWearItem()
        {
            if (playerWornBy == null) { return; }
            UnwearRpc();
        }

        /// <summary>
        /// Determines if the provided player can wear this item.
        /// </summary>
        /// <param name="player">The player to wear this item.</param>
        /// <returns>
        /// True if the player can wear the item.
        /// </returns>
        public bool CanWear(PlayerControllerB player)
        {
            var current = wearableItemProperties;

            foreach (var item in localPlayer.GetWearables())
            {
                var other = item.wearableItemProperties;

                if ((other.restrictSlot || current.restrictSlot) && other.slot == current.slot || current.restrictions.Any(r => other.restrictions.Contains(r)))
                {
                    HUDManager.Instance.DisplayTip("Can't wear item", $"'{item.itemProperties.itemName}' is preventing you from wearing this item", true);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// What should happen when a player wears this item.
        /// </summary>
        /// <remarks>
        /// This is automatically called when the player wears the item. It should not be called manually.
        /// </remarks>
        public virtual void OnWear() { }

        /// <summary>
        /// What should happen when a player unwears this item.
        /// </summary>
        /// <remarks>
        /// This is automatically called when the player unwears the item. It should not be called manually.
        /// </remarks>
        public virtual void OnUnWear() { }

        internal Transform GetWearableParentObject()
        {
            logger.LogDebug("Getting wearable parent object");
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

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        internal void WearRpc(ulong clientId)
        {
            if (playerWornBy != null) { Debug.Log("Player already wearing item"); return; }
            PlayerControllerB? player = StartOfRound.Instance.allPlayerScripts.Where(x => x.actualClientId == clientId).FirstOrDefault();
            if (player == null) { Debug.LogError("Couldn't get player from player client id"); return; }

            Debug.Log(player.playerUsername + " wearing " + itemProperties.itemName);

            playerWornBy = player;
            playerWornBy.DiscardHeldObject(false, playerWornBy.NetworkObject);

            if (localPlayer != playerWornBy)
            {
                StartCoroutine(DelayWear(playerWornBy));
                return;
            }

            parentObject = GetWearableParentObject();
            logger.LogDebug($"Parent set to {parentObject.name}");

            gameObject.GetComponent<Collider>().enabled = false;
            bool _showWearable = localPlayer == playerWornBy ? wearableItemProperties.showWearableOnClient : wearableItemProperties.showWearable;
            EnableItemMeshes(_showWearable);
            scanNode?.gameObject.SetActive(false);

            player.AddWearable(this);
            OnWear();
        }

        IEnumerator DelayWear(PlayerControllerB player)
        {
            yield return new WaitForSeconds(0.1f);

            parentObject = GetWearableParentObject();
            logger.LogDebug($"Parent set to {parentObject.name}");

            gameObject.GetComponent<Collider>().enabled = false;
            bool _showWearable = localPlayer == playerWornBy ? wearableItemProperties.showWearableOnClient : wearableItemProperties.showWearable;
            EnableItemMeshes(_showWearable);
            scanNode?.gameObject.SetActive(false);

            player.AddWearable(this);
            OnWear();
        }

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        internal void UnwearRpc()
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
