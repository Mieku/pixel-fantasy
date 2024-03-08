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
            if (_allStorage.Contains(storage))
            {
                Debug.LogError("Attempted to register the same storage twice");
                return;
            }
            
            _allStorage.Add(storage);
        }

        public void RemoveStorage(Storage storage)
        {
            _allStorage.Remove(storage);
        }

        /// <summary>
        /// Checks if the item is available in storage without claiming it
        /// </summary>
        public bool IsItemInStorage(ItemSettings itemSettings)
        {
            foreach (var storage in _allStorage)
            {
                if (storage.IsAvailable
                    && storage.StorageData.IsItemInStorage(itemSettings))
                {
                    return true;
                }
            }

            return false;
        }
        
        public bool IsItemInStorage(string itemName)
        {
            if (string.IsNullOrEmpty(itemName)) return false;
            
            var itemData = Librarian.Instance.GetItemData(itemName);
            
            foreach (var storage in _allStorage)
            {
                if (storage.IsAvailable 
                    && storage.StorageData.IsItemInStorage(itemData))
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
                if(storage.IsAvailable 
                   && storage.StorageData.AmountCanBeDeposited(item.GetItemData()) > 0)
                {
                    return storage;
                }
            }

            return null;
        }
        
        public Storage FindAvailableStorage(ItemSettings item)
        {
            foreach (var storage in _allStorage)
            {
                if(storage.IsAvailable 
                   && storage.StorageData.AmountCanBeDeposited(item) > 0)
                {
                    return storage;
                }
            }

            return null;
        }

        public Item ClaimItem(ItemSettings itemSettings)
        {
            foreach (var storage in _allStorage)
            {
                if (storage.IsAvailable 
                    && storage.StorageData.AmountCanBeWithdrawn(itemSettings) > 0)
                {
                    return storage.StorageData.SetClaimed(itemSettings);
                }
            }

            return null;
        }
        
        public Item FindAndClaimBestAvailableFood()
        {
            List<Item> availableFood = new List<Item>();
            foreach (var storage in _allStorage)
            {
                if (storage.IsAvailable)
                {
                    availableFood.AddRange(storage.StorageData.GetAllFoodItems(false));
                }
            }
            
            // Sort by nutrition
            var sortedFood = availableFood.OrderByDescending(food => ((IFoodItem)food.GetItemData()).FoodNutrition).ToList();

            if (sortedFood.Count == 0)
            {
                return null;
            }
            else
            {
                var selectedFood = sortedFood[0];
                var claimedFood = selectedFood.AssignedStorage.StorageData.SetClaimed(selectedFood.GetItemData());
                return claimedFood;
            }
        }

        public bool HasToolType(EToolType toolType)
        {
            foreach (var storage in _allStorage)
            {
                var storedItems = storage.StorageData.AvailableInventory;
                foreach (var kvp in storedItems)
                {
                    var tool = kvp.Key as ToolSettings;
                    if (tool != null && tool.ToolType == toolType && kvp.Value.Any())
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        public Item ClaimToolType(EToolType toolType)
        {
            List<ToolSettings> potentialItems = new List<ToolSettings>();
            foreach (var storage in _allStorage)
            {
                var storedItems = storage.StorageData.AvailableInventory;
                foreach (var kvp in storedItems)
                {
                    var tool = kvp.Key as ToolSettings;
                    if (tool != null && tool.ToolType == toolType && kvp.Value.Any())
                    {
                        potentialItems.Add(tool);
                    }
                }
            }

            // Sort by tier
            var sortedTools = potentialItems.OrderByDescending(toolData => toolData.TierLevel).ToList();
            if (sortedTools.Any())
            {
                var bestToolData = sortedTools.First();
                var claimedTool = ClaimItem(bestToolData);
                return claimedTool;
            }
            
            return null;
        }
        
        public Dictionary<ItemSettings, List<Item>> GetAvailableInventory()
        {
            Dictionary<ItemSettings, List<Item>> results = new Dictionary<ItemSettings, List<Item>>();
            foreach (var storage in _allStorage)
            {
                var contents = storage.StorageData.AvailableInventory;
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

        public Dictionary<ItemSettings, int> GetAvailableInventoryQuantities()
        {
            Dictionary<ItemSettings, int> results = new Dictionary<ItemSettings, int>();
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

        public int GetAmountAvailable(ItemSettings itemSettings)
        {
            var allAvailable = GetAvailableInventory();
            if (allAvailable.TryGetValue(itemSettings, out var value))
            {
                return value.Count;
            }
            else
            {
                return 0;
            }
        }

        public bool CanAfford(ItemSettings itemSettings, int amount)
        {
            var availableAmount = GetAmountAvailable(itemSettings);
            return amount <= availableAmount;
        }
    }
}
