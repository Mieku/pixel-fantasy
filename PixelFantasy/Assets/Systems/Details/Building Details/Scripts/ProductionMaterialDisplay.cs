using HUD.Tooltip;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Building_Details.Scripts
{
    public class ProductionMaterialDisplay : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private TooltipTrigger _tooltip;

        public void Init(ItemData item, int quantity)
        {
            _icon.sprite = item.ItemSprite;
            _amountText.text = quantity.ToString();
            _tooltip.Header = item.ItemName;
            _tooltip.Content = item.ItemDescription;
        }
    }
}
