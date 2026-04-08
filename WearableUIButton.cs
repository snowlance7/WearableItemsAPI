using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WearableItemsAPI
{
    public class WearableUIButton : MonoBehaviour
    {
#pragma warning disable CS8618
        [SerializeField]
        public TextMeshProUGUI Label;
        [SerializeField]
        public RawImage Icon;
#pragma warning restore CS8618

        public WearableItem? item;

        public void OnClick()
        {

        }
    }
}
