using System;
using System.Collections.Generic;
using Data.Item;
using Items;
using Sirenix.OdinInspector;
using Systems.Details.Build_Details.Scripts;
using Systems.Details.Components;
using TMPro;
using UnityEngine;

namespace Systems.Details.Generic_Details.Scripts
{
    public class StorageSettingsDetails : MonoBehaviour
    {
        [BoxGroup("Parent"), SerializeField] private PanelLayoutRebuilder _layoutRebuilder;
        
        [BoxGroup("Controls"), SerializeField] private TMP_InputField _searchInput;
        [BoxGroup("Controls"), SerializeField] private RangeSlider _durabilitySlider;
        [BoxGroup("Controls"), SerializeField] private TextMeshProUGUI _durabilityDetails;
        [BoxGroup("Controls"), SerializeField] private RangeSlider _qualitySlider;
        [BoxGroup("Controls"), SerializeField] private TextMeshProUGUI _qualityDetails;
        [BoxGroup("Controls"), SerializeField] private TMP_Dropdown _priorityDropdown;

        [BoxGroup("Search"), SerializeField] private StorageSearchCategory _searchCategory;
        
        [BoxGroup("Category"), SerializeField] private StorageCategoryDisplay _categoryDisplayPrefab;
        
        private IStorage _storage;
        private StoragePlayerSettings _settings => _storage.PlayerSettings;
        private List<StorageCategoryDisplay> _displayedCategories = new List<StorageCategoryDisplay>();

        private void Awake()
        {
            _categoryDisplayPrefab.gameObject.SetActive(false);
        }

        public void Show(IStorage storage)
        {
            gameObject.SetActive(true);
            _storage = storage;
            
            // Clear the search
            _searchInput.SetTextWithoutNotify("");
            _searchCategory.HideSearch();
            
            Refresh();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _storage = null;
        }

        private void Refresh()
        {
            RefreshDurabilityDisplay();
            RefreshQualityDisplay();
            RefreshPriorityDisplay();
            SpawnDisplayStorageOptions();
        }

        #region Controls

        public void OnAllowAllPressed()
        {
            foreach (var optionKVP in _settings.StorageOptions.Options)
            {
                foreach (var entry in optionKVP.Value)
                {
                    entry.IsAllowed = true;
                }
            }

            foreach (var displayedCategory in _displayedCategories)
            {
                displayedCategory.RefreshEntryDisplays();
                displayedCategory.RefreshAllowedToggle();
            }
        }

        public void OnClearAllPressed()
        {
            foreach (var optionKVP in _settings.StorageOptions.Options)
            {
                foreach (var entry in optionKVP.Value)
                {
                    entry.IsAllowed = false;
                }
            }

            foreach (var displayedCategory in _displayedCategories)
            {
                displayedCategory.RefreshEntryDisplays();
                displayedCategory.RefreshAllowedToggle();
            }
        }

        public void OnDurabilitySliderChanged(float min, float max)
        {
            _settings.DurabilityRange.x = (int)min;
            _settings.DurabilityRange.y = (int)max;
            
            _durabilityDetails.text = $"{_settings.DurabilityMin}%  -  {_settings.DurabilityMax}%";
        }

        public void OnQualitySliderChanged(float min, float max)
        {
            _settings.QualityRange.x = (int)min;
            _settings.QualityRange.y = (int)max;
            
            _qualityDetails.text = $"{_settings.QualityMin.GetDescription()}  -  {_settings.QualityMax.GetDescription()}";
        }

        public void OnSearchCharInput(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                _searchCategory.HideSearch();
                RefreshLayout();
            }
            else
            {
                _searchCategory.UpdateSearch(value, _settings, RefreshDisplayedStorageOptions, RefreshLayout);
            }
        }

        public void OnPriorityChanged(int value)
        {
            _settings.UsePriority = (EUsePriority)value;
        }

        private void RefreshDurabilityDisplay()
        {
            _durabilitySlider.SetValues(_settings.DurabilityRange.x, _settings.DurabilityRange.y, false);
            _durabilityDetails.text = $"{_settings.DurabilityMin}%  -  {_settings.DurabilityMax}%";
        }

        private void RefreshQualityDisplay()
        {
            _qualitySlider.SetValues(_settings.QualityRange.x, _settings.QualityRange.y, false);
            _qualityDetails.text = $"{_settings.QualityMin.GetDescription()}  -  {_settings.QualityMax.GetDescription()}";
        }

        private void RefreshPriorityDisplay()
        {
            int currentPriority = (int)_settings.UsePriority;
            _priorityDropdown.SetValueWithoutNotify(currentPriority);
        }

        private void SpawnDisplayStorageOptions()
        {
            // Clear all the displayed options
            foreach (var categoryDisplay in _displayedCategories)
            {
                Destroy(categoryDisplay.gameObject);
            }
            _displayedCategories.Clear();
            
            // Create a display for each category
            var options = _settings.StorageOptions.Options;
            foreach (var optionKVP in options)
            {
                if (optionKVP.Value.Count > 0)
                {
                    StorageCategoryDisplay catDisplay = Instantiate(_categoryDisplayPrefab, transform);
                    int siblingIndex = _categoryDisplayPrefab.transform.GetSiblingIndex();
                    catDisplay.transform.SetSiblingIndex(siblingIndex);
                    catDisplay.gameObject.SetActive(true);
                    catDisplay.Init(optionKVP.Key, optionKVP.Value,RefreshDisplayedStorageOptions, RefreshLayout);
                    _displayedCategories.Add(catDisplay);
                }
            }
        }

        private void RefreshDisplayedStorageOptions()
        {
            _searchCategory.RefreshEntries();
            
            foreach (var category in _displayedCategories)
            {
                category.RefreshEntryDisplays();
                category.RefreshAllowedToggle();
            }
        }

        #endregion
        
        private void RefreshLayout()
        {
            _layoutRebuilder.RefreshLayout();
        }
    }
}
