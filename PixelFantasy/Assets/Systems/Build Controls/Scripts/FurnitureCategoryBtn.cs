using System;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Systems.Build_Controls.Scripts
{
    public class FurnitureCategoryBtn : CategoryBtn
    {
        [SerializeField] private Sprite _storageIcon;
        [SerializeField] private List<FurnitureSettings> _storageOptions;
        [SerializeField] private Sprite _decorationIcon;
        [SerializeField] private List<FurnitureSettings> _decorationOptions;
        [SerializeField] private Sprite _productionIcon;
        [SerializeField] private List<FurnitureSettings> _productionOptions;
        [SerializeField] private Sprite _lightingIcon;
        [SerializeField] private List<FurnitureSettings> _lightingOptions;
        [SerializeField] private Sprite _lifestyleIcon;
        [SerializeField] private List<FurnitureSettings> _lifestyleOptions;

        [SerializeField] private FurnitureSubCategoryBtn _furnitureSubCategoryPrefab;

        private List<FurnitureSubCategoryBtn> _displayedOptions = new List<FurnitureSubCategoryBtn>();
        private FurnitureSubCategoryBtn _selectedSubCategoryBtn;

        protected override void DisplayOptions()
        {
            base.DisplayOptions();
            
            CreateOptionBtn("Lifestyle", _lifestyleIcon, _lifestyleOptions, OnSubCategorySelected);
            CreateOptionBtn("Production", _productionIcon, _productionOptions, OnSubCategorySelected);
            CreateOptionBtn("Storage", _storageIcon, _storageOptions, OnSubCategorySelected);
            CreateOptionBtn("Lighting", _lightingIcon, _lightingOptions, OnSubCategorySelected);
            CreateOptionBtn("Decorations", _decorationIcon, _decorationOptions, OnSubCategorySelected);
        }

        protected override void HideOptions()
        {
            base.HideOptions();
            
            foreach (var displayedOption in _displayedOptions)
            {
                Destroy(displayedOption.gameObject);
            }
            _displayedOptions.Clear();
        }
        
        public override void Cancel()
        {
            if (_selectedSubCategoryBtn != null)
            {
                _selectedSubCategoryBtn.Cancel();
                _selectedSubCategoryBtn = null;
            }
            else
            {
                ButtonDeactivated();
            }
        }
        
        public void OnSubCategorySelected(FurnitureSubCategoryBtn subCategoryBtnToSelect)
        {
            if (_selectedSubCategoryBtn != null) _selectedSubCategoryBtn.Cancel();
            
            _selectedSubCategoryBtn = subCategoryBtnToSelect;
            _selectedSubCategoryBtn.Selected();
        }
        
        public void UnselectSubCategoryBtn()
        {
            _selectedSubCategoryBtn = null;
        }

        private void CreateOptionBtn(string optionName, Sprite optionIcon, List<FurnitureSettings> options, Action<FurnitureSubCategoryBtn> onSelectedCallback)
        {
            if(options == null || options.Count == 0) return;

            var btn = Instantiate(_furnitureSubCategoryPrefab, _optionsLayout.transform);
            btn.Init(optionName, optionIcon, options, onSelectedCallback);
            _displayedOptions.Add(btn);
        }
    }
}
