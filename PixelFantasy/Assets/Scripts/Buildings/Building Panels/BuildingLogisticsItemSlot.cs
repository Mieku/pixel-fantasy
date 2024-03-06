using System;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Buildings.Building_Panels
{
    public class BuildingLogisticsItemSlot : MonoBehaviour
    {
        [SerializeField] private Image _itemImage;
        [SerializeField] private Image _selectorImage;

        private Action<ItemSettings, BuildingLogisticsItemSlot> _onSlotSelected;
        private ItemSettings _item;

        public ItemSettings AssignedItem => _item;

        public void AssignItem(ItemSettings item, Action<ItemSettings, BuildingLogisticsItemSlot> onSlotSelected)
        {
            if (item != null)
            {
                _itemImage.gameObject.SetActive(true);
                _itemImage.sprite = item.ItemSprite;
            }
            else
            {
                _itemImage.gameObject.SetActive(false);
            }

            _item = item;
            _onSlotSelected = onSlotSelected;
        }
        
        public void OnSlotPressed()
        {
            if (_onSlotSelected != null)
            {
                _onSlotSelected.Invoke(_item, this);
                DisplaySelector(true);
            }
        }

        public void DisplaySelector(bool showSelector)
        {
            _selectorImage.gameObject.SetActive(showSelector);
        }
    }
}
