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
        public bool IsItemInStorage(ItemData itemData, bool globalOnly = false)
        {
            foreach (var storage in _allStorage)
            {
                if (storage.FurnitureState == Furniture.EFurnitureState.Built 
                    && (!globalOnly || storage.IsGlobal) 
                    && storage.IsItemInStorage(itemData))
                {
                    return true;
                }
            }

            return false;
        }
        
        public bool IsItemInStorage(string itemName, bool globalOnly = false)
        {
            if (string.IsNullOrEmpty(itemName)) return false;
            
            var itemData = Librarian.Instance.GetItemData(itemName);
            
            foreach (var storage in _allStorage)
            {
                if (storage.FurnitureState == Furniture.EFurnitureState.Built 
                    && (!globalOnly || storage.IsGlobal) 
                    && storage.IsItemInStorage(itemData))
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
                if(storage.FurnitureState == Furniture.EFurnitureState.Built 
                   && storage.AmountCanBeDeposited(item.GetItemData()) > 0)
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
                if(storage.FurnitureState == Furniture.EFurnitureState.Built 
                   && storage.IsGlobal 
                   && storage.AmountCanBeDeposited(item) > 0)
                {
                    return storage;
                }
            }

            return null;
        }

        public Item ClaimItem(ItemData itemData)
        {
            foreach (var storage in _allStorage)
            {
                if (storage.FurnitureState == Furniture.EFurnitureState.Built 
                    && storage.AmountCanBeWithdrawn(itemData) > 0)
                {
                    return storage.SetClaimed(itemData);
                }
            }

            return null;
        }
        
        public Item ClaimItemGlobal(ItemData itemData)
        {
            foreach (var storage in _allStorage)
            {
                if (storage.FurnitureState == Furniture.EFurnitureState.Built 
                    && storage.IsGlobal 
                    && storage.AmountCanBeWithdrawn(itemData) > 0)
                {
                    return storage.SetClaimed(itemData);
                }
            }

            return null;
        }
        
        public Item ClaimItemBuilding(ItemData itemData, Building building)
        {
            var allBuildingStorage = building.GetBuildingStorages();
            foreach (var storage in allBuildingStorage)
            {
                if (storage.FurnitureState == Furniture.EFurnitureState.Built 
                    && storage.AmountCanBeWithdrawn(itemData) > 0)
                {
                    return storage.SetClaimed(itemData);
                }
            }

            return null;
        }
        
        public Dictionary<ItemData, List<Item>> GetAvailableInventory()
        {
            Dictionary<ItemData, List<Item>> results = new Dictionary<ItemData, List<Item>>();
            foreach (var storage in _allStorage)
            {
                var contents = storage.AvailableInventory;
                foreach (var content in contents)
                {
                    if (!results.ContainsKey(content.Key))
                    {
                        results.Add(content.Key, new List<Item>());
                    }

                    foreach (var item in content.Value)
                    {
                        results[content.Key].Add(item);
                    }
                }
            }

            return results;
        }

        public Dictionary<ItemData, int> GetAvailableInventoryQuantities()
        {
            Dictionary<ItemData, int> results = new Dictionary<ItemData, int>();
            var availableInventory = GetAvailableInventory();
            foreach (var availKVP in availableInventory)
            {
                if (!results.ContainsKey(availKVP.Key))
                {
                    results.Add(availKVP.Key, availKVP.Value.Count);
                }
                else
                {
                    results[availKVP.Key] += availKVP.Value.Count;
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

        public bool CanAfford(ItemData itemData, int amount)
        {
            var availableAmount = GetAmountAvailable(itemData);
            return amount <= availableAmount;
        }
    }
}
