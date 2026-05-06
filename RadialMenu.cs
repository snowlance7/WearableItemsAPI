using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using static WearableItemsAPI.Plugin;
using WearableItemsAPI;

internal class RadialMenu : MonoBehaviour
{
    WearableUIController? ui => WearableUIController.Instance;
    RectTransform rt = null!;

    public Transform elementsContainer = null!;

    public Text textLabel = null!;

    public List<RadialMenuElement> elements = new List<RadialMenuElement>();

    float globalOffset = 0f;

    float currentAngle = 0f;

    int index = 0;

    int elementCount => elements.Count;

    float angleOffset;

    int previousActiveIndex = 0;

    PointerEventData pointer = null!;

    public void Rebuild()
    {
        angleOffset = elementCount > 0 ? 360f / elementCount : 0f;

        for (int i = 0; i < elementCount; i++)
        {
            var element = elements[i];

            if (element == null)
                continue;

            element.parentRM = this;
            element.assignedIndex = i;
            element.setAllAngles((angleOffset * i) + globalOffset, angleOffset);

            var rt = element.GetComponent<RectTransform>();
            rt.rotation = Quaternion.Euler(0, 0, -element.angleOffset);
        }

        previousActiveIndex = Mathf.Clamp(previousActiveIndex, 0, elementCount - 1);
        index = Mathf.Clamp(index, 0, elementCount - 1);
    }

    void Awake()
    {
        pointer = new PointerEventData(EventSystem.current);

        rt = GetComponent<RectTransform>();
    }

    void Update()
    {
        float rawAngle;
        Vector2 mousePos = Mouse.current.position.ReadValue();

        rawAngle = Mathf.Atan2(mousePos.y - rt.position.y, mousePos.x - rt.position.x) * Mathf.Rad2Deg;

        currentAngle = NormalizeAngle(-rawAngle + 90 - globalOffset + (angleOffset / 2f));

        if (angleOffset != 0 && elementCount > 0)
        {
            index = Mathf.Clamp((int)(currentAngle / angleOffset), 0, elementCount - 1);

            if (elements[index] != null)
            {
                SelectButton(index);

                if (Mouse.current.leftButton.wasPressedThisFrame)
                    ExecuteEvents.Execute(elements[index].button.gameObject, pointer, ExecuteEvents.submitHandler);
            }
        }
    }

    private void SelectButton(int i)
    {
        if (elements[i].active == false)
        {
            elements[i].highlightThisElement(pointer);

            ui?.SetBodyOutline(elements[i].item.wearableItemProperties.slot);

            if (previousActiveIndex != i)
                elements[previousActiveIndex].unHighlightThisElement(pointer);
        }

        previousActiveIndex = i;
    }

    private float NormalizeAngle(float angle)
    {
        angle = angle % 360f;

        if (angle < 0)
            angle += 360;

        return angle;
    }
}
