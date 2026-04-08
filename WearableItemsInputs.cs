using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine.InputSystem;

namespace WearableItemsAPI
{
    internal class WearableItemsInputs : LcInputActions
    {
#pragma warning disable CS8618
        public static WearableItemsInputs Instance;
#pragma warning restore CS8618

        public static void Init()
        {
            Instance = new WearableItemsInputs();
        }

#pragma warning disable CS8618
        [InputAction(KeyboardControl.I, Name = "OpenInventoryUI")]
        public InputAction OpenUIKey { get; set; }
#pragma warning restore CS8618
    }
}
