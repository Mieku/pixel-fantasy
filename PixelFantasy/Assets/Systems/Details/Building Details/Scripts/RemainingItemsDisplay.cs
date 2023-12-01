using HUD.Tooltip;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Building_Details.Scripts
{
    public class RemainingItemsDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private Image _itemIcon;
        [SerializeField] private TooltipTrigger _tooltip;

        public void Init(ItemData itemData, float currentAmount, float maxAmount)
        {
            _tooltip.Header = itemData.ItemName;
            _tooltip.Content = itemData.ItemDescription;

            _itemIcon.sprite = itemData.ItemSprite;
            _amountText.text = $"{currentAmount} / {maxAmount}";
        }
    }
}
