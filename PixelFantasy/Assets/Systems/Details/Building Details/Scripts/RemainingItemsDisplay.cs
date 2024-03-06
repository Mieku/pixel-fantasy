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

        public void Init(ItemSettings itemSettings, float currentAmount, float maxAmount)
        {
            _tooltip.Header = itemSettings.ItemName;
            _tooltip.Content = itemSettings.ItemDescription;

            _itemIcon.sprite = itemSettings.ItemSprite;
            _amountText.text = $"{currentAmount} / {maxAmount}";
        }
    }
}
