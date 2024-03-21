using System;
using Data.Item;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Build_Details.Scripts
{
    public class MaterialOptionBtn : MonoBehaviour
    {
        [SerializeField] private Image _borderImg;
        [SerializeField] private Image _bgImg;
        [SerializeField] private Image _itemIcon;
        [SerializeField] private Color _selectedBorderColour;
        [SerializeField] private Color _defaultColour;
        [SerializeField] private Color _unavailableColour;

        private string _optionGUID;
        private CraftRequirements _requirements;
        private Action<MaterialOptionBtn, string> _onPressedCallback;
        private bool _isHighlighted;
        
        private void Start()
        {
            GameEvents.OnInventoryAvailabilityChanged += GameEvent_OnInventoryChanged;
        }

        private void OnDestroy()
        {
            GameEvents.OnInventoryAvailabilityChanged -= GameEvent_OnInventoryChanged;
        }

        private void GameEvent_OnInventoryChanged()
        {
            CheckMaterialAvailable();
        }

        public void Init(string optionGUID, Sprite icon, CraftRequirements requirements,
            Action<MaterialOptionBtn, string> onPressedCallback)
        {
            _optionGUID = optionGUID;
            _requirements = requirements;
            _onPressedCallback = onPressedCallback;
            _itemIcon.sprite = icon;
        }

        public void OnPressed()
        {
            _onPressedCallback?.Invoke(this, _optionGUID);
            ShowHighlight();
        }
        
        public void ShowHighlight()
        {
            _isHighlighted = true;
            _borderImg.enabled = true;
            _borderImg.color = _selectedBorderColour;
        }

        public void RemoveHighlight()
        {
            _isHighlighted = false;
            _borderImg.enabled = true;
            _borderImg.color = _defaultColour;
            CheckMaterialAvailable();
        }
        
        private void CheckMaterialAvailable()
        {
            if (_requirements.MaterialsAreAvailable)
            {
                if (_isHighlighted)
                {
                    _borderImg.color = _selectedBorderColour;
                    _bgImg.color = _defaultColour;
                }
                else
                {
                    _borderImg.color = _defaultColour;
                    _bgImg.color = _defaultColour;
                }
            }
            else
            {
                if (_isHighlighted)
                {
                    _borderImg.color = _selectedBorderColour;
                    _bgImg.color = _unavailableColour;
                }
                else
                {
                    _borderImg.color = _unavailableColour;
                    _bgImg.color = _unavailableColour;
                }
            }
        }
    }
}
