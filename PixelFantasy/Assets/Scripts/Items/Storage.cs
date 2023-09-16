using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Popups.Inventory;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    public class Storage : Furniture
    {
        [SerializeField] protected InventoryPanel _inventoryPanel;

        [SerializeField] private List<StorageSlot> _storageSlots = new List<StorageSlot>();
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
        
        public void SetIncoming(ItemState itemState)
        {
            foreach (var slot in _storageSlots)
            {
                int amountSpaceAvailable = slot.AmountCanBeDeposited(itemState.Data);
                if (amountSpaceAvailable != 0)
                {
                    slot.SetIncoming(itemState);
                    return;
                }
            }
        }

        public void DepositItems(ItemState itemState)
        {
            foreach (var slot in _storageSlots)
            {
                if(slot.StoredItemData() != itemState.Data) continue;
                if (slot.Incoming.Contains(itemState))
                {
                    slot.AddItem(itemState, this);
                    GameEvents.Trigger_RefreshInventoryDisplay();
                    RefreshDisplayedInventoryPanel();
                    return;
                }
            }
            
            Debug.LogError($"Item: {itemState.UID} was not deposited, was not found as incoming");
        }

        public void RestoreClaimed(ItemState itemState)
        {
            foreach (var slot in _storageSlots)
            {
                if (slot.Claimed.Contains(itemState))
                {
                    slot.Claimed.Remove(itemState);
                    return;
                }
            }

            Debug.LogError($"Item Claim: {itemState.UID} was not restored, was not found in claimed");
        }
        
        public ItemState SetClaimed(ItemData itemData)
        {
            foreach (var slot in _storageSlots)
            {
                int amountClaimable = slot.AmountCanBeWithdrawn(itemData);
                if (amountClaimable > 0)
                {
                    return slot.ClaimItem();
                }
            }
            
            Debug.LogError($"Item Claim: {itemData.ItemName} was not set, nothing could be withdrawn");
            return null;
        }

        public void WithdrawItem(ItemState itemState)
        {
            foreach (var slot in _storageSlots)
            {
                if(slot.StoredItemData() != itemState.Data) continue;
                
                slot.RemoveItem(itemState);
                GameEvents.Trigger_RefreshInventoryDisplay();
                RefreshDisplayedInventoryPanel();
                return;
            }
            
            Debug.LogError($"Item Withdrawl: {itemState.UID} was not set, could not find the requested item");
        }

        public Dictionary<ItemData, List<ItemState>> AvailableInventory
        {
            get
            {
                Dictionary<ItemData, List<ItemState>> results = new Dictionary<ItemData, List<ItemState>>();
                foreach (var slot in _storageSlots)
                {
                    if (slot != null && !slot.IsEmpty())
                    {
                        foreach (var storedItem in slot.Stored)
                        {
                            if (results.ContainsKey(storedItem.Data))
                            {
                                results[storedItem.Data].Add(storedItem);
                            }
                            else
                            {
                                results.Add(storedItem.Data, new List<ItemState>(){storedItem});
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
    }

    [Serializable]
    public class StorageSlot
    {
        public List<ItemState> Stored = new List<ItemState>();
        public List<ItemState> Incoming = new List<ItemState>();
        public List<ItemState> Claimed = new List<ItemState>();

        public int NumStored => Stored.Count;
        public int NumIncoming => Incoming.Count;
        public int NumClaimed => Claimed.Count;

        public int NumAvailable => Stored.Count - Claimed.Count;

        /// <summary>
        /// Inform the Slot that an item is on its way
        /// </summary>
        public void SetIncoming(ItemState itemState)
        {
            Incoming.Add(itemState);
        }
        
        /// <summary>
        /// Add the item to the storage
        /// </summary>
        public void AddItem(ItemState itemState, Storage storage)
        {
            itemState.Storage = storage;
            Stored.Add(itemState);
            Incoming.Remove(itemState);
        }

        public ItemState ClaimItem()
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

        /// <summary>
        /// Remove the item from storage
        /// </summary>
        public void RemoveItem(ItemState itemState)
        {
            itemState.Storage = null;
            Stored.Remove(itemState);
            Claimed.Remove(itemState);
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
                if (Stored[0].Data != null)
                {
                    return Stored[0].Data;
                }
            }
            
            if (Incoming.Count > 0)
            {
                if (Incoming[0].Data != null)
                {
                    return Incoming[0].Data;
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