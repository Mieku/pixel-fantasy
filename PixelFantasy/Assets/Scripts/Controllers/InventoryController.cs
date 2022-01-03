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

        public StorageSlot GetAvailableStorageSlot(Item item)
        {
            // First check if there is any place that can stack
            foreach (var itemSlot in _storageSlots)
            {
                if (itemSlot.CanStack(item))
                {
                    return itemSlot;
                }
            }
            
            // Then find empty
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

        
        public Item ClaimResource(ItemData itemData)
        {
            // Can afford?
            if (_inventory.ContainsKey(itemData))
            {
                // TODO: This can be improved by comparing all the slots that have the resource's distance from the destination and choosing the closest
                foreach (var storageSlot in _storageSlots)
                {
                    if (!storageSlot.IsEmpty() && storageSlot.GetStoredType() == itemData)
                    {
                        var item = storageSlot.ClaimItem();
                        if (item != null)
                        {
                            return item;
                        }
                    }
                }
            }
            
            return null;
        }

        public void DeductClaimedResource(Item claimedResource)
        {
            foreach (var storageSlot in _storageSlots)
            {
                if (storageSlot.HasItemClaimed(claimedResource))
                {
                    storageSlot.RemoveClaimedItem(claimedResource);
                    break;
                }
            }
        }
    }
}
