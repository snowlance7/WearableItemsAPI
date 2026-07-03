using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine.InputSystem;

namespace WearableItemsAPI.UI
{
    internal class WearableItemsInputs : LcInputActions
    {
#pragma warning disable CS8618
        public static WearableItemsInputs Instance = new WearableItemsInputs();
#pragma warning restore CS8618

        public string OpenUIKey_BindingDisplayString => OpenUIKey.GetBindingDisplayString(StartOfRound.Instance.localPlayerUsingController ? 1 : 0);

        [InputAction(KeyboardControl.I, Name = "OpenWearableItemsUI")]
        public InputAction OpenUIKey { get; set; } = null!;
    }
}
