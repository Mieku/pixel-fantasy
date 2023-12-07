using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Building_Details.Scripts
{
    public class ItemStorageDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _quantityText;
        [SerializeField] private Image _itemIcon;

        private ItemData _itemData;
        public ItemData ItemData => _itemData;

        public void Init(ItemData itemData, int amount, int? minimum = null)
        {
            _itemData = itemData;
            _itemIcon.sprite = _itemData.ItemSprite;
            
            RefreshValues(amount, minimum);
        }

        public void RefreshValues(int amount, int? minimum = null)
        {
            if (minimum == null)
            {
                _quantityText.text = $"{amount}";
            }
            else
            {
                _quantityText.text = $"{amount} / {minimum}";
            }
        }
    }
}
