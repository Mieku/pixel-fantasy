using System.Collections.Generic;
using System.Linq;
using Databrain.Attributes;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data.Item
{
    public class StorageData : FurnitureData
    {
        // Settings
        [SerializeField] private int _maxStorage;
        [SerializeField, FormerlySerializedAs("AcceptedCategories")] private List<EItemCategory> _acceptedCategories = new List<EItemCategory>();
        [SerializeField] List<ItemData> _specificStorage;
        
        // Accessors
        public int MaxStorage => _maxStorage;
        public List<EItemCategory> AcceptedCategories => _acceptedCategories;
        public List<ItemData> SpecificStorage => _specificStorage;
        
        // Runtime
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public List<ItemData> Stored = new List<ItemData>();
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public List<ItemData> Incoming = new List<ItemData>();
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public List<ItemData> Claimed = new List<ItemData>();


        public override void InitData()
        {
            base.InitData();
        }


        public int AmountCanBeDeposited(ItemData itemData)
        {
            if (!IsItemValidToStore(itemData))
            {
                return 0;
            }

            int maxStorage = MaxStorage;
            
            return maxStorage - (Stored.Count + Incoming.Count);
        }
        
        public int AmountCanBeWithdrawn(ItemData itemData)
        {
            if (!IsItemValidToStore(itemData)) return 0;

            return NumStored(itemData) - NumClaimed(itemData);
        }
        
        private int NumStored(ItemData itemData)
        {
            int result = 0;
            foreach (var storedItem in Stored)
            {
                if (storedItem == itemData)
                {
                    result++;
                }
            }

            return result;
        }

        private int NumClaimed(ItemData itemData)
        {
            int result = 0;
            foreach (var claimedItem in Claimed)
            {
                if (claimedItem == itemData)
                {
                    result++;
                }
            }

            return result;
        }

        private int NumIncoming(ItemData itemData)
        {
            int result = 0;
            foreach (var incomingItem in Incoming)
            {
                if (incomingItem == itemData)
                {
                    result++;
                }
            }

            return result;
        }
        
        public void CancelIncoming(ItemData itemData)
        {
            if (Incoming.Contains(itemData))
            {
                Incoming.Remove(itemData);
            }
        }
        
        public void SetIncoming(ItemData itemData)
        {
            if (!IsItemValidToStore(itemData))
            {
                Debug.LogError("Attempting to store the wrong item category");
                return;
            }

            var availableSpace = AmountCanBeDeposited(itemData);
            if (availableSpace <= 0)
            {
                Debug.LogError("Attempted to set incoming with no space available");
                return;
            }
            
            Incoming.Add(itemData);
        }
        
        public void RestoreClaimed(ItemData itemData)
        {
            if (!Claimed.Contains(itemData))
            {
                Debug.LogError($"Item Claim: {itemData.guid} was not restored, was not found in claimed");
                return;
            }

            Claimed.Remove(itemData);
        }
        
        public void SetClaimed(ItemData itemData)
        {
            int amountClaimable = AmountCanBeWithdrawn(itemData);
            if (amountClaimable <= 0)
            {
                Debug.LogError("Nothing could be withdrawn");
                return;
            }

            foreach (var storedItem in Stored)
            {
                if (storedItem == itemData)
                {
                    if (!Claimed.Contains(storedItem))
                    {
                        Claimed.Add(storedItem);
                        return;
                    }
                }
            }

            Debug.LogError($"Item Claim: {itemData.guid} was not set, nothing could be withdrawn");
        }

        public bool SetClaimedItem(ItemData itemToClaim)
        {
            foreach (var storedItem in Stored)
            {
                if (storedItem == itemToClaim)
                {
                    if (Claimed.Contains(itemToClaim))
                    {
                        Debug.LogError($"Attempted to Claim {itemToClaim.guid}, but it was already claimed");
                        return false;
                    }
                    
                    Claimed.Add(itemToClaim);
                    return true;
                }
            }
            
            Debug.LogError($"Could not find {itemToClaim.guid} in storage");
            return false;
        }
        
        public bool CanItemBeClaimed(ItemData itemData)
        {
            if (Stored.Contains(itemData))
            {
                if (Claimed.Contains(itemData)) return false;

                return true;
            }

            return false;
        }

        public List<T> GetAvailableInventory<T>()
        {
            List<T> results = new List<T>();
            var storedTypeList = Stored.OfType<T>().ToList();
            var claimedTypeList = Claimed.OfType<T>().ToList();
            
            foreach (var storedItem in storedTypeList)
            {
                if (!claimedTypeList.Contains(storedItem))
                {
                    results.Add(storedItem);
                }
            }
        
            return results;
        }
        
        public bool IsItemInStorage(ItemData itemData)
        {
            if (IsItemValidToStore(itemData))
            {
                foreach (var storagedItem in Stored)
                {
                    if (!Claimed.Contains(storagedItem))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        public List<IFoodItem> GetAllFoodItems(bool sortByBestNutrition, bool includeIncoming = false)
        {
            List<IFoodItem> foodItems = new List<IFoodItem>();
            foreach (var storedItem in Stored)
            {
                if (!Claimed.Contains(storedItem))
                {
                    if (storedItem is IFoodItem)
                    {
                        foodItems.Add(storedItem as IFoodItem);
                    }
                }
            }

            if (includeIncoming)
            {
                foreach (var incomingItem in Incoming)
                {
                    if (incomingItem is IFoodItem)
                    {
                        foodItems.Add(incomingItem as IFoodItem);
                    }
                }
            }

            if (sortByBestNutrition)
            {
                return foodItems.OrderByDescending(food => food.FoodNutrition).ToList();
            }
            
            return foodItems;
        }
        
        public bool IsItemValidToStore(ItemData itemData)
        {
            if (AcceptedCategories.Contains(EItemCategory.SpecificStorage))
            {
                if (SpecificStorage.Contains(itemData))
                {
                    return true;
                }
            }
            
            return AcceptedCategories.Contains(itemData.Category);
        }
        
        public void WithdrawItem(ItemData itemData)
        {
            if (!Stored.Contains(itemData))
            {
                Debug.LogError("Tried to withdraw an item that is not stored");
            }

            if (!Claimed.Contains(itemData))
            {
                Debug.LogError("Tried to withdraw an item that is not claimed");
            }
            
            Stored.Remove(itemData);
            Claimed.Remove(itemData);
            
            GameEvents.Trigger_RefreshInventoryDisplay();
        }
    }
}
