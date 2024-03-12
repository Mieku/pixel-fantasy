using System;
using Data.Item;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Building_Details.Scripts
{
    public class ItemStockpileDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private Image _itemIcon;
        [SerializeField] private Toggle _toggle;

        private ItemData _itemSettings;
        private Action<ItemData, bool> _onAllowStockpileChanged;

        public ItemData ItemSettings => _itemSettings;

        public void Init(ItemAmount itemAmount, bool isAllowed,
            Action<ItemData, bool> onAllowStockpileChanged)
        {
            _itemSettings = itemAmount.Item;
            _onAllowStockpileChanged = onAllowStockpileChanged;

            _amountText.text = $"{itemAmount.Quantity}";
            _itemIcon.sprite = _itemSettings.ItemSprite;
            _toggle.SetIsOnWithoutNotify(isAllowed);
        }

        public void RefreshValues(int amountStored, bool isAllowed)
        {
            _amountText.text = $"{amountStored}";
            _toggle.SetIsOnWithoutNotify(isAllowed);
        }

        public void OnToggleChanged(bool value)
        {
            _onAllowStockpileChanged.Invoke(_itemSettings, value);
        }
    }
}
