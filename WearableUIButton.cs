using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WearableItemsAPI
{
    public class WearableUIButton : MonoBehaviour
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        [SerializeField]
        public TextMeshProUGUI Label;
        [SerializeField]
        public RawImage Icon;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public WearableItem? item;

        public void OnClick()
        {

        }
    }
}
