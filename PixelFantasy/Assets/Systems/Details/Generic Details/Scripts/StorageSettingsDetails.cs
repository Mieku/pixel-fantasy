using System;
using System.Collections.Generic;
using Data.Item;
using Items;
using Sirenix.OdinInspector;
using Systems.Details.Build_Details.Scripts;
using Systems.Details.Components;
using Systems.Notifications.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Generic_Details.Scripts
{
    public class StorageSettingsDetails : MonoBehaviour
    {
        [BoxGroup("Parent"), SerializeField] private PanelLayoutRebuilder _layoutRebuilder;

        [BoxGroup("Buttons"), SerializeField] private Image _settingsButtonImg;
        [BoxGroup("Buttons"), SerializeField] private Sprite _defaultButtonBG, _activeButtonBG;

        [BoxGroup("Controls"), SerializeField] private GameObject _controlsHandle;
        [BoxGroup("Controls"), SerializeField] private TMP_InputField _searchInput;
        [BoxGroup("Controls"), SerializeField] private RangeSlider _durabilitySlider;
        [BoxGroup("Controls"), SerializeField] private TextMeshProUGUI _durabilityDetails;
        [BoxGroup("Controls"), SerializeField] private RangeSlider _qualitySlider;
        [BoxGroup("Controls"), SerializeField] private TextMeshProUGUI _qualityDetails;
        [BoxGroup("Controls"), SerializeField] private TMP_Dropdown _priorityDropdown;
        [BoxGroup("Controls"), SerializeField] private Button _pasteBtn;
        [BoxGroup("Controls"), SerializeField] private Image _pasteIcon;
        [BoxGroup("Controls"), SerializeField] private Color _pasteActiveColour;
        [BoxGroup("Controls"), SerializeField] private Color _pasteInactiveColour;
 
        [BoxGroup("Search"), SerializeField] private StorageSearchCategory _searchCategory;
        
        [BoxGroup("Category"), SerializeField] private StorageCategoryDisplay _categoryDisplayPrefab;
        [BoxGroup("Category"), SerializeField] private Transform _categoryParent;

        [BoxGroup("Inventory"), SerializeField] private StorageInventoryDisplay _inventoryDisplay;
        [BoxGroup("Inventory"), SerializeField] private GameObject _inventoryHandle;
        
        private IStorage _storage;
        private StorageConfigs _settings => _storage.StorageConfigs;
        private List<StorageCategoryDisplay> _displayedCategories = new List<StorageCategoryDisplay>();
        private bool _showingSettings;

        private void Awake()
        {
            _categoryDisplayPrefab.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            GameEvents.OnConfigClipboardChanged += RefreshPasteButton;
        }

        private void OnDisable()
        {
            GameEvents.OnConfigClipboardChanged -= RefreshPasteButton;
        }

        public void Show(IStorage storage)
        {
            _settingsButtonImg.gameObject.SetActive(true);
            gameObject.SetActive(true);
            _storage = storage;
            
            // Clear the search
            _searchInput.SetTextWithoutNotify("");
            _searchCategory.HideSearch();
            
            RefreshView();
            Refresh();
        }

        public void ToggleShowSettings()
        {
            _showingSettings = !_showingSettings;
            
            RefreshView();
        }

        private void RefreshView()
        {
            if (_showingSettings)
            {
                _settingsButtonImg.sprite = _activeButtonBG;
            }
            else
            {
                _settingsButtonImg.sprite = _defaultButtonBG;
            }
            
            _controlsHandle.SetActive(_showingSettings);
            _inventoryHandle.SetActive(!_showingSettings);
        }

        public void Hide()
        {
            _settingsButtonImg.gameObject.SetActive(false);
            gameObject.SetActive(false);
            _storage = null;
            _inventoryDisplay.ClearDisplay();
        }

        private void Refresh()
        {
            RefreshPasteButton();
            RefreshDurabilityDisplay();
            RefreshQualityDisplay();
            RefreshPriorityDisplay();
            SpawnDisplayStorageOptions();
            
            _inventoryDisplay.RefreshDisplay(_storage);
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
        
        private Dictionary<EItemCategory, StorageCategoryDisplay> _displayedCategoriesDict = new Dictionary<EItemCategory, StorageCategoryDisplay>();
        private void SpawnDisplayStorageOptions()
        {
            // Dictionary to hold the updated categories
            Dictionary<EItemCategory, StorageCategoryDisplay> updatedCategories = new Dictionary<EItemCategory, StorageCategoryDisplay>();

            // Iterate over new options
            var options = _settings.StorageOptions.Options;
            foreach (var optionKVP in options)
            {
                if (optionKVP.Value.Count > 0 && _storage.IsCategoryAllowed(optionKVP.Key))
                {
                    // Check if the category is already displayed
                    if (_displayedCategoriesDict.TryGetValue(optionKVP.Key, out StorageCategoryDisplay existingDisplay))
                    {
                        // Update the existing display if it already exists
                        existingDisplay.Init(optionKVP.Key, optionKVP.Value, RefreshDisplayedStorageOptions, RefreshLayout);
                        updatedCategories[optionKVP.Key] = existingDisplay;
                    }
                    else
                    {
                        // Instantiate a new display if not found
                        StorageCategoryDisplay newDisplay = Instantiate(_categoryDisplayPrefab, _categoryParent);
                        newDisplay.transform.SetSiblingIndex(_categoryDisplayPrefab.transform.GetSiblingIndex());
                        newDisplay.gameObject.SetActive(true);
                        newDisplay.Init(optionKVP.Key, optionKVP.Value, RefreshDisplayedStorageOptions, RefreshLayout);
                        updatedCategories[optionKVP.Key] = newDisplay;
                    }
                }
            }

            // Destroy any old categories that are not in the new options
            foreach (var kvp in _displayedCategoriesDict)
            {
                if (!updatedCategories.ContainsKey(kvp.Key))
                {
                    Destroy(kvp.Value.gameObject);
                }
            }

            // Replace the old dictionary with the new one
            _displayedCategoriesDict = updatedCategories;
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

        public void OnCopyConfigsPressed()
        {
            ConfigClipboard.Instance.Copy(_settings);
            NotificationManager.Instance.Toast("Settings Copied to Clipboard");
        }

        public void OnPasteConfigsPressed()
        {
            if(!ConfigClipboard.Instance.HasConfig(_settings.ConfigType)) return;
            
            var pastedConfigs = ConfigClipboard.Instance.Paste(_settings.ConfigType);
            _storage.StorageConfigs.PasteConfigs(pastedConfigs);
            
            Refresh();
            
            NotificationManager.Instance.Toast("Settings Pasted");
        }

        private void RefreshPasteButton()
        {
            bool configsAvailable = ConfigClipboard.Instance.HasConfig(_settings.ConfigType);
            _pasteBtn.interactable = configsAvailable;

            if (configsAvailable)
            {
                _pasteIcon.color = _pasteActiveColour;
            }
            else
            {
                _pasteIcon.color = _pasteInactiveColour;
            }
        }

        #endregion
        
        private void RefreshLayout()
        {
            _layoutRebuilder.RefreshLayout();
        }
    }
}
