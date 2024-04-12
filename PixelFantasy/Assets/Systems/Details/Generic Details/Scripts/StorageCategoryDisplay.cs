using System;
using System.Collections.Generic;
using Data.Item;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Generic_Details.Scripts
{
    public class StorageCategoryDisplay : MonoBehaviour
    {
        [SerializeField] private Image _collapseIcon;
        [SerializeField] private Sprite _collapsedSprite;
        [SerializeField] private Sprite _expandedSprite;

        [SerializeField] private TextMeshProUGUI _categoryTitle;

        [SerializeField] private GameObject _allowToggleAllHandle;
        [SerializeField] private GameObject _allowToggleSomeHandle;

        [SerializeField] private Transform _categoryContentParent;
        [SerializeField] private StorageEntryDisplay _entryDisplayPrefab;

        private bool _isExpanded;
        private EItemCategory _category;
        private List<AllowedStorageEntry> _entries;
        private Action _refreshLayoutCallback;
        private Action _onEntryChangedCallback;
        private List<StorageEntryDisplay> _displayedEntries = new List<StorageEntryDisplay>();

        public void Init(EItemCategory category, List<AllowedStorageEntry> entries, Action onEntryChangedCallback, Action refreshLayoutCallback)
        {
            _entryDisplayPrefab.gameObject.SetActive(false);
            
            _category = category;
            _entries = entries;
            _refreshLayoutCallback = refreshLayoutCallback;
            _onEntryChangedCallback = onEntryChangedCallback;

            _categoryTitle.text = _category.GetDescription();
            
            CreateOptions();
            
            RefreshAllowedToggle();
            
            CollapseCategory();
        }

        private void CreateOptions()
        {
            foreach (var entry in _entries)
            {
                var entryDisplay = Instantiate(_entryDisplayPrefab, _categoryContentParent);
                entryDisplay.gameObject.SetActive(true);
                entryDisplay.Init(entry, OnEntryChanged);
                _displayedEntries.Add(entryDisplay);
            }
        }

        private void OnEntryChanged()
        {
            RefreshAllowedToggle();
            _onEntryChangedCallback.Invoke();
        }

        private void ExpandCategory()
        {
            _isExpanded = true;
            _collapseIcon.sprite = _expandedSprite;
            
            _categoryContentParent.gameObject.SetActive(true);
            
            _refreshLayoutCallback.Invoke();
        }

        private void CollapseCategory()
        {
            _isExpanded = false;
            _collapseIcon.sprite = _collapsedSprite;
            
            _categoryContentParent.gameObject.SetActive(false);
            
            _refreshLayoutCallback.Invoke();
        }

        public void OnCollapseCategoryPressed()
        {
            if (_isExpanded)
            {
                CollapseCategory();
            }
            else
            {
                ExpandCategory();
            }
        }

        public void AllowedTogglePressed()
        {
            if (!AreAllAllowed())
            {
                // Allow All
                foreach (var entry in _entries)
                {
                    entry.IsAllowed = true;
                }
                
                RefreshEntryDisplays();
            }
            else
            {
                // Allow None
                foreach (var entry in _entries)
                {
                    entry.IsAllowed = false;
                }
                
                RefreshEntryDisplays();
            }
            
            RefreshAllowedToggle();
        }

        public void RefreshEntryDisplays()
        {
            foreach (var entry in _displayedEntries)
            {
                entry.RefreshSelectionBorder();
            }
        }

        public void RefreshAllowedToggle()
        {
            _allowToggleAllHandle.SetActive(false);
            _allowToggleSomeHandle.SetActive(false);
            
            if (AreAllAllowed())
            {
                _allowToggleAllHandle.SetActive(true);
            } else if (!AreAllNotAllowed())
            {
                _allowToggleSomeHandle.SetActive(true);
            }
        }
        
        private bool AreAllAllowed()
        {
            foreach (var entry in _entries)
            {
                if (!entry.IsAllowed)
                {
                    return false;
                }
            }

            return true;
        }

        private bool AreAllNotAllowed()
        {
            foreach (var entry in _entries)
            {
                if (entry.IsAllowed)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
