using System;
using Controllers;
using Data.Zones;
using HUD.Tooltip;
using Systems.Zones.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Build_Controls.Scripts
{
    public class ZoneSubCategoryBtn : MonoBehaviour
    {
        [SerializeField] private Image _btnIcon;
        [SerializeField] private TooltipTrigger _tooltip;
        [SerializeField] private Image _btnImg;
        [SerializeField] private Sprite _defaultBtn;
        [SerializeField] private Sprite _highlightedBtn;

        private Action<ZoneSubCategoryBtn> _onSelectedCallback;
        private ZoneSettings _zoneSettings;

        public void Init(string optionName, Sprite icon, ZoneSettings zoneSettings, Action<ZoneSubCategoryBtn> onSelectedCallback)
        {
            _tooltip.Header = optionName;
            _zoneSettings = zoneSettings;
            _btnIcon.sprite = icon;
            _onSelectedCallback = onSelectedCallback;
        }

        public void Selected()
        {
            HighlightBtn(true);
            
            ZoneManager.Instance.BeginPlanningZone(_zoneSettings, PlanningCompleteCallback);
        }

        private void PlanningCompleteCallback()
        {
            HighlightBtn(false);
        }

        public void Cancel()
        {
            HighlightBtn(false);
            
            HUDController.Instance.HideDetails();
        }

        public void HighlightBtn(bool isHighlighted)
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
