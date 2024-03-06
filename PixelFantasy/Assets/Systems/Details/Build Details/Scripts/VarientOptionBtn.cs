using System;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Build_Details.Scripts
{
    public class VarientOptionBtn : MonoBehaviour
    {
        [SerializeField] private Image _borderImg;
        [SerializeField] private Image _bgImg;
        [SerializeField] private Image _itemIcon;
        [SerializeField] private Color _selectedBorderColour;
        [SerializeField] private Color _defaultColour;
        [SerializeField] private Color _unavailableColour;
        
        private Action<VarientOptionBtn, FurnitureItemData> _onPressedCallback;
        private bool _isHighlighted;
        private CraftRequirements _craftRequirements;
        private FurnitureItemData _furnitureItemData;

        public bool IsColourMode;
        public bool IsDefault;
        public FurnitureVarient Varient;
        public DyePaletteData DyePalette;

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

        private void CheckMaterialAvailable()
        {
            if(IsColourMode) return;

            if (_craftRequirements.MaterialsAreAvailable)
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

        public void Init(DyePaletteData dyePaletteData, FurnitureItemData furnitureItemData, Action<VarientOptionBtn, FurnitureItemData> onPressedCallback)
        {
            IsDefault = false;
            IsColourMode = true;
            DyePalette = dyePaletteData;
            _furnitureItemData = furnitureItemData;
            _onPressedCallback = onPressedCallback;

            _borderImg.enabled = false;
            _bgImg.color = dyePaletteData.Primary;
            _itemIcon.enabled = false;
            
            RemoveHighlight();
        }

        public void Init(FurnitureVarient varient, FurnitureItemData furnitureItemData, Action<VarientOptionBtn, FurnitureItemData> onPressedCallback)
        {
            IsDefault = false;
            IsColourMode = false;
            Varient = varient;
            _furnitureItemData = furnitureItemData;
            _craftRequirements = varient.CraftRequirements;
            _onPressedCallback = onPressedCallback;

            _borderImg.enabled = true;
            _itemIcon.sprite = varient.MaterialSelectIcon;
            
            RemoveHighlight();
        }

        public void InitDefault(FurnitureItemData furnitureItemData, Action<VarientOptionBtn, FurnitureItemData> onPressedCallback)
        {
            IsDefault = true;
            _furnitureItemData = furnitureItemData;
            _onPressedCallback = onPressedCallback;
            _craftRequirements = furnitureItemData.CraftRequirements;
            
            _borderImg.enabled = true;
            _itemIcon.sprite = furnitureItemData.CraftRequirements.MaterialCosts[0].Item.ItemSprite;
            
            RemoveHighlight();
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
            if (IsColourMode)
            {
                _borderImg.enabled = false;
            }
            else
            {
                _borderImg.enabled = true;
                _borderImg.color = _defaultColour;
                CheckMaterialAvailable();
            }
        }

        public void OnPressed()
        {
            _onPressedCallback?.Invoke(this, _furnitureItemData);
            ShowHighlight();
        }
    }
}
