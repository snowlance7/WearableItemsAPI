# Initial Setup

If you haven't already, create a Unity 2022.3.62f2 project. You can follow the official setup guide [here](https://lethal.wiki/dev/initial-setup).

Next, import **Snowlance.WearableItemsAPI.Core.dll** into your Unity project.

This guide assumes you already know how to create and register custom items. If you don't, it's recommended that you learn that process first before continuing.

# Unity Setup

Creating a wearable item is very similar to creating a shop or scrap item.

Instead of adding a **PhysicsProp** component to your prefab, add a **WearableObject** component. If your item has custom code, simply have your script inherit from **WearableObject** instead of **PhysicsProp**.

Configure the item as you normally would. The only additional requirement is assigning a **WearableItem** asset.

A **WearableItem** is a **ScriptableObject** that stores information about how the item should be worn, such as its position, rotation, and other wearable-specific settings.

## Creating a WearableItem
1. **Right-click** in the **Project** window.
2. Select **Create** → **WearableItemsAPI** → **WearableItem**.
3. Name the newly created asset.

## Configuring a WearableItem

A **WearableItem** primarily defines where an item is positioned when equipped.

Set the position and rotation offsets relative to the player's body, similar to configuring the **PositionOffset** and **RotationOffset** fields in **ItemProperties**.

Every field includes a tooltip in the Unity Inspector. Hover over a property's label to see an explanation of what it does.

Finally, assign the **WearableItem** asset to your **WearableObject** component and register the item as you normally would.