using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Systems.Details.Generic_Details.Scripts
{
    public class StorageSearchCategory : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private Transform _categoryContentParent;
        [SerializeField] private StorageEntryDisplay _entryDisplayPrefab;
        
        private List<StorageEntryDisplay> _displayedEntries = new List<StorageEntryDisplay>();
        private Action _onEntryChangedCallback;

        private void Awake()
        {
            _entryDisplayPrefab.gameObject.SetActive(false);
        }

        public void UpdateSearch(string searchString, StorageConfigs storageConfigs, Action onEntryChangedCallback, Action refreshLayoutCallback)
        {
            gameObject.SetActive(true);

            _title.text = $"Search: {searchString}";
            _onEntryChangedCallback = onEntryChangedCallback;

            foreach (var entry in _displayedEntries)
            {
                Destroy(entry.gameObject);
            }
            _displayedEntries.Clear();

            var searchedEntries = storageConfigs.StorageOptions.SearchByName(searchString);
            foreach (var searchedEntry in searchedEntries)
            {
                var entryDisplay = Instantiate(_entryDisplayPrefab, _categoryContentParent);
                entryDisplay.gameObject.SetActive(true);
                entryDisplay.Init(searchedEntry, EntryChanged);
                _displayedEntries.Add(entryDisplay);
            }
            
            refreshLayoutCallback.Invoke();
        }

        public void RefreshEntries()
        {
            foreach (var entry in _displayedEntries)
            {
                entry.RefreshSelectionBorder();
            }
        }

        private void EntryChanged()
        {
            _onEntryChangedCallback.Invoke();
        }

        public void HideSearch()
        {
            gameObject.SetActive(false);
        }
    }
}
