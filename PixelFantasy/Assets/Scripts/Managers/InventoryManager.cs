using System.Collections.Generic;
using System.Linq;
using Buildings;
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
        
        public Storage FindAvailableGlobalStorage(ItemData item)
        {
            foreach (var storage in _allStorage)
            {
                if(storage.IsGlobal && storage.AmountCanBeDeposited(item) > 0)
                {
                    return storage;
                }
            }

            return null;
        }

        public ItemState ClaimItem(ItemData itemData)
        {
            foreach (var storage in _allStorage)
            {
                if (storage.AmountCanBeWithdrawn(itemData) > 0)
                {
                    return storage.SetClaimed(itemData);
                }
            }

            return null;
        }
        
        public ItemState ClaimItemGlobal(ItemData itemData)
        {
            foreach (var storage in _allStorage)
            {
                if (storage.IsGlobal && storage.AmountCanBeWithdrawn(itemData) > 0)
                {
                    return storage.SetClaimed(itemData);
                }
            }

            return null;
        }
        
        public ItemState ClaimItemBuilding(ItemData itemData, Building building)
        {
            var allBuildingStorage = building.GetBuildingStorages();
            foreach (var storage in allBuildingStorage)
            {
                if (storage.AmountCanBeWithdrawn(itemData) > 0)
                {
                    return storage.SetClaimed(itemData);
                }
            }

            return null;
        }
        
        public Dictionary<ItemData, List<ItemState>> GetAvailableInventory()
        {
            Dictionary<ItemData, List<ItemState>> results = new Dictionary<ItemData, List<ItemState>>();
            foreach (var storage in _allStorage)
            {
                var contents = storage.AvailableInventory;
                foreach (var content in contents)
                {
                    if (!results.ContainsKey(content.Key))
                    {
                        results.Add(content.Key, new List<ItemState>());
                    }

                    foreach (var item in content.Value)
                    {
                        results[content.Key].Add(item);
                    }
                }
            }

            return results;
        }

        public int GetAmountAvailable(ItemData itemData)
        {
            var allAvailable = GetAvailableInventory();
            if (allAvailable.ContainsKey(itemData))
            {
                return allAvailable[itemData].Count;
            }
            else
            {
                return 0;
            }
        }
    }
}
