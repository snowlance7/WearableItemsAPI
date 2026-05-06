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

        [Tooltip("Wears this item if used by the player holding the item.")]
        public bool wearOnUse = false;

        [Tooltip("Prevents this item from being worn with any item sharing the same restriction and vice-versa.")]
        public List<string> restrictions = new List<string>();

        [Tooltip("Body part this item attaches to. Use offsets to adjust alignment.")]
        public WearableSlot slot = WearableSlot.None;

        [Tooltip("Prevents other items from being worn in the same slot.")]
        public bool restrictSlot = false;

        [Tooltip("Positional offset relative to the assigned slot.")]
        public Vector3 wornPositionOffset = Vector3.zero;

        [Tooltip("Rotational offset relative to the assigned slot.")]
        public Vector3 wornRotationOffset = Vector3.zero;

        [Tooltip("Determines if the wearable is visible to all players.")]
        public bool showWearable = true;

        [Tooltip("Determines if the wearable is visible to the local player.")]
        public bool showWearableOnClient = true;

        [Tooltip("This is what will show on the button for the wearable in the UI. Leave empty to use the items default icon.")]
        public Sprite icon = null!;
    }
}
