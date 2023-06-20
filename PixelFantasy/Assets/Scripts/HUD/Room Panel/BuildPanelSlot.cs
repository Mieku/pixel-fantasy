using System;
using HUD.Tooltip;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HUD.Room_Panel
{
    public class BuildPanelSlot : MonoBehaviour
    {
        [SerializeField] private Image _productImg;
        [SerializeField] private TextMeshProUGUI _productName;
        [SerializeField] private TooltipTrigger _tooltipTrigger;
        [SerializeField] private Image _slotBG;
        [SerializeField] private Sprite _inactiveBGSpr;
        [SerializeField] private Sprite _activeBGSpr;

        private Action<FurnitureItemData, BuildPanelSlot> _onPressed;
        private FurnitureItemData _furnitureItemData;

        public void Init(FurnitureItemData furnitureItemData, Action<FurnitureItemData, BuildPanelSlot> onPressedCallback)
        {
            _furnitureItemData = furnitureItemData;
            _onPressed = onPressedCallback;

            _productImg.sprite = _furnitureItemData.ItemSprite;
            _productName.text = _furnitureItemData.ItemName;

            _tooltipTrigger.Content = "Not built yet!";
        }

        public void DisplayActive(bool isActive)
        {
            if (isActive)
            {
                _slotBG.sprite = _activeBGSpr;
            }
            else
            {
                _slotBG.sprite = _inactiveBGSpr;
            }
        }

        public void OnPressed()
        {
            _onPressed.Invoke(_furnitureItemData, this);
        }
    }
}
