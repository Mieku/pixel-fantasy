using System.Collections.Generic;
using System.Linq;
using Databrain.Attributes;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data.Item
{
    public class StorageData : FurnitureData
    {
        // Settings
        [SerializeField] private int _maxStorage;
        [SerializeField] private List<EItemCategory> _acceptedCategories = new List<EItemCategory>();
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
                if (storedItem.Equals(itemData))
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
                if (claimedItem.Equals(itemData))
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
                if (incomingItem.Equals(itemData))
                {
                    result++;
                }
            }

            return result;
        }
        
        public void CancelIncoming(ItemData itemData)
        {
            if (IsSpecificItemDataIncoming(itemData))
            {
                Incoming.Remove(itemData);
            }
            
            GameEvents.Trigger_RefreshInventoryDisplay();
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
            GameEvents.Trigger_RefreshInventoryDisplay();
        }

        public bool IsSpecificItemDataClaimed(ItemData itemData)
        {
            var runtimeData = itemData.GetRuntimeData();
            foreach (var claimed in Claimed)
            {
                if (claimed.GetRuntimeData() == runtimeData)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsSpecificItemDataIncoming(ItemData itemData)
        {
            var runtimeData = itemData.GetRuntimeData();
            foreach (var incoming in Incoming)
            {
                if (incoming.GetRuntimeData() == runtimeData)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsSpecificItemDataStored(ItemData itemData)
        {
            var runtimeData = itemData.GetRuntimeData();
            foreach (var stored in Stored)
            {
                if (stored.GetRuntimeData() == runtimeData)
                {
                    return true;
                }
            }

            return false;
        }
        
        public void RestoreClaimed(ItemData itemData)
        {
            if (!IsSpecificItemDataClaimed(itemData))
            {
                Debug.LogError($"Item Claim: {itemData.guid} was not restored, was not found in claimed");
                return;
            }

            Claimed.Remove(itemData);
            GameEvents.Trigger_RefreshInventoryDisplay();
        }

        public ItemData GetItemDataOfType(string itemGuid)
        {
            var item = Librarian.Instance.GetInitialItemDataByGuid(itemGuid);
            int amountClaimable = AmountCanBeWithdrawn(item);
            if (amountClaimable <= 0)
            {
                return null;
            }
            
            foreach (var storedItem in Stored)
            {
                if (storedItem.initialGuid == itemGuid && !IsSpecificItemDataClaimed(storedItem))
                {
                    return storedItem.GetRuntimeData();
                }
            }
            
            return null;
        }
       
        public bool ClaimItem(ItemData itemToClaim)
        {
            foreach (var storedItem in Stored)
            {
                if (storedItem == itemToClaim)
                {
                    if (IsSpecificItemDataClaimed(itemToClaim))
                    {
                        Debug.LogError($"Attempted to Claim {itemToClaim.guid}, but it was already claimed");
                        return false;
                    }
                    
                    Claimed.Add(itemToClaim);
                    GameEvents.Trigger_RefreshInventoryDisplay();
                    return true;
                }
            }
            
            Debug.LogError($"Could not find {itemToClaim.guid} in storage");
            return false;
        }
        
        public bool CanItemBeClaimed(ItemData itemData)
        {
            if (IsSpecificItemDataStored(itemData))
            {
                if (IsSpecificItemDataClaimed(itemData)) return false;

                return true;
            }

            return false;
        }

        public List<ItemData> GetAvailableInventory()
        {
            List<ItemData> results = new List<ItemData>();
            
            foreach (var storedItem in Stored)
            {
                if (!IsSpecificItemDataClaimed(storedItem))
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
                    if (storagedItem.GetRuntimeData() == itemData.GetRuntimeData() && !IsSpecificItemDataClaimed(storagedItem))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public List<ToolData> GetAllToolItems(bool includeIncoming = false)
        {
            List<ToolData> toolItems = new List<ToolData>();
            foreach (var storedItem in Stored)
            {
                if (!IsSpecificItemDataClaimed(storedItem))
                {
                    if (storedItem is ToolData)
                    {
                        toolItems.Add(storedItem as ToolData);
                    }
                }
            }
            
            if (includeIncoming)
            {
                foreach (var incomingItem in Incoming)
                {
                    if (incomingItem is ToolData)
                    {
                        toolItems.Add(incomingItem as ToolData);
                    }
                }
            }

            return toolItems;
        }
        
        public List<IFoodItem> GetAllFoodItems(bool sortByBestNutrition, bool includeIncoming = false)
        {
            List<IFoodItem> foodItems = new List<IFoodItem>();
            foreach (var storedItem in Stored)
            {
                if (!IsSpecificItemDataClaimed(storedItem))
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
        
        public Items.Item WithdrawItem(ItemData itemData)
        {
            if (!IsSpecificItemDataStored(itemData))
            {
                Debug.LogError("Tried to withdraw an item that is not stored");
            }

            if (!IsSpecificItemDataClaimed(itemData))
            {
                Debug.LogError("Tried to withdraw an item that is not claimed");
            }
            
            Stored.Remove(itemData);
            Claimed.Remove(itemData);
            
            GameEvents.Trigger_RefreshInventoryDisplay();

            var item = itemData.CreateItemObject(Position, false);
            return item;
        }
        
        public void DepositItems(Items.Item item)
        {
            var runtimeData = item.RuntimeData.GetRuntimeData();
            
            if (!IsSpecificItemDataIncoming(runtimeData))
            {
                Debug.LogError("Tried to deposit an item that was not set as incoming");
                return;
            }

            runtimeData.AssignedStorage = (Storage)LinkedFurniture;
            
            Stored.Add(runtimeData);
            Incoming.Remove(runtimeData);
            
            GameEvents.Trigger_RefreshInventoryDisplay();
            
            Destroy(item.gameObject);
        }
        
        /// <summary>
        /// To be used when initializing the game or loading saves
        /// </summary>
        public void ForceDepositItem(ItemData itemData)
        {
            var runtimeData = itemData.GetRuntimeData();
            runtimeData.AssignedStorage = (Storage)LinkedFurniture;
            runtimeData.Position = LinkedFurniture.transform.position;
            
            Stored.Add(runtimeData);
            
            GameEvents.Trigger_RefreshInventoryDisplay();
        }
    }
}
