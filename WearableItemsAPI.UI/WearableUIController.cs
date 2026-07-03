using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static WearableItemsAPI.Plugin;
using static WearableItemsAPI.WearableItem;

namespace WearableItemsAPI.UI
{
    internal class WearableUIController : MonoBehaviour // TODO: Set up wiki
    {
        public static GameObject prefab = null!;
        public static WearableUIController? Instance;

        [SerializeField] GameObject radialMenuPrefab = null!;
        [SerializeField] GameObject radialMenuElementPrefab = null!;
        [SerializeField] Transform canvas = null!;
        [SerializeField] Image icon = null!;
        [SerializeField] TMP_Text iconLabel = null!;
        [SerializeField] Animator animator = null!;
        [SerializeField] Sprite[] bodyOutlineParts = null!;

        [HideInInspector] public RadialMenu? radial;

        Image? selfHighlight;
        CanvasGroup? selfHighlightCanvasGroup;

        public bool uiOpen => radial != null;
        bool openingUI;

        bool iconEnabled;

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
                logger.LogDebug("Destroying UIController");
                Instance = null;
                PlayerWearables.OnWearablesUpdate.RemoveListener(UpdateIcon);
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

            PlayerWearables.OnWearablesUpdate.AddListener(UpdateIcon);

            logger.LogDebug("UIControllerScript: Start() complete");
        }

        public void Update()
        {
            iconLabel.text = $"[{WearableItemsInputs.Instance.OpenUIKey_BindingDisplayString}]";

            if (uiOpen && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame))
                HideUI();
            
            if (WearableItemsInputs.Instance.OpenUIKey.WasPressedThisFrame() && localPlayer.CheckConditionsForEmote() && localPlayer.GetWearables().Count > 0)
            {
                if (!uiOpen) { ShowUI(); }
                else { HideUI(); }
            }
        }

        public void UpdateIcon()
        {
            int count = localPlayer.GetWearables().Count;
            logger.LogDebug("Updating icon, wearables count: " + count);
            icon.enabled = count > 0;
            iconLabel.enabled = count > 0;
            icon.color = new Color32((byte)UnityEngine.Random.Range(0, 255), (byte)UnityEngine.Random.Range(0, 255), (byte)UnityEngine.Random.Range(0, 255), 200);
            HUDManager.Instance.DisplayTip("WearableItemsAPI", $"Press {WearableItemsInputs.Instance.OpenUIKey_BindingDisplayString} to open the Wearable Items UI", false, true, "WearableItemsAPITip1");
        }

        public void ShowUI()
        {
            logger.LogDebug("Showing UI");

            localPlayer.disableMoveInput = true;
            localPlayer.disableInteract = true;
            localPlayer.disableLookInput = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;

            openingUI = true;
            animator.SetBool("open", true);
        }

        public void HideUI()
        {
            logger.LogDebug("Hiding UI");

            localPlayer.disableMoveInput = false;
            localPlayer.disableInteract = false;
            localPlayer.disableLookInput = false;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;

            openingUI = false;
            GameObject.Destroy(radial?.gameObject);
            radial = null;
            animator.SetBool("open", false);

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

            foreach (var item in localPlayer.GetWearables().ToList())
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

            HideUI();
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
