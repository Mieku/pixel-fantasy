using System;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Building_Details.Scripts
{
    public class StorageItemOption : MonoBehaviour
    {
        [SerializeField] private Image _itemIcon;
        [SerializeField] private GameObject _selectedHandle;
        [SerializeField] private GameObject _ruleIcon;

        private ItemData _itemData;
        private Action<ItemData> _onPressedCallback;
        private bool _isSelected;

        public ItemData ItemData => _itemData;
        
        public void Init(ItemData itemData, bool isSelected, bool hasRule, Action<ItemData> onPressedCallback)
        {
            _itemData = itemData;
            _onPressedCallback = onPressedCallback;

            _itemIcon.sprite = _itemData.ItemSprite;
            SetSelected(isSelected);
            SetHasRule(hasRule);
        }

        public void SetSelected(bool isSelected)
        {
            _isSelected = isSelected;
            _selectedHandle.SetActive(_isSelected);
        }

        public void SetHasRule(bool hasRule)
        {
            _ruleIcon.SetActive(hasRule);
        }
        
        public void OnPressed()
        {
            _onPressedCallback.Invoke(_itemData);
        }
    }
}
