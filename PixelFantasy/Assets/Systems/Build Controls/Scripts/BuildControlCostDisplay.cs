using System;
using Managers;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Build_Controls.Scripts
{
    public class BuildControlCostDisplay : MonoBehaviour
    {
        [SerializeField] private Image _bgPanel;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _amount;
        [SerializeField] private Sprite _canAffordBG;
        [SerializeField] private Sprite _cantAffordBG;

        private ItemAmount _itemAmount;
        
        private void OnDestroy()
        {
            GameEvents.RefreshInventoryDisplay -= GameEvents_RefreshInventoryDisplay;
        }

        private void GameEvents_RefreshInventoryDisplay()
        {
            CheckAmounts();
        }

        public void Init(ItemAmount itemAmount)
        {
            _itemAmount = itemAmount;
            _icon.sprite = _itemAmount.Item.ItemSprite;
            _amount.text = _itemAmount.Quantity.ToString();
            
            GameEvents.RefreshInventoryDisplay += GameEvents_RefreshInventoryDisplay;
            
            CheckAmounts();
        }

        private void CheckAmounts()
        {
            if (InventoryManager.Instance.CanAfford(_itemAmount.Item, _itemAmount.Quantity))
            {
                _bgPanel.sprite = _canAffordBG;
            }
            else
            {
                _bgPanel.sprite = _cantAffordBG;
            }
        }
    }
}
