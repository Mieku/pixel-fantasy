using System.Collections.Generic;
using System.Linq;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    public class StorageData : FurnitureData
    {
        [ShowInInspector] public List<Item> Stored { get; set; } = new List<Item>();
        [ShowInInspector] public List<Item> Incoming { get; set; } = new List<Item>();
        [ShowInInspector] public List<Item> Claimed { get; set; } = new List<Item>();
        
        public StorageSettings StorageSettings => Settings as StorageSettings;

        public StorageData(FurnitureSettings settings, FurnitureVarient selectedVariant, DyeSettings selectedDyeSettings) : base(settings, selectedVariant, selectedDyeSettings)
        {
            
        }
        
        public int AmountCanBeDeposited(ItemSettings itemSettings)
        {
            if (!IsItemValidToStore(itemSettings))
            {
                return 0;
            }

            int maxStorage = StorageSettings.MaxStorage;
            
            return maxStorage - (Stored.Count + Incoming.Count);
        }
        
        public int AmountCanBeWithdrawn(ItemSettings itemSettings)
        {
            if (!IsItemValidToStore(itemSettings)) return 0;

            return NumStored(itemSettings) - NumClaimed(itemSettings);
        }
        
        private int NumStored(ItemSettings itemSettings)
        {
            int result = 0;
            foreach (var storedItem in Stored)
            {
                if (storedItem.GetItemData() == itemSettings)
                {
                    result++;
                }
            }

            return result;
        }

        private int NumClaimed(ItemSettings itemSettings)
        {
            int result = 0;
            foreach (var claimedItem in Claimed)
            {
                if (claimedItem.GetItemData() == itemSettings)
                {
                    result++;
                }
            }

            return result;
        }

        private int NumIncoming(ItemSettings itemSettings)
        {
            int result = 0;
            foreach (var incomingItem in Incoming)
            {
                if (incomingItem.GetItemData() == itemSettings)
                {
                    result++;
                }
            }

            return result;
        }
        
        public void CancelIncoming(Item item)
        {
            if (Incoming.Contains(item))
            {
                Incoming.Remove(item);
            }
        }
        
        public void SetIncoming(Item item)
        {
            if (!IsItemValidToStore(item.GetItemData()))
            {
                Debug.LogError("Attempting to store the wrong item category");
                return;
            }

            var availableSpace = AmountCanBeDeposited(item.GetItemData());
            if (availableSpace <= 0)
            {
                Debug.LogError("Attempted to set incoming with no space available");
                return;
            }
            
            Incoming.Add(item);
        }
        
        public void RestoreClaimed(Item item)
        {
            if (!Claimed.Contains(item))
            {
                Debug.LogError($"Item Claim: {item.State.UID} was not restored, was not found in claimed");
                return;
            }

            Claimed.Remove(item);
        }
        
        public Item SetClaimed(ItemSettings itemSettings)
        {
            int amountClaimable = AmountCanBeWithdrawn(itemSettings);
            if (amountClaimable <= 0)
            {
                Debug.LogError("Nothing could be withdrawn");
                return null;
            }

            foreach (var storedItem in Stored)
            {
                if (storedItem.GetItemData() == itemSettings)
                {
                    if (!Claimed.Contains(storedItem))
                    {
                        Claimed.Add(storedItem);
                        return storedItem;
                    }
                }
            }

            Debug.LogError($"Item Claim: {itemSettings.ItemName} was not set, nothing could be withdrawn");
            return null;
        }

        public bool SetClaimedItem(Item itemToClaim)
        {
            foreach (var storedItem in Stored)
            {
                if (storedItem == itemToClaim)
                {
                    if (Claimed.Contains(itemToClaim))
                    {
                        Debug.LogError($"Attempted to Claim {itemToClaim.State.UID}, but it was already claimed");
                        return false;
                    }
                    
                    Claimed.Add(itemToClaim);
                    return true;
                }
            }
            
            Debug.LogError($"Could not find {itemToClaim.State.UID} in storage");
            return false;
        }
        
        public bool CanItemBeClaimed(Item item)
        {
            if (Stored.Contains(item))
            {
                if (Claimed.Contains(item)) return false;

                return true;
            }

            return false;
        }
        
        public Dictionary<ItemSettings, List<Item>> AvailableInventory
        {
            get
            {
                Dictionary<ItemSettings, List<Item>> results = new Dictionary<ItemSettings, List<Item>>();
                foreach (var storedItem in Stored)
                {
                    if (!Claimed.Contains(storedItem))
                    {
                        if (results.ContainsKey(storedItem.GetItemData()))
                        {
                            results[storedItem.GetItemData()].Add(storedItem);
                        }
                        else
                        {
                            results.Add(storedItem.GetItemData(), new List<Item>(){storedItem});
                        }
                    }
                }

                return results;
            }
        }
        
        public bool IsItemInStorage(ItemSettings itemSettings)
        {
            if (IsItemValidToStore(itemSettings))
            {
                foreach (var storagedItem in Stored)
                {
                    if (storagedItem.GetItemData() == itemSettings && !Claimed.Contains(storagedItem))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        public List<Item> GetAllFoodItems(bool sortByBestNutrition, bool includeIncoming = false)
        {
            List<Item> foodItems = new List<Item>();
            foreach (var storedItem in Stored)
            {
                if (!Claimed.Contains(storedItem))
                {
                    if (storedItem.GetItemData() is IFoodItem)
                    {
                        foodItems.Add(storedItem);
                    }
                }
            }

            if (includeIncoming)
            {
                foreach (var incomingItem in Incoming)
                {
                    if (incomingItem.GetItemData() is IFoodItem)
                    {
                        foodItems.Add(incomingItem);
                    }
                }
            }

            if (sortByBestNutrition)
            {
                return foodItems.OrderByDescending(food => ((IFoodItem)food.GetItemData()).FoodNutrition).ToList();
            }
            
            return foodItems;
        }
        
        public bool IsItemValidToStore(ItemSettings itemSettings)
        {
            if (StorageSettings.AcceptedCategories.Contains(EItemCategory.SpecificStorage))
            {
                if (StorageSettings.SpecificStorage.Contains(itemSettings))
                {
                    return true;
                }
            }
            
            return StorageSettings.AcceptedCategories.Contains(itemSettings.Category);
        }
    }
}
