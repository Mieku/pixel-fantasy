using System;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Buildings.Building_Panels
{
    public class ProductOption : MonoBehaviour
    {
        [SerializeField] private Image _productImg;
        [SerializeField] private GameObject _selectorHandle;

        private CraftedItemData _craftedItemData;
        private Action<CraftedItemData, ProductOption> _onSelectedCallback;
        
        public void Init(CraftedItemData craftedItemData, Action<CraftedItemData, ProductOption> onSelectedCallback)
        {
            _craftedItemData = craftedItemData;
            _onSelectedCallback = onSelectedCallback;

            _productImg.sprite = _craftedItemData.ItemSprite;

            DisplaySelected(false);
        }

        public void OnPressed()
        {
            _onSelectedCallback.Invoke(_craftedItemData, this);
        }

        public void DisplaySelected(bool isSelected)
        {
            _selectorHandle.SetActive(isSelected);
        }
    }
}
