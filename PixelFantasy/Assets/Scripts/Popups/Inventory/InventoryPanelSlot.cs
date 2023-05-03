using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Popups.Inventory
{
    public class InventoryPanelSlot : MonoBehaviour
    {
        [SerializeField] private Image _itemIcon;
        [SerializeField] private TextMeshProUGUI _quantity;
        [SerializeField] private Image _slotBG;

        [SerializeField] private Sprite _emptyBG, _filledBG;

        private Action<InventoryPanelSlot> _onSlotClicked;

        public void Init(Action<InventoryPanelSlot> onSlotClicked = null)
        {
            _onSlotClicked = onSlotClicked;
        }

        public void SetValues(Sprite icon, int quantity)
        {
            _slotBG.sprite = _filledBG;
            _itemIcon.gameObject.SetActive(true);
            _quantity.gameObject.SetActive(true);
            
            _itemIcon.sprite = icon;
            _quantity.text = quantity.ToString();
        }

        public void Clear()
        {
            _slotBG.sprite = _emptyBG;
            _itemIcon.gameObject.SetActive(false);
            _quantity.gameObject.SetActive(false);
        }

        public void OnSlotClicked()
        {
            _onSlotClicked.Invoke(this);
        }
    }
}
