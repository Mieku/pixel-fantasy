using System;
using System.Collections.Generic;
using System.Linq;
using Data.Structure;
using Data.Zones;
using Databrain;
using Databrain.Attributes;
using UnityEngine;

namespace Systems.Build_Controls.Scripts
{
    public class ZoneCategoryBtn : CategoryBtn
    {
        public DataLibrary DataLibrary;
        
        // Stockpiles
        [SerializeField] private Sprite _stockpileIcon;
        [DataObjectDropdown("DataLibrary", true)] [SerializeField] private List<StockpileZoneSettings> _stockpileOptions;
        
        // Farming
        [SerializeField] private Sprite _farmIcon;
        [DataObjectDropdown("DataLibrary")] [SerializeField] private List<FarmingZoneSettings> _farmingOptions;

        [SerializeField] private ZoneSubCategoryBtn _zoneSubCategoryPrefab;

        private List<ZoneSubCategoryBtn> _displayedOptions = new List<ZoneSubCategoryBtn>();
        private ZoneSubCategoryBtn _selectedSubCategoryBtn;

        protected override void DisplayOptions()
        {
            base.DisplayOptions();
            
            CreateOptionBtn("Stockpiles", _stockpileIcon, _stockpileOptions.Cast<ZoneSettings>().ToList(), OnSubCategorySelected);
            CreateOptionBtn("Farms", _farmIcon, _farmingOptions.Cast<ZoneSettings>().ToList(), OnSubCategorySelected);
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
        
        public void OnSubCategorySelected(ZoneSubCategoryBtn subCategoryBtnToSelect)
        {
            if (_selectedSubCategoryBtn != null) _selectedSubCategoryBtn.Cancel();
            
            _selectedSubCategoryBtn = subCategoryBtnToSelect;
            _selectedSubCategoryBtn.Selected();
        }
        
        public void UnselectSubCategoryBtn()
        {
            _selectedSubCategoryBtn = null;
        }

        private void CreateOptionBtn(string optionName, Sprite optionIcon, List<ZoneSettings> options, Action<ZoneSubCategoryBtn> onSelectedCallback)
        {
            if(options == null || options.Count == 0) return;

            foreach (var zoneOption in options)
            {
                var btn = Instantiate(_zoneSubCategoryPrefab, _optionsLayout.transform);
                btn.Init(optionName, optionIcon, zoneOption, onSelectedCallback);
                _displayedOptions.Add(btn);
            }
        }

        public void HighlightSelectedButton(bool isHighlighted)
        {
            if (_selectedSubCategoryBtn != null)
            {
                _selectedSubCategoryBtn.HighlightBtn(isHighlighted);
            }
        }
    }
}
