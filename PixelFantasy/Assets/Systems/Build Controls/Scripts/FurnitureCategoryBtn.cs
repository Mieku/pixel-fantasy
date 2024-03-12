using System;
using System.Collections.Generic;
using System.Linq;
using Data.Item;
using Databrain;
using Databrain.Attributes;
using ScriptableObjects;
using UnityEngine;

namespace Systems.Build_Controls.Scripts
{
    public class FurnitureCategoryBtn : CategoryBtn
    {
        public DataLibrary DataLibrary;
        
        [SerializeField] private Sprite _storageIcon;
        [DataObjectDropdown("DataLibrary")] [SerializeField] private List<StorageData> _storageOptions;
        [SerializeField] private Sprite _decorationIcon;
        [DataObjectDropdown("DataLibrary")] [SerializeField] private List<FurnitureData> _decorationOptions;
        [SerializeField] private Sprite _productionIcon;
        [DataObjectDropdown("DataLibrary")] [SerializeField] private List<CraftingTableData> _productionOptions;
        [SerializeField] private Sprite _lightingIcon;
        [DataObjectDropdown("DataLibrary")] [SerializeField] private List<FurnitureData> _lightingOptions;
        [SerializeField] private Sprite _lifestyleIcon;
        [DataObjectDropdown("DataLibrary")] [SerializeField] private List<FurnitureData> _lifestyleOptions;

        [SerializeField] private FurnitureSubCategoryBtn _furnitureSubCategoryPrefab;

        private List<FurnitureSubCategoryBtn> _displayedOptions = new List<FurnitureSubCategoryBtn>();
        private FurnitureSubCategoryBtn _selectedSubCategoryBtn;

        protected override void DisplayOptions()
        {
            base.DisplayOptions();
            
            CreateOptionBtn("Lifestyle", _lifestyleIcon, _lifestyleOptions, OnSubCategorySelected);
            CreateOptionBtn("Production", _productionIcon, _productionOptions.Cast<FurnitureData>().ToList(), OnSubCategorySelected);
            CreateOptionBtn("Storage", _storageIcon, _storageOptions.Cast<FurnitureData>().ToList(), OnSubCategorySelected);
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

        private void CreateOptionBtn(string optionName, Sprite optionIcon, List<FurnitureData> options, Action<FurnitureSubCategoryBtn> onSelectedCallback)
        {
            if(options == null || options.Count == 0) return;

            var btn = Instantiate(_furnitureSubCategoryPrefab, _optionsLayout.transform);
            btn.Init(optionName, optionIcon, options, onSelectedCallback);
            _displayedOptions.Add(btn);
        }
    }
}
