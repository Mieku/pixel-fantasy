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
        
        private Action<FurnitureSettings, FurnitureVarientOption> _onPressedCallback;
        private FurnitureSettings _furnitureSettings;
        
        public void Init(FurnitureSettings furnitureSettings, Action<FurnitureSettings, FurnitureVarientOption> onPressedCallback)
        {
            _furnitureSettings = furnitureSettings;
            _onPressedCallback = onPressedCallback;

            _itemImage.sprite = _furnitureSettings.ItemSprite;
            DisplaySelected(false);
        }

        public void DisplaySelected(bool isSelected)
        {
            _selectionHandle.SetActive(isSelected);
        }

        public void VarientSelected()
        {
            _onPressedCallback.Invoke(_furnitureSettings, this);
        }
    }
}
