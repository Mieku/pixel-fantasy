using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using Gods;
using Items;
using UnityEngine;

namespace Controllers
{
    public class InventoryController : God<InventoryController>
    {
        private List<StorageSlot> _storageSlots = new List<StorageSlot>();
        private Dictionary<ItemData, int> _inventory = new Dictionary<ItemData, int>();
        
        [SerializeField] private GameObject _storageZonePrefab;
        [SerializeField] private Transform _storageParent;

        public Dictionary<ItemData, int> Inventory => _inventory;

        public StorageSlot GetAvailableStorageSlot()
        {
            foreach (var itemSlot in _storageSlots)
            {
                if (itemSlot.IsEmpty())
                {
                    return itemSlot;
                }
            }

            return null;
        }

        public void AddNewStorageSlot(StorageSlot newSlot)
        {
            _storageSlots.Add(newSlot);
        }

        /// <summary>
        /// Spawns an item slot in the game at the target location
        /// </summary>
        public void SpawnStorageSlot(Vector3 position)
        {
            var storageSlot = Instantiate(_storageZonePrefab, position, Quaternion.identity);
            storageSlot.transform.SetParent(_storageParent);
            var storage = storageSlot.GetComponent<StorageSlot>();
            AddNewStorageSlot(storage);
        }

        public void AddToInventory(ItemData itemData, int quantity)
        {
            if (_inventory.ContainsKey(itemData))
            {
                _inventory[itemData] += quantity;
            }
            else
            {
                _inventory.Add(itemData, quantity);
            }
            
            
            GameEvents.Trigger_OnInventoryAdded(itemData, _inventory[itemData]);
        }

        public void RemoveFromInventory(ItemData itemData, int quantity)
        {
            if (_inventory.ContainsKey(itemData))
            {
                _inventory[itemData] -= quantity;
            
                GameEvents.Trigger_OnInventoryRemoved(itemData, _inventory[itemData]);
            }
            else
            {
                Debug.LogError($"Tried removing {itemData.ItemName} from inventory, but it doesn't exist!");
            }
        }
    }
}
