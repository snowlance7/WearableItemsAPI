using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;


[AddComponentMenu("Radial Menu")]
public class RadialMenu : MonoBehaviour
{
    [HideInInspector]
    public RectTransform rt = null!;

    public Transform elementsContainer = null!;

    public bool useLazySelection = true;

    public bool useSelectionFollower = true;

    public RectTransform selectionFollowerContainer = null!;

    public Text textLabel = null!;

    public List<RadialMenuElement> elements = new List<RadialMenuElement>();

    public float globalOffset = 0f;


    [HideInInspector]
    public float currentAngle = 0f;

    [HideInInspector]
    public int index = 0;

    private int elementCount => elements.Count;

    private float angleOffset;

    private int previousActiveIndex = 0;

    private PointerEventData pointer = null!;

    public void Build()
    {
        angleOffset = (360f / (float)elementCount);

        //Loop through and set up the elements.
        for (int i = 0; i < elementCount; i++)
        {
            if (elements[i] == null)
            {
                Debug.LogError("Radial Menu: element " + i.ToString() + " in the radial menu " + gameObject.name + " is null!");
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

        //rawAngle = Mathf.Atan2(Input.mousePosition.y - rt.position.y, Input.mousePosition.x - rt.position.x) * Mathf.Rad2Deg;
        rawAngle = Mathf.Atan2(mousePos.y - rt.position.y, mousePos.x - rt.position.x) * Mathf.Rad2Deg;

        currentAngle = normalizeAngle(-rawAngle + 90 - globalOffset + (angleOffset / 2f));

        if (angleOffset != 0 && useLazySelection) {

            index = (int)(currentAngle / angleOffset);

            if (elements[index] != null) {

                selectButton(index);

                /*if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Submit"))
                    ExecuteEvents.Execute(elements[index].button.gameObject, pointer, ExecuteEvents.submitHandler);*/
                if (Mouse.current.leftButton.wasPressedThisFrame)
                    ExecuteEvents.Execute(elements[index].button.gameObject, pointer, ExecuteEvents.submitHandler);
            }
        }

        if (useSelectionFollower && selectionFollowerContainer != null)
        {
            selectionFollowerContainer.rotation = Quaternion.Euler(0, 0, rawAngle + 270);
        }
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
