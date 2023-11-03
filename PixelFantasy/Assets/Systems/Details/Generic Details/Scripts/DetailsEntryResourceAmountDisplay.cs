using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Generic_Details.Scripts
{
    public class DetailsEntryResourceAmountDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _amount;
        [SerializeField] private Image _icon;

        public void Init(ItemAmount itemAmount, bool showTilda)
        {
            string text = "";
            if (showTilda)
            {
                text = "~";
            }
            text += $"{itemAmount.Quantity}";
            _amount.text = text;
            _icon.sprite = itemAmount.Item.ItemSprite;
        }
    }
}
