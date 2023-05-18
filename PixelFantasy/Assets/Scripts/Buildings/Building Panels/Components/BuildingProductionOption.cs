using System;
using HUD.Tooltip;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Buildings.Building_Panels.Components
{
    public class BuildingProductionOption : MonoBehaviour
    {
        [SerializeField] private Image _bg;
        [SerializeField] private Image _productIcon;
        [SerializeField] private TooltipTrigger _tooltip;

        private Action<CraftedItemData> _onPressed;
        private CraftedItemData _itemData;

        public void Init(CraftedItemData itemData, Action<CraftedItemData> onPressed)
        {
            _itemData = itemData;
            _onPressed = onPressed;

            _productIcon.sprite = _itemData.ItemSprite;
            _tooltip.Header = _itemData.ItemName;
        }

        public void OnPressed()
        {
            _onPressed.Invoke(_itemData);
        }
    }
}
