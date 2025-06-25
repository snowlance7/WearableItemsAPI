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
    public class WearableItem : PhysicsProp
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

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

        //internal static HashSet<string> AllRestrictions = [];
        //public string Restriction = "";
        //public string[] SubRestrictions = [];

        public ScanNodeProperties? ScanNode;
        public WearableSlot WornSlot = WearableSlot.None;
        public Vector3 wornPositionOffset = Vector3.zero;
        public Vector3 wornRotationOffset = Vector3.zero;
        public bool showWearable = true;
        public bool showWearableOnClient = true;

        public override void Start()
        {
            base.Start();
            //Restriction = Regex.Replace(Restriction, @"\s+", "").ToLower();
            //SubRestrictions = SubRestrictions.Select(s => Regex.Replace(s, @"\s+", "").ToLower()).ToArray();
        }

        public override void Update()
        {
            base.Update();
            if (playerWornBy != null)
            {
                lastPlayerWornBy = playerWornBy;

                if (!playerWornBy.isPlayerControlled)
                {
                    UnWear(grabItem: false);
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

        public virtual void Wear(ulong clientId)
        {
            LoggerInstance.LogDebug("Wearing " + itemProperties.itemName);

            playerWornBy = PlayerFromId(clientId);
            playerWornBy.DiscardHeldObject(false, playerWornBy.NetworkObject);

            parentObject = WornSlot == WearableSlot.None ? playerWornBy.transform : playerWornBy.bodyParts[(int)WornSlot];
            base.gameObject.GetComponent<Collider>().enabled = false;
            bool _showWearable = localPlayer == playerWornBy ? showWearableOnClient : showWearable;
            EnableItemMeshes(_showWearable);
            ScanNode?.gameObject.SetActive(false);

            HUDManager.Instance.DisplayTip("Wearable Items", $"Press I to open the Wearable Items inventory", false, true, "WearableItemsTip1");
        }

        public virtual void UnWear(bool grabItem = true)
        {
            if (playerWornBy != null)
            {
                if (!playerWornBy.isPlayerControlled || !grabItem)
                {
                    LoggerInstance.LogDebug("Player is dead");

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
                    if (playerWornBy.isPlayerControlled && grabItem)
                    {
                        LoggerInstance.LogDebug("Grabbing " + itemProperties.itemName);
                        playerWornBy.GrabObjectServerRpc(NetworkObject);
                        parentObject = playerWornBy.localItemHolder;
                        GrabItemOnClient();
                    }
                }
            }

            base.gameObject.GetComponent<Collider>().enabled = true;
            ScanNode?.gameObject.SetActive(true);
            EnableItemMeshes(true);
            playerWornBy = null;
        }


        // RPCs
        [ServerRpc(RequireOwnership = false)]
        public void WearServerRpc(ulong clientId)
        {
            if (!IsServerOrHost) { return; }
            WearClientRpc(clientId);
        }

        [ClientRpc]
        public void WearClientRpc(ulong clientId)
        {
            Wear(clientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void UnwearServerRpc(bool grabItem = true)
        {
            if (!IsServerOrHost) { return; }
            UnwearClientRpc(grabItem);
        }

        [ClientRpc]
        public void UnwearClientRpc(bool grabItem = true)
        {
            UnWear(grabItem);
        }
    }
}
