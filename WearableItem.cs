using System.Collections.Generic;
using UnityEngine;

namespace WearableItemsAPI
{
    [CreateAssetMenu(menuName = "WearableItemsAPI/WearableItem")]
    public class WearableItem : ScriptableObject
    {
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

        [Header("Restrictions")]

        [Tooltip("Prevents this item from being worn with any item sharing the same restrictions.")]
        public List<string> restrictions = new();

        [Tooltip("Prevents other items from being worn in the same slot.")]
        public bool restrictSlot = false;


        [Header("Positioning (Other Players)")]

        [Tooltip("Body part this item attaches to. Used when no bone transform is specified. This also determines which body part is highlighted in the UI when hovering over the item in the menu.")]
        public WearableSlot slot = WearableSlot.None;

        [Tooltip("Name of the bone transform the item should be attached to. Leave empty to use slots. Type '/wi_transforms' in game chat to log available transforms.")]
        public string boneTransform = "";

        [Tooltip("Positional offset relative to the assigned slot or bone transform.")]
        public Vector3 wornPositionOffset = Vector3.zero;

        [Tooltip("Rotational offset relative to the assigned slot or bone transform.")]
        public Vector3 wornRotationOffset = Vector3.zero;


        [Header("Positioning (Local Player)")]

        [Tooltip("Name of the bone transform the item should be attached to for the player wearing it. Leave empty to use slots or `boneTransform`. Type '/wi_transformslocal' in chat to log available transforms.")]
        public string boneTransformLocal = "";

        [Tooltip("Should local offsets be used for the local player?")]
        public bool useLocalOffsets = false;

        [Tooltip("Positional offset for the local player view.")]
        public Vector3 wornPositionOffsetLocal = Vector3.zero;

        [Tooltip("Rotational offset for the local player view.")]
        public Vector3 wornRotationOffsetLocal = Vector3.zero;


        [Header("Visibility")]

        [Tooltip("Determines if the wearable is visible to other players.")]
        public bool showWearable = true;

        [Tooltip("Determines if the wearable is visible to the local player.")]
        public bool showWearableOnClient = true;


        [Header("UI")]

        [Tooltip("Icon shown in the wearable UI. Leave empty to use the item's default icon.")]
        public Sprite icon = null!;


        [Header("Behavior")]

        [Tooltip("Automatically wear this item when used by the player.")]
        public bool wearOnUse = false;
    }
}
