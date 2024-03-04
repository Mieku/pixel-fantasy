using System;
using System.Collections.Generic;
using Controllers;
using HUD.Tooltip;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Build_Controls.Scripts
{
    public class FurnitureSubCategoryBtn : MonoBehaviour
    {
        [SerializeField] private Image _btnIcon;
        [SerializeField] private TooltipTrigger _tooltip;
        [SerializeField] private Image _btnImg;
        [SerializeField] private Sprite _defaultBtn;
        [SerializeField] private Sprite _highlightedBtn;

        private Action<FurnitureSubCategoryBtn> _onSelectedCallback;
        private string _optionName;
        private List<FurnitureItemData> _options;

        public void Init(string optionName, Sprite icon, List<FurnitureItemData> options, Action<FurnitureSubCategoryBtn> onSelectedCallback)
        {
            _tooltip.Header = optionName;
            _optionName = optionName;
            _options = options;
            _btnIcon.sprite = icon;
            _onSelectedCallback = onSelectedCallback;
        }

        public void Selected()
        {
            HighlightBtn(true);
            
            HUDController.Instance.ShowBuildDetails($"{_optionName} Furniture", new List<CraftedItemData>(_options));
        }

        public void Cancel()
        {
            HighlightBtn(false);
            
            HUDController.Instance.HideDetails();
        }

        private void HighlightBtn(bool isHighlighted)
        {
            if (isHighlighted)
            {
                _btnImg.sprite = _highlightedBtn;
            }
            else
            {
                _btnImg.sprite = _defaultBtn;
            }
        }

        public void OnPressed()
        {
            _onSelectedCallback?.Invoke(this);
        }
    }
}
