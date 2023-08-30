using System;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Buildings.Building_Panels
{
    public class FurnitureVarientOption : MonoBehaviour
    {
        [SerializeField] private Image _itemImage;
        [SerializeField] private GameObject _selectionHandle;
        
        private Action<FurnitureItemData, FurnitureVarientOption> _onPressedCallback;
        private FurnitureItemData _furnitureItemData;
        
        public void Init(FurnitureItemData furnitureItemData, Action<FurnitureItemData, FurnitureVarientOption> onPressedCallback)
        {
            _furnitureItemData = furnitureItemData;
            _onPressedCallback = onPressedCallback;

            _itemImage.sprite = _furnitureItemData.ItemSprite;
            DisplaySelected(false);
        }

        public void DisplaySelected(bool isSelected)
        {
            _selectionHandle.SetActive(isSelected);
        }

        public void VarientSelected()
        {
            _onPressedCallback.Invoke(_furnitureItemData, this);
        }
    }
}
