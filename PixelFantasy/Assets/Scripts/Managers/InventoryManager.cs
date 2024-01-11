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

        public Storage GetAvailableStorage(Item item, bool globalOnly)
        {
            foreach (var storage in _allStorage)
            {
                if(storage.FurnitureState == Furniture.EFurnitureState.Built 
                   && storage.AmountCanBeDeposited(item.GetItemData()) > 0)
                {
                    if (globalOnly && storage.IsGlobal)
                    {
                        return storage;
                    }
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

        public Item FindAndClaimBestAvailableFoodGlobal()
        {
            List<Item> availableFood = new List<Item>();
            foreach (var storage in _allStorage)
            {
                if (storage.FurnitureState == Furniture.EFurnitureState.Built
                    && storage.IsGlobal)
                {
                    availableFood.AddRange(storage.GetAllFoodItems(false));
                }
            }
            
            // Sort by nutrition
            var sortedFood = availableFood.OrderByDescending(food => (food.GetItemData() as RawFoodItemData).FoodNutrition).ToList();

            if (sortedFood.Count == 0)
            {
                return null;
            }
            else
            {
                var selectedFood = sortedFood[0];
                var claimedFood = selectedFood.AssignedStorage.SetClaimed(selectedFood.GetItemData());
                return claimedFood;
            }
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

        public Item ClaimToolTypeBuilding(EToolType toolType, Building building)
        {
            List<ToolData> potentialItems = new List<ToolData>();
            
            var allBuildingStorage = building.GetBuildingStorages();
            foreach (var storage in allBuildingStorage)
            {
                var storedItems = storage.AvailableInventory;
                foreach (var kvp in storedItems)
                {
                    var tool = kvp.Key as ToolData;
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
                var claimedTool = ClaimItemBuilding(bestToolData, building);
                return claimedTool;
            }
            
            return null;
        }

        public Item ClaimToolTypeGlobal(EToolType toolType)
        {
            List<ToolData> potentialItems = new List<ToolData>();
            foreach (var storage in _allStorage)
            {
                if (storage.IsGlobal)
                {
                    var storedItems = storage.AvailableInventory;
                    foreach (var kvp in storedItems)
                    {
                        var tool = kvp.Key as ToolData;
                        if (tool != null && tool.ToolType == toolType && kvp.Value.Any())
                        {
                            potentialItems.Add(tool);
                        }
                    }
                }
            }

            // Sort by tier
            var sortedTools = potentialItems.OrderByDescending(toolData => toolData.TierLevel).ToList();
            if (sortedTools.Any())
            {
                var bestToolData = sortedTools.First();
                var claimedTool = ClaimItemGlobal(bestToolData);
                return claimedTool;
            }
            
            return null;
        }
        
        public Dictionary<ItemData, List<Item>> GetAvailableInventory(bool globalOnly)
        {
            Dictionary<ItemData, List<Item>> results = new Dictionary<ItemData, List<Item>>();
            foreach (var storage in _allStorage)
            {
                if (storage.IsGlobal || !globalOnly)
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
            }

            return results;
        }

        public Dictionary<ItemData, int> GetAvailableInventoryQuantities(bool globalOnly)
        {
            Dictionary<ItemData, int> results = new Dictionary<ItemData, int>();
            var availableInventory = GetAvailableInventory(globalOnly);
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

        public int GetAmountAvailable(ItemData itemData, bool globalOnly)
        {
            var allAvailable = GetAvailableInventory(globalOnly);
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
            var availableAmount = GetAmountAvailable(itemData, true);
            return amount <= availableAmount;
        }
    }
}
