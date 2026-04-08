using BepInEx.Logging;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static WearableItemsAPI.Plugin;

namespace WearableItemsAPI
{
    internal class WearableUIController : MonoBehaviour
    {
        private static ManualLogSource logger = Plugin.logger;

        /*private static WearableUIController? _instance;
        public static WearableUIController Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = new GameObject("WearableUIController");
                    _instance = obj.AddComponent<WearableUIController>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }*/

        public static WearableUIController? Instance;

        public static string OpenUIKeybind => InputControlPath.ToHumanReadableString(WearableItemsInputs.Instance.OpenUIKey.bindings[0].path, InputControlPath.HumanReadableStringOptions.OmitDevice);

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public static GameObject Prefab;

        public GameObject ButtonPrefab;
        public Animator uiAnimator;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public List<WearableUIButton> wearableUIButtons = [];

        public bool IsUIOpen {  get; private set; }

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

        public static void Init()
        {
            if (Instance != null) { return; }
            Instantiate(Prefab, localPlayer.transform);
        }

        public void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Start()
        {
            logger.LogDebug("UIController: Start()");

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            //DontDestroyOnLoad(gameObject);

            HUDManager.Instance.DisplayTip("WearableItemsAPI", $"Press {OpenUIKeybind} to open Wearable Items UI Inventory", false, true, "WearableItemsAPITip1");

            logger.LogDebug("UIControllerScript: Start() complete");
        }

        public void Update()
        {
            if (IsUIOpen && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame))
                HideUI();
            
            if (WearableItemsInputs.Instance.OpenUIKey.WasPressedThisFrame() && localPlayer.CheckConditionsForEmote())
            {
                if (!IsUIOpen) { ShowUI(); }
                else { HideUI(); }
            }

            /*if (configShowTooltip.Value)
            {
                HUDManager.Instance.ChangeControlTip(HUDManager.Instance.controlTipLines.Length - 1, $"Open Wearables UI [{OpenUIKeybind}]"); // TODO
            }*/
        }

        public void ShowUI()
        {
            logger.LogDebug("Showing UI");



            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            StartOfRound.Instance.localPlayerController.disableMoveInput = true;
            StartOfRound.Instance.localPlayerController.disableInteract = true;
            StartOfRound.Instance.localPlayerController.disableLookInput = true;
        }

        public void HideUI()
        {
            logger.LogDebug("Hiding UI");
            //veMain.style.display = DisplayStyle.None;

            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
            StartOfRound.Instance.localPlayerController.disableMoveInput = false;
            StartOfRound.Instance.localPlayerController.disableInteract = false;
            StartOfRound.Instance.localPlayerController.disableLookInput = false;
        }
    }
}
