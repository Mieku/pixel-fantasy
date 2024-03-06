using System;
using Items;
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
        [SerializeField] private GameObject _equippingIndicator;

        private Action<ItemState, KinlingInfoOwnedItemSlot> _onSlotPressed;
        private ItemState _item;
        private bool _isEquipped;
        
        public void Init(Action<ItemState, KinlingInfoOwnedItemSlot> onSlotPressed)
        {
            _onSlotPressed = onSlotPressed;
            
            Refresh();
        }

        public void AssignItem(ItemState item, bool isEquipped = false)
        {
            _item = item;
            _isEquipped = isEquipped;
            
            Refresh();
        }

        public void ShowEquippingIndicator(bool isEquipping)
        {
            _equippingIndicator.SetActive(isEquipping);
        }

        public void TriggerSelected()
        {
            OnPressed();
        }

        private void Refresh()
        {
            if (_item?.Settings == null)
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
                _itemImage.gameObject.SetActive(true);
                _itemImage.sprite = _item.Settings.ItemSprite;
            }
        }
        
        public void OnPressed()
        {
            _onSlotPressed.Invoke(_item, this);
        }

        public void DisplaySelected(bool isSelected)
        {
            _selector.SetActive(isSelected);
        }
    }
}
