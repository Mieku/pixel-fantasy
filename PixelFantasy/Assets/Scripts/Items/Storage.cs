using System;
using System.Collections.Generic;
using Gods;
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
        
        protected override void Awake()
        {
            if (_storageItemData != null && IsBuilt)
            {
                Init(_storageItemData);
                //CompletePlacement();
            }
            base.Awake();
        }

        public override void Init(FurnitureItemData furnitureItemData)
        {
            base.Init(furnitureItemData);

            for (int i = 0; i < _storageItemData.NumSlots; i++)
            {
                _storageSlots.Add(new StorageSlot());
            }
            
            InventoryManager.Instance.AddStorage(this);
            GameEvents.Trigger_RefreshInventoryDisplay();
            RefreshDisplayedInventoryPanel();
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
        
        public void SetIncoming(ItemData itemData, int quantity)
        {
            int remainder = quantity;
            foreach (var slot in _storageSlots)
            {
                int amountSpaceAvailable = slot.AmountCanBeDeposited(itemData);
                if (amountSpaceAvailable != 0 && amountSpaceAvailable < remainder)
                {
                    remainder -= amountSpaceAvailable;
                    slot.SetIncoming(itemData, amountSpaceAvailable);
                } 
                else if (amountSpaceAvailable != 0 && amountSpaceAvailable >= remainder)
                {
                    slot.SetIncoming(itemData, remainder);
                    remainder = 0;
                }

                if (remainder == 0)
                {
                    return;
                }
            }

            if (remainder > 0)
            {
                Debug.LogError("There was still a remainder after setting incoming: " + remainder);
            }
        }

        public void DepositItems(ItemData itemData, int quantity)
        {
            int remainder = quantity;
            foreach (var slot in _storageSlots)
            {
                if(slot.Item != itemData) continue;
                
                int space = slot.NumIncoming;
                if (space < remainder)
                {
                    slot.AddItem(itemData, space);
                    remainder -= space;
                }
                else
                {
                    slot.AddItem(itemData, remainder);
                    remainder = 0;
                }
                
                if (remainder == 0)
                {
                    GameEvents.Trigger_RefreshInventoryDisplay();
                    RefreshDisplayedInventoryPanel();
                    return;
                }
            }
            
            if (remainder > 0)
            {
                Debug.LogError("There was still a remainder after depositting item: " + remainder);
            }
            
            GameEvents.Trigger_RefreshInventoryDisplay();
            RefreshDisplayedInventoryPanel();
        }

        public void RestoreClaimed(ItemData itemData, int quantity)
        {
            int remainder = quantity;
            foreach (var slot in _storageSlots)
            {
                if (slot.Item == itemData)
                {
                    if (slot.NumClaimed >= remainder)
                    {
                        slot.NumClaimed -= remainder;
                        return;
                    }
                    else
                    {
                        int amountToRemove = slot.NumClaimed;
                        slot.NumClaimed = 0;
                        remainder -= amountToRemove;
                    }
                }

                if (remainder == 0)
                {
                    return;
                }
            }

            if (remainder > 0)
            {
                Debug.LogError("There was still a remainder after restoring claimed: " + remainder);
            }
        }
        
        public void SetClaimed(ItemData itemData, int quantity)
        {
            int remainder = quantity;
            foreach (var slot in _storageSlots)
            {
                int amountClaimable = slot.AmountCanBeWithdrawn(itemData);
                if (amountClaimable != 0 && amountClaimable < remainder)
                {
                    remainder -= amountClaimable;
                    slot.SetClaimed(amountClaimable);
                }
                else if (amountClaimable != 0 && amountClaimable >= remainder)
                {
                    slot.SetClaimed(remainder);
                    remainder = 0;
                }

                if (remainder == 0)
                {
                    return;
                }
            }
            
            if (remainder > 0)
            {
                Debug.LogError("There was still a remainder after setting claimed: " + remainder);
            }
        }

        public void WithdrawItems(ItemData itemData, int quantity)
        {
            int remainder = quantity;
            foreach (var slot in _storageSlots)
            {
                if(slot.Item != itemData) continue;

                int available = slot.NumClaimed;
                if (available < quantity)
                {
                    slot.RemoveItem(available);
                    remainder -= available;
                }
                else
                {
                    slot.RemoveItem(remainder);
                    remainder = 0;
                }
                
                if (remainder == 0)
                {
                    GameEvents.Trigger_RefreshInventoryDisplay();
                    RefreshDisplayedInventoryPanel();
                    return;
                }
            }
            
            if (remainder > 0)
            {
                Debug.LogError("There was still a remainder after Withdrawing item: " + remainder);
            }
            
            GameEvents.Trigger_RefreshInventoryDisplay();
            RefreshDisplayedInventoryPanel();
        }

        public Dictionary<ItemData, int> AvailableInventory
        {
            get
            {
                Dictionary<ItemData, int> result = new Dictionary<ItemData, int>();
                foreach (var slot in _storageSlots)
                {
                    if (slot != null && !slot.IsEmpty())
                    {
                        if (result.ContainsKey(slot.Item))
                        {
                            result[slot.Item] += slot.NumStored;
                        }
                        else
                        {
                            result[slot.Item] = slot.NumStored;
                        }
                    }
                }

                return result;
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
    }

    [Serializable]
    public class StorageSlot
    {
        public ItemData Item;
        public int NumStored;
        public int NumIncoming;
        public int NumClaimed;

        public int NumAvailable => NumStored - NumClaimed;

        /// <summary>
        /// Inform the Slot that an item is on its way
        /// </summary>
        public void SetIncoming(ItemData itemData, int quantity)
        {
            Item = itemData;
            NumIncoming += quantity;
        }
        
        /// <summary>
        /// Add the item to the storage
        /// </summary>
        public void AddItem(ItemData itemData, int quantity)
        {
            Item = itemData;
            NumStored += quantity;
            NumIncoming -= quantity;
        }

        /// <summary>
        /// Inform the slot that the item is claimed by someone on their way to pick it up
        /// </summary>
        public void SetClaimed(int quantity)
        {
            NumClaimed += quantity;
        }

        /// <summary>
        /// Remove the item from storage
        /// </summary>
        public void RemoveItem(int quantity)
        {
            NumStored -= quantity;
            NumClaimed -= quantity;

            if (IsEmpty())
            {
                Item = null;
            }
        }

        public bool IsEmpty()
        {
            return NumStored == 0 && NumIncoming == 0;
        }
        
        public int AmountCanBeStored(ItemData item)
        {
            if (Item != null)
            {
                if (Item != item) // Not the same item
                {
                    return 0;
                }

                int totalStoredOrIncoming = NumStored + NumIncoming;
                return Item.MaxStackSize - totalStoredOrIncoming;
            }
            else
            {
                return item.MaxStackSize;
            }
        }

        public int AmountCanBeWithdrawn(ItemData item)
        {
            if (IsEmpty()) return 0;
            if (Item != item) return 0;

            return NumStored - NumClaimed;
        }

        public int AmountCanBeDeposited(ItemData item)
        {
            if (Item != null && Item != item) return 0;

            return item.MaxStackSize - (NumStored + NumIncoming);
        }
    }
}
