using System;
using Data.Dye;
using Data.Item;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;
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
        
        private Action<VarientOptionBtn, FurnitureData> _onPressedCallback;
        private bool _isHighlighted;
        private CraftRequirements _craftRequirements;
        private FurnitureData _furnitureSettings;

        public bool IsColourMode;
        public bool IsDefault;
        [FormerlySerializedAs("Varient")] public FurnitureVariant Variant;
        public DyeData DyePalette;

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

        public void Init(DyeData dyeSettings, FurnitureData furnitureData, Action<VarientOptionBtn, FurnitureData> onPressedCallback)
        {
            IsDefault = false;
            IsColourMode = true;
            DyePalette = dyeSettings;
            _furnitureSettings = furnitureData;
            _onPressedCallback = onPressedCallback;

            _borderImg.enabled = false;
            _bgImg.color = dyeSettings.Primary;
            _itemIcon.enabled = false;
            
            RemoveHighlight();
        }

        public void Init(FurnitureVariant variant, FurnitureData furnitureData, Action<VarientOptionBtn, FurnitureData> onPressedCallback)
        {
            IsDefault = false;
            IsColourMode = false;
            Variant = variant;
            _furnitureSettings = furnitureData;
            _craftRequirements = variant.FurnitureData.CraftRequirements;
            _onPressedCallback = onPressedCallback;

            _borderImg.enabled = true;
            _itemIcon.sprite = variant.MaterialSelectIcon;
            
            RemoveHighlight();
        }

        public void InitDefault(FurnitureData furnitureData, Action<VarientOptionBtn, FurnitureData> onPressedCallback)
        {
            IsDefault = true;
            _furnitureSettings = furnitureData;
            _onPressedCallback = onPressedCallback;
            _craftRequirements = furnitureData.CraftRequirements;
            
            _borderImg.enabled = true;
            _itemIcon.sprite = furnitureData.CraftRequirements.MaterialCosts[0].Item.ItemSprite;
            
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
            _onPressedCallback?.Invoke(this, _furnitureSettings);
            ShowHighlight();
        }
    }
}
