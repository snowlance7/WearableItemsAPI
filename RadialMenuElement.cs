using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WearableItemsAPI.Core;

internal class RadialMenuElement : MonoBehaviour
{
    public Button button = null!;

    public Image icon = null!;

    [HideInInspector]
    public RadialMenu parentRM = null!;

    [HideInInspector]
    public string label = null!;

    [HideInInspector]
    public WearableObject item = null!;

    [HideInInspector]
    public float angleMin, angleMax;

    [HideInInspector]
    public float angleOffset;

    [HideInInspector]
    public bool active = false;

    [HideInInspector]
    public int assignedIndex = 0;

    RectTransform rt = null!;

    void Awake()
    {
        rt = gameObject.GetComponent<RectTransform>();
    }

    void Start ()
    {
        rt.rotation = Quaternion.Euler(0, 0, -angleOffset);
    }
	
    public void setAllAngles(float offset, float baseOffset) {

        angleOffset = offset;
        angleMin = offset - (baseOffset / 2f);
        angleMax = offset + (baseOffset / 2f);
    }

    public void highlightThisElement(PointerEventData p) {

        ExecuteEvents.Execute(button.gameObject, p, ExecuteEvents.selectHandler);
        active = true;
        setParentMenuLable(label);
    }

    public void setParentMenuLable(string l) {

        if (parentRM.textLabel != null)
            parentRM.textLabel.text = l;
    }

    public void unHighlightThisElement(PointerEventData p) {

        ExecuteEvents.Execute(button.gameObject, p, ExecuteEvents.deselectHandler);
        active = false;
    }
}
