using System;
using System.Collections.Generic;
using Items;
using UnityEngine;

namespace Systems.Details.Generic_Details.Scripts
{
    public class DetailsEntryInventoryDisplay : MonoBehaviour
    {
        [SerializeField] private Transform _slotLayout;
        //[SerializeField] private InventoryEntrySlot _slotPrefab;

        private Storage _linkedStorage;
        //private List<InventoryEntrySlot> _displayedSlots = new List<InventoryEntrySlot>();
        
        private void Awake()
        {
            GameEvents.OnInventoryChanged += GameEvents_OnInventoryChanged;
        }

        private void OnDestroy()
        {
            GameEvents.OnInventoryChanged -= GameEvents_OnInventoryChanged;
        }

        private void GameEvents_OnInventoryChanged()
        {
            Refresh();
        }

        public void Init(Storage storage)
        {
            _linkedStorage = storage;
            
            Refresh();
        }

        private void Refresh()
        {
             ClearSlots();
            
            // var storageSlots = _linkedStorage.StorageSlots;
            // foreach (var storageSlot in storageSlots)
            // {
            //     var slot = Instantiate(_slotPrefab, _slotLayout);
            //     _displayedSlots.Add(slot);
            //     slot.Init(storageSlot);
            // }
        }

        private void ClearSlots()
        {
            // foreach (var displayedSlot in _displayedSlots)
            // {
            //     Destroy(displayedSlot.gameObject);
            // }
            // _displayedSlots.Clear();
        }
    }
}
