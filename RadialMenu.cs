using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using static WearableItemsAPI.Plugin;


[AddComponentMenu("Radial Menu")]
public class RadialMenu : MonoBehaviour
{
    [HideInInspector]
    public RectTransform rt = null!;

    public Transform elementsContainer = null!;

    public RectTransform selectionFollowerContainer = null!;

    public Text textLabel = null!;

    public List<RadialMenuElement> elements = new List<RadialMenuElement>();

    float globalOffset = 0f;

    float currentAngle = 0f;

    int index = 0;

    int elementCount => elements.Count;

    float angleOffset;

    int previousActiveIndex = 0;

    PointerEventData pointer = null!;

    public void Build()
    {
        angleOffset = (360f / (float)elementCount);

        for (int i = 0; i < elementCount; i++)
        {
            if (elements[i] == null)
            {
                logger.LogError("Radial Menu: element " + i.ToString() + " in the radial menu " + gameObject.name + " is null!");
                continue;
            }
            elements[i].parentRM = this;

            elements[i].setAllAngles((angleOffset * i) + globalOffset, angleOffset);

            elements[i].assignedIndex = i;
        }
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

        currentAngle = normalizeAngle(-rawAngle + 90 - globalOffset + (angleOffset / 2f));

        if (angleOffset != 0) 
        {
            index = (int)(currentAngle / angleOffset);

            if (elements[index] != null)
            {
                selectButton(index);

                if (Mouse.current.leftButton.wasPressedThisFrame)
                    ExecuteEvents.Execute(elements[index].button.gameObject, pointer, ExecuteEvents.submitHandler);
            }
        }

        if (selectionFollowerContainer != null)
            selectionFollowerContainer.rotation = Quaternion.Euler(0, 0, rawAngle + 270);
    }

    private void selectButton(int i)
    {
        if (elements[i].active == false)
        {
            elements[i].highlightThisElement(pointer);

            if (previousActiveIndex != i) 
                elements[previousActiveIndex].unHighlightThisElement(pointer);
        }

        previousActiveIndex = i;
    }

    private float normalizeAngle(float angle)
    {
        angle = angle % 360f;

        if (angle < 0)
            angle += 360;

        return angle;
    }
}
