using System.Collections.Generic;
using System.Linq;
using Buildings;
using Data.Item;
using Items;
using ScriptableObjects;
using UnityEngine;
using EFoodType = Data.Item.EFoodType;
using EToolType = Data.Item.EToolType;

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
            if (foodType == EFoodType.Meal)
            {
                var meals = GetAvailableInventory<MealData>();
                return meals.FirstOrDefault();
            }
            else
            {
                var rawFoods = GetAvailableInventory<RawFoodData>();
                foreach (var rawFood in rawFoods)
                {
                    if (rawFood.RawFoodSettings.FoodType == foodType)
                    {
                        return rawFood;
                    }
                }

                return null;
            }
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
        
        public List<T> GetAvailableInventory<T>()
        {
            List<T> results = new List<T>();
            foreach (var storage in _allStorage)
            {
                var storedTypeList = storage.Stored.OfType<T>().ToList();
                var claimedTypeList = storage.Claimed.OfType<T>().ToList();
            
                foreach (var storedItem in storedTypeList)
                {
                    if (!claimedTypeList.Contains(storedItem))
                    {
                        results.Add(storedItem);
                    }
                }
            }

            return results;
        }

        public Dictionary<ItemSettings, int> GetAvailableInventoryQuantities()
        {
            Dictionary<ItemSettings, int> results = new Dictionary<ItemSettings, int>();
            var availableInventory = GetAvailableInventory<ItemData>();
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
            int amountAvailable = 0;
            if (foodType != EFoodType.Meal)
            {
                var rawFoods = GetAvailableInventory<RawFoodData>();
                foreach (var rawFood in rawFoods)
                {
                    if (rawFood.RawFoodSettings.FoodType == foodType)
                    {
                        amountAvailable++;
                    }
                }

                return amountAvailable >= amount;
            }
            else
            {
                var meals = GetAvailableInventory<MealData>();
                return meals.Count >= amount;
            }
        }
    }
}
