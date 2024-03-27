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
                    && storage.RuntimeStorageData.IsItemInStorage(itemSettings))
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
                    && storage.RuntimeStorageData.IsSpecificItemInStorage(specificItem))
                {
                    return true;
                }
            }

            return false;
        }
        
        public Storage GetAvailableStorage(ItemSettings itemSettings)
        {
            foreach (var storage in _allStorage)
            {
                if(storage.IsAvailable 
                   && storage.RuntimeStorageData.AmountCanBeDeposited(itemSettings) > 0)
                {
                    return storage;
                }
            }

            return null;
        }
        
        public Storage FindAvailableStorage(ItemSettings itemSettings)
        {
            foreach (var storage in _allStorage)
            {
                if(storage.IsAvailable 
                   && storage.RuntimeStorageData.AmountCanBeDeposited(itemSettings) > 0)
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
                    var item = storage.RuntimeStorageData.GetItemDataOfType(itemSettings);
                    if (item != null)
                    {
                        return item;
                    }
                }
            }

            return null;
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
                var storedItems = storage.RuntimeStorageData.GetAllToolItems();
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
                var storedItems = storage.RuntimeStorageData.GetAllToolItems();
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
                var storedTypeList = storage.RuntimeStorageData.Stored.OfType<T>().ToList();
                var claimedTypeList = storage.RuntimeStorageData.Claimed.OfType<T>().ToList();
            
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
