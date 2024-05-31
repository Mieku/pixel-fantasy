using Data.Item;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Generic_Details.Scripts
{
    public class InventoryItemDisplay : MonoBehaviour
    {
        [SerializeField] private Image _itemIcon;
        [SerializeField] private TextMeshProUGUI _itemNameText;
        [SerializeField] private TextMeshProUGUI _quantityText;
        [SerializeField] private TextMeshProUGUI _incomingClaimedText;

        public ItemSettings ItemSettings;

        public void Init(InventoryAmount inventoryAmount)
        {
            ItemSettings = inventoryAmount.ItemSettings;
            
            _itemIcon.sprite = inventoryAmount.ItemSettings.ItemSprite;
            _itemNameText.text = inventoryAmount.ItemSettings.ItemName;
            _quantityText.text = inventoryAmount.AmountStored + "";
            _incomingClaimedText.text = inventoryAmount.GetIncomingClaimedString(true, true);
        }
    }
}
