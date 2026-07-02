using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine.Events;

namespace WearableItemsAPI.Core
{
    public static class PlayerWearables
    {
        private static readonly Dictionary<PlayerControllerB, List<WearableObject>> wearableLookup = new();

        internal static UnityEvent OnWearablesUpdate = new UnityEvent();

        /// <summary>
        /// Gets all of the player's currently worn items.
        /// </summary>
        public static List<WearableObject> GetWearables(this PlayerControllerB player)
        {
            if (!wearableLookup.TryGetValue(player, out var list))
            {
                list = new List<WearableObject>();
                wearableLookup[player] = list;
            }

            return list;
        }

        internal static void AddWearable(this PlayerControllerB player, WearableObject item)
        {
            var wearables = player.GetWearables();
            wearables.Add(item);
            OnWearablesUpdate.Invoke();
        }

        internal static void RemoveWearable(this PlayerControllerB player, WearableObject item)
        {
            var wearables = player.GetWearables();
            wearables.Remove(item);
            OnWearablesUpdate.Invoke();
        }
    }
}
