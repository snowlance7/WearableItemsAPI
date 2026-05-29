using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WearableItemsAPI
{
    public static class PlayerWearables
    {
        private static readonly Dictionary<PlayerControllerB, List<WearableObject>> wearableLookup = new();

        public static List<WearableObject> GetWornItems(this PlayerControllerB player)
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
            player.GetWornItems().Add(item);
        }

        internal static void RemoveWearable(this PlayerControllerB player, WearableObject item)
        {
            player.GetWornItems().Remove(item);
        }

        internal static void UpdateWearables()
        {
            foreach (var player in wearableLookup.Keys.ToList())
            {
                if (player == null)
                {
                    wearableLookup.Remove(player);
                }
            }
        }
    }
}
