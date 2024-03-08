using System.Collections.Generic;
using System.Linq;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    public class Storage : Furniture
    {
        public List<Item> Stored = new List<Item>();
        public List<Item> Incoming = new List<Item>();
        public List<Item> Claimed = new List<Item>();

        public Transform StoredItemParent;
        
        protected StorageSettings _storageSettings => Settings as StorageSettings;

        public bool IsGlobal
        {
            get
            {
                return true;
            }
        }
        
        public int MaxStorage => _storageSettings.MaxStorage;

        public int UsedStorage => Stored.Count;

        protected override void Awake()
        {
            // if (_storageSettings != null && Data.State == FurnitureData.EFurnitureState.Built)
            // {
            //     Init(_storageSettings);
            // }
            base.Awake();
        }
        
        protected override void Built_Enter()
        {
            InventoryManager.Instance.AddStorage(this);
            GameEvents.Trigger_RefreshInventoryDisplay();

            base.Built_Enter();
        }

        public bool IsItemValidToStore(ItemSettings itemSettings)
        {
            if (AcceptedCategories.Contains(EItemCategory.SpecificStorage))
            {
                if (_storageSettings.SpecificStorage.Contains(itemSettings))
                {
                    return true;
                }
            }
            
            return _storageSettings.AcceptedCategories.Contains(itemSettings.Category);
        }

        public List<EItemCategory> AcceptedCategories => _storageSettings.AcceptedCategories;

        public int AmountCanBeDeposited(ItemSettings itemSettings)
        {
            if (!IsItemValidToStore(itemSettings))
            {
                return 0;
            }

            int maxStorage = _storageSettings.MaxStorage;
            
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

        public void DepositItems(Item item)
        {
            if (!Incoming.Contains(item))
            {
                Debug.LogError("Tried to deposit an item that was not set as incoming");
                return;
            } 
            
            item.transform.parent = StoredItemParent;
            item.AssignedStorage = this;
            item.gameObject.SetActive(false);
            Stored.Add(item);
            Incoming.Remove(item);
            
            GameEvents.Trigger_RefreshInventoryDisplay();
        }

        /// <summary>
        /// To be used when initializing the game or loading saves
        /// </summary>
        public void ForceDepositItem(Item item)
        {
            item.transform.parent = StoredItemParent;
            item.AssignedStorage = this;
            item.gameObject.SetActive(false);
            Stored.Add(item);
            
            GameEvents.Trigger_RefreshInventoryDisplay();
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

        public void WithdrawItem(Item item)
        {
            if (!Stored.Contains(item))
            {
                Debug.LogError("Tried to withdraw an item that is not stored");
            }

            if (!Claimed.Contains(item))
            {
                Debug.LogError("Tried to withdraw an item that is not claimed");
            }
            
            item.AssignedStorage = null;
            Stored.Remove(item);
            Claimed.Remove(item);
            
            item.gameObject.SetActive(true);
            GameEvents.Trigger_RefreshInventoryDisplay();
        }
        
        public Item WithdrawItem(ItemState itemState)
        {
            Item result = null;
            foreach (var claimedItem in Claimed)
            {
                if (claimedItem.State.Equals(itemState))
                {
                    Claimed.Remove(claimedItem);
                    break;
                }
            }
            
            foreach (var storedItem in Stored)
            {
                if (storedItem.State.Equals(itemState))
                {
                    Stored.Remove(storedItem);
                    result = storedItem;
                    break;
                }
            }

            if (result != null)
            {
                result.gameObject.SetActive(true);
            }
            
            GameEvents.Trigger_RefreshInventoryDisplay();
            return result;
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
    }
}