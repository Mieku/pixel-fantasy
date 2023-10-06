using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Popups.Inventory;
using ScriptableObjects;
using Systems.SmartObjects.Scripts;
using UnityEngine;

namespace Items
{
    public class Storage : Furniture
    {
        [SerializeField] protected InventoryPanel _inventoryPanel;

        [SerializeField] private List<StorageSlot> _storageSlots = new List<StorageSlot>();

        public Transform StoredItemParent;
        
        protected StorageItemData _storageItemData => _furnitureItemData as StorageItemData;

        public bool IsGlobal => _parentBuilding == null;

        protected override void Awake()
        {
            if (_storageItemData != null && IsBuilt)
            {
                Init(_storageItemData);
            }
            base.Awake();
        }
        
        protected override void CompletePlacement()
        {
            for (int i = 0; i < _storageItemData.NumSlots; i++)
            {
                _storageSlots.Add(new StorageSlot());
            }
            
            InventoryManager.Instance.AddStorage(this);
            GameEvents.Trigger_RefreshInventoryDisplay();
            RefreshDisplayedInventoryPanel();
            
            base.CompletePlacement();
        }

        public int AmountCanBeDeposited(ItemData itemData)
        {
            int result = 0;
            foreach (var slot in _storageSlots)
            {
                result += slot.AmountCanBeDeposited(itemData);
            }
            return result;
        }

        public int AmountCanBeWithdrawn(ItemData itemData)
        {
            int result = 0;
            foreach (var slot in _storageSlots)
            {
                result += slot.AmountCanBeWithdrawn(itemData);
            }
            return result;
        }
        
        public void SetIncoming(Item item)
        {
            foreach (var slot in _storageSlots)
            {
                int amountSpaceAvailable = slot.AmountCanBeDeposited(item.GetItemData());
                if (amountSpaceAvailable != 0)
                {
                    slot.SetIncoming(item);
                    return;
                }
            }
        }

        public void DepositItems(Item item)
        {
            foreach (var slot in _storageSlots)
            {
                if(slot.StoredItemData() != item.GetItemData()) continue;
                if (slot.Incoming.Contains(item))
                {
                    slot.AddItem(item, this);
                    GameEvents.Trigger_RefreshInventoryDisplay();
                    RefreshDisplayedInventoryPanel();
                    return;
                }
            }
            
            Debug.LogError($"Item: {item.State.UID} was not deposited, was not found as incoming");
        }

        public void RestoreClaimed(Item item)
        {
            foreach (var slot in _storageSlots)
            {
                if (slot.Claimed.Contains(item))
                {
                    slot.Claimed.Remove(item);
                    return;
                }
            }

            Debug.LogError($"Item Claim: {item.State.UID} was not restored, was not found in claimed");
        }
        
        public void RestoreClaimed(ItemState itemState)
        {
            foreach (var slot in _storageSlots)
            {
                foreach (var claimedItem in slot.Claimed)
                {
                    if (claimedItem.State.Equals(itemState))
                    {
                        slot.Claimed.Remove(claimedItem);
                        return;
                    }
                }
            }

            Debug.LogError($"Item Claim: {itemState.UID} was not restored, was not found in claimed");
        }
        
        public Item SetClaimed(ItemData itemData)
        {
            foreach (var slot in _storageSlots)
            {
                int amountClaimable = slot.AmountCanBeWithdrawn(itemData);
                if (amountClaimable > 0)
                {
                    var claimedItemState = slot.ClaimItem();
                    return claimedItemState;
                }
            }
            
            Debug.LogError($"Item Claim: {itemData.ItemName} was not set, nothing could be withdrawn");
            return null;
        }

        public bool SetClaimedItem(Item itemToClaim)
        {
            foreach (var slot in _storageSlots)
            {
                if (slot.Stored.Contains(itemToClaim))
                {
                    if (slot.ClaimItemState(itemToClaim))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool CanItemBeClaimed(Item item)
        {
            foreach (var slot in _storageSlots)
            {
                if (slot.Stored.Contains(item))
                {
                    if (slot.Claimed.Contains(item)) return false;

                    return true;
                }
            }

            return false;
        }

        public void WithdrawItem(Item item)
        {
            foreach (var slot in _storageSlots)
            {
                if(slot.StoredItemData() != item.GetItemData()) continue;
                
                slot.RemoveItem(item);
                item.gameObject.SetActive(true);
                GameEvents.Trigger_RefreshInventoryDisplay();
                RefreshDisplayedInventoryPanel();
                return;
            }
            
            Debug.LogError($"Item Withdrawl: {item.State.UID} was not set, could not find the requested item");
        }
        
        public Item WithdrawItem(ItemState itemState)
        {
            foreach (var slot in _storageSlots)
            {
                if(slot.StoredItemData() != itemState.Data) continue;
                
                var item = slot.RemoveItem(itemState);
                GameEvents.Trigger_RefreshInventoryDisplay();
                RefreshDisplayedInventoryPanel();
                item.gameObject.SetActive(true);
                return item;
            }
            
            Debug.LogError($"Item Withdrawl: {itemState.UID} was not set, could not find the requested item");
            return null;
        }

        public Dictionary<ItemData, List<Item>> AvailableInventory
        {
            get
            {
                Dictionary<ItemData, List<Item>> results = new Dictionary<ItemData, List<Item>>();
                foreach (var slot in _storageSlots)
                {
                    if (slot != null && !slot.IsEmpty())
                    {
                        foreach (var storedItem in slot.Stored)
                        {
                            if (!slot.Claimed.Contains(storedItem))
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
                    }
                }

                return results;
            }
        }

        protected void RefreshDisplayedInventoryPanel()
        {
            if (_inventoryPanel.IsOpen)
            {
                _inventoryPanel.UpdateDisplayedInventory(_storageSlots);
            }
        }
        
        protected override void OnClicked()
        {
            _inventoryPanel.Init(_storageItemData, _storageSlots, this);
        }

        public bool IsItemInStorage(ItemData itemData)
        {
            foreach (var slot in _storageSlots)
            {
                if(slot.StoredItemData() != itemData) continue;

                return slot.NumAvailable > 0;
            }

            return false;
        }

        public List<Item> GetAllFoodItems()
        {
            List<Item> foodItems = new List<Item>();
            foreach (var slot in _storageSlots)
            {
                var foodItemData = slot.StoredItemData() as RawFoodItemData;
                if (foodItemData != null && slot.NumAvailable > 0)
                {
                    var slotFoodItems = slot.UnclaimedStored;
                    foreach (var slotFoodItem in slotFoodItems)
                    {
                        foodItems.Add(slotFoodItem);
                    }
                }
            }

            return foodItems;
        }
    }

    [Serializable]
    public class StorageSlot
    {
        public List<Item> Stored = new List<Item>();
        public List<Item> Incoming = new List<Item>();
        public List<Item> Claimed = new List<Item>();

        public List<Item> UnclaimedStored
        {
            get
            {
                List<Item> results = new List<Item>();
                foreach (var storedItem in Stored)
                {
                    if (!Claimed.Contains(storedItem))
                    {
                        results.Add(storedItem);
                    }
                }

                return results;
            }
        }

        public int NumStored => Stored.Count;
        public int NumIncoming => Incoming.Count;
        public int NumClaimed => Claimed.Count;

        public int NumAvailable => Stored.Count - Claimed.Count;

        /// <summary>
        /// Inform the Slot that an item is on its way
        /// </summary>
        public void SetIncoming(Item item)
        {
            Incoming.Add(item);
        }
        
        /// <summary>
        /// Add the item to the storage
        /// </summary>
        public void AddItem(Item item, Storage storage)
        {
            item.transform.parent = storage.StoredItemParent;
            item.AssignedStorage = storage;
            item.gameObject.SetActive(false);
            Stored.Add(item);
            Incoming.Remove(item);
        }

        public Item ClaimItem()
        {
            foreach (var storedItem in Stored)
            {
                if (!Claimed.Contains(storedItem))
                {
                    Claimed.Add(storedItem);
                    return storedItem;
                }
            }
            
            Debug.LogError($"No items are left available to be claimed");
            return null;
        }
        
        public bool ClaimItemState(Item item)
        {
            foreach (var storedItem in Stored)
            {
                if (storedItem == item)
                {
                    if (Claimed.Contains(item))
                    {
                        Debug.LogError($"Attempted to Claim {item.State.UID}, but it was already claimed");
                        return false;
                    }
                    
                    Claimed.Add(item);
                    return true;
                }
            }

            Debug.LogError($"Could not find {item.State.UID} in storage");
            return false;
        }

        /// <summary>
        /// Remove the item from storage
        /// </summary>
        public void RemoveItem(Item item)
        {
            item.AssignedStorage = null;
            Stored.Remove(item);
            Claimed.Remove(item);
        }

        public Item RemoveItem(ItemState itemState)
        {
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
                    return storedItem;
                }
            }

            return null;
        }

        public bool IsEmpty()
        {
            return Stored.Count == 0 && Incoming.Count == 0;
        }
        
        private bool IsItemValidToStore(ItemData itemData)
        {
            if (IsEmpty()) return true;
            if (StoredItemData() == null) return true;

            return StoredItemData() == itemData;
        }

        public ItemData StoredItemData()
        {
            if (IsEmpty()) return null;
            if (Stored.Count > 0)
            {
                if (Stored[0].GetItemData() != null)
                {
                    return Stored[0].GetItemData();
                }
            }
            
            if (Incoming.Count > 0)
            {
                if (Incoming[0].GetItemData() != null)
                {
                    return Incoming[0].GetItemData();
                }
            }

            return null;
        }

        public int AmountCanBeWithdrawn(ItemData item)
        {
            if (IsEmpty()) return 0;
            if (!IsItemValidToStore(item)) return 0;

            return Stored.Count - Claimed.Count;
        }

        public int AmountCanBeDeposited(ItemData item)
        {
            var storedItemData = StoredItemData();
            if (storedItemData == null)
            {
                return item.MaxStackSize;
            }

            if (storedItemData != item)
            {
                return 0;
            }

            return item.MaxStackSize - (Stored.Count + Incoming.Count);
        }
    }
}