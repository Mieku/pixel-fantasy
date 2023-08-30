using System;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Buildings.Building_Panels
{
    public class BuildingFurnitureOption : MonoBehaviour
    {
        [SerializeField] private Image _itemImage;
        [SerializeField] private GameObject _selectorHandle;

        private FurnitureItemData _furniture;
        private Action<FurnitureItemData, BuildingFurnitureOption> _onPressedCallback;

        public void Init(FurnitureItemData furniture,
            Action<FurnitureItemData, BuildingFurnitureOption> onPressedCallback)
        {
            _furniture = furniture;
            _onPressedCallback = onPressedCallback;

            _itemImage.sprite = _furniture.ItemSprite;
        }
        
        public void DisplaySelector(bool isDisplayed)
        {
            _selectorHandle.SetActive(isDisplayed);
        }        
        
        public void OnOptionPressed()
        {
            _onPressedCallback.Invoke(_furniture, this);
        }
    }
}
