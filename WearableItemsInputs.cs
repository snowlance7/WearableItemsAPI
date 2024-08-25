using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine.InputSystem;

namespace WearableItemsAPI
{
    internal class WearableItemsInputs : LcInputActions
    {
        public static WearableItemsInputs Instance;

        public static void Init()
        {
            Instance = new WearableItemsInputs();
        }

        [InputAction(KeyboardControl.I, Name = "OpenInventoryUI")]
        public InputAction OpenUIKey { get; set; }
    }
}
