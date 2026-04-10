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
        public static WearableUIController? Instance;

        public static string OpenUIKeybind => InputControlPath.ToHumanReadableString(WearableItemsInputs.Instance.OpenUIKey.bindings[0].path, InputControlPath.HumanReadableStringOptions.OmitDevice);

#pragma warning disable CS8618
        public static GameObject prefab;
        public GameObject radialMenuPrefab;
        public GameObject radialMenuElementPrefab;
        public Animator animator;
        public Transform canvas;
#pragma warning restore CS8618

        [HideInInspector]
        public RadialMenu? ui;

        bool uiOpen => ui != null;
        bool openingUI;

        public static void Init()
        {
            if (Instance != null) { return; }
            Instantiate(prefab, localPlayer.transform);
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
            DontDestroyOnLoad(gameObject); // TODO: Test

            logger.LogDebug("UIControllerScript: Start() complete");
        }

        public void Update()
        {
            if (uiOpen && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame))
                HideUI();
            
            if (WearableItemsInputs.Instance.OpenUIKey.WasPressedThisFrame() && localPlayer.CheckConditionsForEmote())
            {
                if (!uiOpen) { ShowUI(); }
                else { HideUI(); }
            }
        }

        public void ShowUI()
        {
            logger.LogDebug("Showing UI");

            openingUI = true;
            animator.SetBool("open", true);

            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            localPlayer.disableMoveInput = true;
            localPlayer.disableInteract = true;
            localPlayer.disableLookInput = true;
        }

        public void HideUI()
        {
            logger.LogDebug("Hiding UI");

            openingUI = false;
            GameObject.Destroy(ui?.gameObject);
            ui = null;
            animator.SetBool("open", false);

            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
            localPlayer.disableMoveInput = false;
            localPlayer.disableInteract = false;
            localPlayer.disableLookInput = false;
        }

        public void BuildUI() // Animation
        {
            if (!openingUI) { return; }
            ui = Instantiate(radialMenuPrefab, canvas).GetComponent<RadialMenu>();
            foreach (var item in WearableObject.wornItems)
            {
                RadialMenuElement element = Instantiate(radialMenuElementPrefab, ui.elementsContainer).GetComponent<RadialMenuElement>();
                element.item = item;
                element.label = item.itemProperties.itemName;
                element.icon.sprite = item.wearableItemProperties.icon != null ? item.wearableItemProperties.icon : item.itemProperties.itemIcon;
                element.button.onClick.AddListener(() => OnClickElement(element));
                ui.elements.Add(element);
            }
            ui.Build();
        }

        public void OnClickElement(RadialMenuElement element)
        {
            ui!.elements.Remove(element);
            element.item.UnwearServerRpc();
            Destroy(element.gameObject);
            ui.Build();
        }
    }
}
