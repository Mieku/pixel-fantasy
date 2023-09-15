using System;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Popups.Kinling_Info_Popup
{
    public class KinlingInfoOwnedItemSlot : MonoBehaviour
    {
        [SerializeField] private Image _itemImage;
        [SerializeField] private TextMeshProUGUI _quantityText;
        [SerializeField] private GameObject _selector;

        private Action<ItemData, KinlingInfoOwnedItemSlot> _onSlotPressed;
        private ItemData _itemData;
        private int _quantity;
        private bool _isEquipped;
        
        public void Init(Action<ItemData, KinlingInfoOwnedItemSlot> onSlotPressed)
        {
            _onSlotPressed = onSlotPressed;
            
            Refresh();
        }

        public void AssignItem(ItemData itemData, int quantity, bool isEquipped = false)
        {
            _itemData = itemData;
            _quantity = quantity;
            _isEquipped = isEquipped;
            
            Refresh();
        }

        public void TriggerSelected()
        {
            OnPressed();
        }

        private void Refresh()
        {
            if (_itemData == null)
            {
                _quantityText.text = "";
                _itemImage.gameObject.SetActive(false);
            }
            else
            {
                if (_isEquipped)
                {
                    _quantityText.text = "E";
                }
                else
                {
                    _quantityText.text = _quantity.ToString();
                }
                _itemImage.gameObject.SetActive(true);
                _itemImage.sprite = _itemData.ItemSprite;
            }
        }
        
        public void OnPressed()
        {
            _onSlotPressed.Invoke(_itemData, this);
        }

        public void DisplaySelected(bool isSelected)
        {
            _selector.SetActive(isSelected);
        }
    }
}
