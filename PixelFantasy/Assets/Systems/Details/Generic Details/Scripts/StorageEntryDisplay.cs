using System;
using Data.Item;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Generic_Details.Scripts
{
    public class StorageEntryDisplay : MonoBehaviour
    {
        [SerializeField] private Image _itemIcon;
        [SerializeField] private TextMeshProUGUI _itemName;
        [SerializeField] private GameObject _selectionBorder;

        private AllowedStorageEntry _entry;
        private Action _entryChangedCallback;

        public void Init(AllowedStorageEntry entry, Action entryChangedCallback)
        {
            _entry = entry;
            _entryChangedCallback = entryChangedCallback;
            _itemIcon.sprite = _entry.Item.ItemSprite;
            _itemName.text = _entry.Item.ItemName;
            
            RefreshSelectionBorder();
        }

        public void RefreshSelectionBorder()
        {
            _selectionBorder.SetActive(_entry.IsAllowed);
        }

        public void OnPressed()
        {
            _entry.IsAllowed = !_entry.IsAllowed;
            RefreshSelectionBorder();
            _entryChangedCallback.Invoke();
        }
    }
}
