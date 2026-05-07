using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static WearableItemsAPI.Plugin;
using static WearableItemsAPI.WearableItem;

namespace WearableItemsAPI
{
    internal class WearableUIController : MonoBehaviour
    {
        public static GameObject prefab = null!;
        public static WearableUIController? Instance;

        [SerializeField] GameObject radialMenuPrefab = null!;
        [SerializeField] GameObject radialMenuElementPrefab = null!;
        [SerializeField] Transform canvas = null!;
        [SerializeField] TMP_Text iconLabel = null!;
        [SerializeField] Animator animator = null!;
        [SerializeField] Sprite[] bodyOutlineParts = null!;

        [HideInInspector] public RadialMenu? radial;

        Image? selfHighlight;
        CanvasGroup? selfHighlightCanvasGroup;

        public bool uiOpen => radial != null;
        bool openingUI;

        public static void Init()
        {
            if (Instance != null) { return; }
            if (prefab == null) { logger.LogError("Could not instantiate UI"); return; }
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

            var selfRedObj = HUDManager.Instance.selfRedCanvasGroup?.gameObject;
            if (selfRedObj != null)
            {
                GameObject selfHighlightObj = Instantiate(selfRedObj, selfRedObj.transform.parent);
                selfHighlightObj.name = "WISelfHighlight";
                selfHighlight = selfHighlightObj.GetComponent<Image>();
                selfHighlightCanvasGroup = selfHighlightObj.GetComponent<CanvasGroup>();

                selfHighlightCanvasGroup.alpha = 0;
                selfHighlight.sprite = null;
                selfHighlight.color = Color.white;
            }

            logger.LogDebug("UIControllerScript: Start() complete");
        }

        public void Update()
        {
            PlayerWearables.UpdateWearables();

            iconLabel.text = $"[{WearableItemsInputs.OpenUIKeybind}]";

            if (uiOpen && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame))
                HideUI();
            
            if (WearableItemsInputs.Instance.OpenUIKey.WasPressedThisFrame() && localPlayer.CheckConditionsForEmote() && localPlayer.GetWornItems().Count > 0)
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
            GameObject.Destroy(radial?.gameObject);
            radial = null;
            animator.SetBool("open", false);

            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
            localPlayer.disableMoveInput = false;
            localPlayer.disableInteract = false;
            localPlayer.disableLookInput = false;
            SetBodyOutline(WearableSlot.None);
        }

        public void BuildUI() // Animation
        {
            if (!openingUI) { return; }

            if (radial != null)
            {
                GameObject.Destroy(radial.gameObject);
                radial = null;
            }

            radial = Instantiate(radialMenuPrefab, canvas).GetComponent<RadialMenu>();

            foreach (var item in localPlayer.GetWornItems().ToList())
            {
                RadialMenuElement element = Instantiate(radialMenuElementPrefab, radial.elementsContainer).GetComponent<RadialMenuElement>();
                element.item = item;
                element.label = item.itemProperties.itemName;
                element.icon.sprite = item.wearableItemProperties.icon != null ? item.wearableItemProperties.icon : item.itemProperties.itemIcon;
                element.button.onClick.AddListener(() => OnClickElement(element));
                radial.elements.Add(element);
            }

            radial.Rebuild();
        }

        public void OnClickElement(RadialMenuElement element)
        {
            element.item.UnWearItem();

            radial!.elements.Remove(element);
            Destroy(element.gameObject);

            if (radial.elements.Count <= 0 || configCloseUIOnUnwear.Value)
            {
                HideUI();
                return;
            }

            radial.Rebuild();
        }

        public void SetBodyOutline(WearableSlot slot)
        {
            if (selfHighlight == null || selfHighlightCanvasGroup == null) { return; }

            int index = (int)slot;
            selfHighlight.sprite = index >= 0 ? bodyOutlineParts[index] : null;
            selfHighlightCanvasGroup.alpha = index >= 0 ? 1 : 0;
        }
    }
}
