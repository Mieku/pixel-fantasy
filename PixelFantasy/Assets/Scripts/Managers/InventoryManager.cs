using System.Collections.Generic;
using Items;
using ScriptableObjects;
using UnityEngine;

namespace Managers
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        private List<Storage> _allStorage = new List<Storage>();

        public Storage FindStorageByUID(string uid)
        {
            foreach (var storage in _allStorage)
            {
                if (storage.UniqueId == uid)
                {
                    return storage;
                }
            }

            Debug.LogError($"Storage with UID: {uid} can't be found");
            return null;
        }
        
        public void AddStorage(Storage storage)
        {
            _allStorage.Add(storage);
        }

        public void RemoveStorage(Storage storage)
        {
            _allStorage.Remove(storage);
        }

        /// <summary>
        /// Checks if the item is available in storage without claiming it
        /// </summary>
        public bool IsItemInStorage(ItemData itemData)
        {
            foreach (var storage in _allStorage)
            {
                if (storage.IsItemInStorage(itemData))
                {
                    return true;
                }
            }

            return false;
        }

        public Storage GetAvailableStorage(Item item)
        {
            foreach (var storage in _allStorage)
            {
                if(storage.AmountCanBeDeposited(item.GetItemData()) > 0)
                {
                    return storage;
                }
            }

            return null;
        }

        public Storage ClaimItem(ItemData itemData)
        {
            foreach (var storage in _allStorage)
            {
                if (storage.AmountCanBeWithdrawn(itemData) > 0)
                {
                    storage.SetClaimed(itemData, 1);
                    return storage;
                }
            }

            return null;
        }

        public void RestoreClaimedItems(Storage originalStorage, ItemData itemData, int quantity)
        {
            originalStorage.RestoreClaimed(itemData, quantity);
        }

        public Dictionary<ItemData, int> GetAvailableInventory()
        {
            Dictionary<ItemData, int> result = new Dictionary<ItemData, int>();
            foreach (var storage in _allStorage)
            {
                var contents = storage.AvailableInventory;
                foreach (var content in contents)
                {
                    if (result.ContainsKey(content.Key))
                    {
                        result[content.Key] += content.Value;
                    }
                    else
                    {
                        result[content.Key] = content.Value;
                    }
                }
            }

            return result;
        }

        public int GetAmountAvailable(ItemData itemData)
        {
            var allAvailable = GetAvailableInventory();
            if (allAvailable.ContainsKey(itemData))
            {
                return allAvailable[itemData];
            }
            else
            {
                return 0;
            }
        }
    }
}
