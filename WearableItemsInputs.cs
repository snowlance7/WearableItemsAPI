using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine.InputSystem;

namespace WearableItemsAPI
{
    internal class WearableItemsInputs : LcInputActions
    {
#pragma warning disable CS8618
        public static WearableItemsInputs Instance = new WearableItemsInputs();
#pragma warning restore CS8618

        public static string OpenUIKeybind => InputControlPath.ToHumanReadableString(WearableItemsInputs.Instance.OpenUIKey.bindings[0].path, InputControlPath.HumanReadableStringOptions.OmitDevice);

        [InputAction(KeyboardControl.I, Name = "OpenWearableItemsUI")]
        public InputAction OpenUIKey { get; set; } = null!;
    }
}
