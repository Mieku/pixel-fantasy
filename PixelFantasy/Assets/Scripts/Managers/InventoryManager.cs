using System.Collections.Generic;
using System.Linq;
using Items;
using UnityEngine;

namespace Managers
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        private List<IStorage> _allStorage = new List<IStorage>();
        
        public void AddStorage(IStorage storage)
        {
            if (_allStorage.Contains(storage))
            {
                Debug.LogError("Attempted to register the same storage twice");
                return;
            }
            
            _allStorage.Add(storage);
        }

        public void RemoveStorage(IStorage storage)
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
                    && storage.IsItemInStorage(itemSettings))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsSpecificItemInStorage(ItemData specificItem)
        {
            foreach (var storage in _allStorage)
            {
                if (storage.IsAvailable
                    && storage.IsSpecificItemInStorage(specificItem))
                {
                    return true;
                }
            }

            return false;
        }

        public IStorage GetStorageByID(string storageID)
        {
            if (string.IsNullOrEmpty(storageID)) return null;
            
            return _allStorage.Find(s => s.UniqueID == storageID);
        }
        
        public IStorage GetAvailableStorage(ItemSettings itemSettings)
        {
            foreach (var storage in _allStorage)
            {
                if(storage.IsAvailable 
                   && storage.AmountCanBeDeposited(itemSettings) > 0)
                {
                    return storage;
                }
            }

            return null;
        }

        public ItemData GetItemOfType(ItemSettings itemSettings)
        {
            foreach (var storage in _allStorage)
            {
                if (storage.IsAvailable)
                {
                    var item = storage.GetItemDataOfType(itemSettings);
                    if (item != null)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        public List<ItemData> ClaimItemsOfType(ItemAmount itemAmount)
        {
            List<ItemData> results = new List<ItemData>();
            for (int i = 0; i < itemAmount.Quantity; i++)
            {
                var item = GetItemOfType(itemAmount.Item);
                if (item == null) // Just in case
                {
                    Debug.LogError("Failed to get an item");
                    foreach (var result in results)
                    {
                        result.UnclaimItem();
                    }
                    return null;
                }
                
                item.ClaimItem();
                results.Add(item);
            }

            return results;
        }

        public ItemData GetFoodItemOfType(EFoodType foodType)
        {
            var availableInventory = GetAvailableInventory();
            if (foodType == EFoodType.Meal)
            {
                foreach (var item in availableInventory)
                {
                    if (item is MealData meal)
                    {
                        return meal;
                    }
                }
            }
            else
            {
                foreach (var item in availableInventory)
                {
                    if (item is RawFoodData rawFood)
                    {
                        return rawFood;
                    }
                }
            }
            return null;
        }
        
        public bool HasToolType(EToolType toolType)
        {
            foreach (var storage in _allStorage)
            {
                var storedItems = storage.GetAllToolItems();
                foreach (var tool in storedItems)
                {
                    if (tool.ToolType == toolType)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        public ToolData ClaimToolType(EToolType toolType)
        {
            List<ToolData> potentialItems = new List<ToolData>();
            foreach (var storage in _allStorage)
            {
                var storedItems = storage.GetAllToolItems();
                foreach (var tool in storedItems)
                {
                    if (tool.ToolType == toolType)
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
                bestToolData.ClaimItem();
                return bestToolData;
            }
            
            return null;
        }

        public List<ItemData> GetAvailableInventory()
        {
            List<ItemData> results = new List<ItemData>();
            foreach (var storage in _allStorage)
            {
                results.AddRange(storage.Stored.Where(storedItem => !storage.Claimed.Contains(storedItem)));
            }

            return results;
        }
    
        public Dictionary<ItemSettings, int> GetAvailableInventoryQuantities()
        {
            Dictionary<ItemSettings, int> results = new Dictionary<ItemSettings, int>();
            var availableInventory = GetAvailableInventory();
            foreach (var item in availableInventory)
            {
                if (!results.TryAdd(item.Settings, 1))
                {
                    results[item.Settings]++;
                }
            }
            
            return results;
        }

        public int GetAmountAvailable(ItemSettings itemSettings)
        {
            var allAvailable = GetAvailableInventoryQuantities();
            return allAvailable.GetValueOrDefault(itemSettings, 0);
        }

        public bool CanAfford(ItemSettings itemSettings, int amount)
        {
            var availableAmount = GetAmountAvailable(itemSettings);
            return amount <= availableAmount;
        }

        public bool AreFoodTypesAvailable(EFoodType foodType, int amount)
        {
            var availableInventory = GetAvailableInventory();
            int amountAvailable = 0;
            if (foodType != EFoodType.Meal)
            {
                foreach (var item in availableInventory)
                {
                    var rawFood = item as RawFoodData;
                    if (rawFood != null && rawFood.RawFoodSettings.FoodType == foodType)
                    {
                        amountAvailable++;
                    }
                }
            }
            else
            {
                foreach (var item in availableInventory)
                {
                    var meal = item as MealData;
                    if (meal != null)
                    {
                        amountAvailable++;
                    }
                }
            }
            
            return amountAvailable >= amount;
        }
    }
}
