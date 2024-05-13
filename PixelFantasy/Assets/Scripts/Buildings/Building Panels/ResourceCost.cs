using Data.Item;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Buildings.Building_Panels
{
    public class ResourceCost : MonoBehaviour
    {
        [SerializeField] private Image _itemImage;
        [SerializeField] private TextMeshProUGUI _quantityText;

        public ItemSettings ItemSettings { get; private set; }
        private int? _total;

        public void Init(ItemAmount itemAmount)
        {
            ItemSettings = itemAmount.Item;
            _itemImage.sprite = itemAmount.Item.icon;
            _quantityText.text = itemAmount.Quantity.ToString();
        }

        public void Init(ItemAmount itemAmount, int totalNeeded)
        {
            ItemSettings = itemAmount.Item;
            _itemImage.sprite = itemAmount.Item.icon;
            int missingAmount = totalNeeded - itemAmount.Quantity;
            _quantityText.text = $"{missingAmount}/{totalNeeded}";
            _total = totalNeeded;
        }

        public void RefreshAmount(ItemAmount itemAmount)
        {
            if (_total != null)
            {
                int missingAmount = (int)_total - itemAmount.Quantity;
                _quantityText.text = $"{missingAmount}/{_total}";
            }
            else
            {
                _quantityText.text = itemAmount.Quantity.ToString();
            }
        }
    }
}
