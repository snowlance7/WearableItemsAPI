using BepInEx.Logging;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static WearableItemsAPI.Plugin;

namespace WearableItemsAPI
{
    public class WearableUIController : MonoBehaviour
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        public static WearableUIController Instance;

        internal VisualElement veMain;
        internal VisualElement veUIContainer;

        private Button btnHead;
        private Button btnRightArm;
        private Button btnLeftArm;
        private Button btnChest;
        private Button btnLegs;
        private Button btnFeet;

        public GrabbableObject HeadItem;
        public GrabbableObject RightArmItem;
        public GrabbableObject LeftArmItem;
        public GrabbableObject BothArmsItem;
        public GrabbableObject ChestItem;
        public GrabbableObject LegsItem;
        public GrabbableObject FeetItem;

        /* bodyparts
         * 0 head
         * 1 right arm
         * 2 left arm
         * 3 right leg
         * 4 left leg
         * 5 chest
         * 6 feet
         * 7 right hip
         * 8 crotch
         * 9 left shoulder
         * 10 right shoulder */

        internal static void Init()
        {
            logger.LogDebug("UIController: Init()");

            GameObject WearableItemsUIHandler = ModAssets.LoadAsset<GameObject>("Assets/ModAssets/WearableItemsUIHandler.prefab");

            if (WearableItemsUIHandler == null)
            {
                logger.LogError("WearableItemsUIHandler not found.");
                return;
            }

            WearableItemsUIHandler.AddComponent<WearableUIController>();
            WearableItemsUIHandler = Instantiate(WearableItemsUIHandler);
        }

        private void Start()
        {
            logger.LogDebug("UIController: Start()");

            if (Instance == null)
            {
                Instance = this;
            }
            
            // Get UIDocument
            logger.LogDebug("Getting UIDocument");
            UIDocument uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null) { logger.LogError("uiDocument not found."); return; }

            // Get VisualTreeAsset
            logger.LogDebug("Getting visual tree asset");
            if (uiDocument.visualTreeAsset == null) { logger.LogError("visualTreeAsset not found."); return; }

            // Instantiate root
            VisualElement root = uiDocument.visualTreeAsset.Instantiate();
            if (root == null) { logger.LogError("root is null!"); return; }
            logger.LogDebug("Adding root");
            uiDocument.rootVisualElement.Add(root);
            if (uiDocument.rootVisualElement == null) { logger.LogError("uiDocument.rootVisualElement not found."); return; }
            logger.LogDebug("Got root");
            root = uiDocument.rootVisualElement;

            veMain = root.Q<VisualElement>("veMain");
            if (veMain == null) { logger.LogError("veMain not found."); return; }
            veMain.style.display = DisplayStyle.None;

            veUIContainer = root.Q<VisualElement>("veUIContainer");
            if (veUIContainer == null) { logger.LogError("veUIContainer not found."); return; }
            veUIContainer.style.bottom = configUIPositionY.Value;
            veUIContainer.style.left = configUIPositionX.Value;

            VisualElement veHead = root.Q<VisualElement>("veHead");
            if (veHead == null) { logger.LogError("veHead not found."); return; }
            veHead.style.marginBottom = configSpaceBetweenHeadAndChest.Value;
            
            VisualElement veChest = root.Q<VisualElement>("veChest");
            if (veChest == null) { logger.LogError("veChest not found."); return; }
            veChest.style.marginBottom = configSpaceBetweenChestAndLegs.Value;
            if (configArmSpacing.Value) { veChest.style.justifyContent = Justify.SpaceAround; }
            
            VisualElement veLegs = root.Q<VisualElement>("veLegs");
            if (veLegs == null) { logger.LogError("veLegs not found."); return; }
            veLegs.style.marginBottom = configSpaceBetweenLegsAndFeet.Value;

            // Find elements

            btnHead = root.Q<Button>("btnHead");
            if (btnHead == null) { logger.LogError("btnHead not found."); return; }
            btnHead.style.width = configUIWidth.Value;
            btnHead.style.height = configUIHeight.Value;

            btnRightArm = root.Q<Button>("btnRightArm");
            if (btnRightArm == null) { logger.LogError("btnRightArm not found."); return; }
            btnRightArm.style.width = configUIWidth.Value;
            btnRightArm.style.height = configUIHeight.Value;
            btnRightArm.style.marginRight = configSpaceBetweenArms.Value;

            btnLeftArm = root.Q<Button>("btnLeftArm");
            if (btnLeftArm == null) { logger.LogError("btnLeftArm not found."); return; }
            btnLeftArm.style.width = configUIWidth.Value;
            btnLeftArm.style.height = configUIHeight.Value;
            btnLeftArm.style.marginLeft = configSpaceBetweenArms.Value;

            btnChest = root.Q<Button>("btnChest");
            if (btnChest == null) { logger.LogError("btnChest not found."); return; }
            btnChest.style.width = configUIWidth.Value;
            btnChest.style.height = configUIHeight.Value;

            btnLegs = root.Q<Button>("btnLegs");
            if (btnLegs == null) { logger.LogError("btnLegs not found."); return; }
            btnLegs.style.width = configUIWidth.Value;
            btnLegs.style.height = configUIHeight.Value;

            btnFeet = root.Q<Button>("btnFeet");
            if (btnFeet == null) { logger.LogError("btnFeet not found."); return; }
            btnFeet.style.width = configUIWidth.Value;
            btnFeet.style.height = configUIHeight.Value;

            logger.LogDebug("Got Controls for UI");

            // Add event handlers
            btnHead.clickable.clicked += () => ButtonHeadClicked();
            btnRightArm.clickable.clicked += () => ButtonRightArmClicked();
            btnLeftArm.clickable.clicked += () => ButtonLeftArmClicked();
            btnChest.clickable.clicked += () => ButtonChestClicked();
            btnLegs.clickable.clicked += () => ButtonLegsClicked();
            btnFeet.clickable.clicked += () => ButtonFeetClicked();

            logger.LogDebug("UIControllerScript: Start() complete");
        }

        private void Update()
        {
            if (veMain.style.display == DisplayStyle.Flex && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame)) { HideUI(); }
            
            if (WearableItemsInputs.Instance.OpenUIKey.WasPressedThisFrame() && localPlayer.CheckConditionsForEmote())
            {
                if (veMain.style.display == DisplayStyle.None) { ShowUI(); }
                else { HideUI(); }
            }
        }

        public void ShowUI()
        {
            logger.LogDebug("Showing UI");
            veMain.style.display = DisplayStyle.Flex;

            // TODO: Change background to item icon and set text to item name
            if (HeadItem != null)
            {
                btnHead.text = HeadItem.itemProperties.itemName;
                btnHead.style.backgroundImage = new StyleBackground(HeadItem.itemProperties.itemIcon);
            }
            else
            {
                btnHead.text = "Head";
                btnHead.style.backgroundImage = null;
            }

            if (BothArmsItem == null)
            {
                if (RightArmItem != null)
                {
                    btnRightArm.text = RightArmItem.itemProperties.itemName;
                    btnRightArm.style.backgroundImage = new StyleBackground(RightArmItem.itemProperties.itemIcon);
                }
                else
                {
                    btnRightArm.text = "Arm";
                    btnRightArm.style.backgroundImage = null;
                }

                if (LeftArmItem != null)
                {
                    btnLeftArm.text = LeftArmItem.itemProperties.itemName;
                    btnLeftArm.style.backgroundImage = new StyleBackground(LeftArmItem.itemProperties.itemIcon);
                }
                else
                {
                    btnLeftArm.text = "Arm";
                    btnLeftArm.style.backgroundImage = null;
                }
            }
            else
            {
                btnLeftArm.style.backgroundImage = new StyleBackground(BothArmsItem.itemProperties.itemIcon);
                btnLeftArm.text = BothArmsItem.itemProperties.itemName;
                btnRightArm.style.backgroundImage = new StyleBackground(BothArmsItem.itemProperties.itemIcon);
                btnRightArm.text = BothArmsItem.itemProperties.itemName;
            }

            if (ChestItem != null)
            {
                btnChest.text = ChestItem.itemProperties.itemName;
                btnChest.style.backgroundImage = new StyleBackground(ChestItem.itemProperties.itemIcon);
            }
            else
            {
                btnChest.text = "Chest";
                btnChest.style.backgroundImage = null;
            }

            if (LegsItem != null)
            {
                btnLegs.text = LegsItem.itemProperties.itemName;
                btnLegs.style.backgroundImage = new StyleBackground(LegsItem.itemProperties.itemIcon);
            }
            else
            {
                btnLegs.text = "Legs";
                btnLegs.style.backgroundImage = null;
            }

            if (FeetItem != null)
            {
                btnFeet.text = FeetItem.itemProperties.itemName;
                btnFeet.style.backgroundImage = new StyleBackground(FeetItem.itemProperties.itemIcon);
            }
            else
            {
                btnFeet.text = "Feet";
                btnFeet.style.backgroundImage = null;
            }

            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            StartOfRound.Instance.localPlayerController.disableMoveInput = true;
            StartOfRound.Instance.localPlayerController.disableInteract = true;
            StartOfRound.Instance.localPlayerController.disableLookInput = true;
        }

        public void HideUI()
        {
            logger.LogDebug("Hiding UI");
            veMain.style.display = DisplayStyle.None;

            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
            StartOfRound.Instance.localPlayerController.disableMoveInput = false;
            StartOfRound.Instance.localPlayerController.disableInteract = false;
            StartOfRound.Instance.localPlayerController.disableLookInput = false;
        }

        private void ButtonHeadClicked()
        {
            logger.LogDebug("Button Head clicked");

            if (HeadItem == null) { return; }
            HeadItem.GetComponent<WearableItem>().UnWear();
            HideUI();
        }

        private void ButtonRightArmClicked()
        {
            logger.LogDebug("Button Right Arm clicked");
            if (BothArmsItem == null)
            {
                if (RightArmItem == null) { return; }
                RightArmItem.GetComponent<WearableItem>().UnWear();
                HideUI();
            }
            else
            {
                BothArmsItem.GetComponent<WearableItem>().UnWear();
                HideUI();
            }
        }

        private void ButtonLeftArmClicked()
        {
            logger.LogDebug("Button Left Arm clicked");
            if (BothArmsItem == null)
            {
                if (LeftArmItem == null) { return; }
                LeftArmItem.GetComponent<WearableItem>().UnWear();
                HideUI();
            }
            else
            {
                BothArmsItem.GetComponent<WearableItem>().UnWear();
                HideUI();
            }
        }

        private void ButtonChestClicked()
        {
            logger.LogDebug("Button Chest clicked");

            if (ChestItem == null) { return; }
            ChestItem.GetComponent<WearableItem>().UnWear();
            HideUI();
        }

        private void ButtonLegsClicked()
        {
            logger.LogDebug("Button Legs clicked");

            if (LegsItem == null) { return; }
            LegsItem.GetComponent<WearableItem>().UnWear();
            HideUI();
        }

        private void ButtonFeetClicked()
        {
            logger.LogDebug("Button Feet clicked");

            if (FeetItem == null) { return; }
            FeetItem.GetComponent<WearableItem>().UnWear();
            HideUI();
        }
    }
}
