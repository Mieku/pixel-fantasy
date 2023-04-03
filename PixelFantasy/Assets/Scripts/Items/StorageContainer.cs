using System;
using System.Collections.Generic;
using System.Linq;
using Gods;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    public class StorageContainer : ClickableObject
    {
        [SerializeField] private SpriteRenderer _containerSprite;
        [SerializeField] private List<ItemSlot> _slots;
    
        public Family Owner;

        private StorageItemData _storageItemData;
        private float _maxWeight;
        private int _numSlots;
        private string _name;

        protected override void Awake()
        {
            _spriteToOutline = _containerSprite;
            base.Awake();
        }

        public void Init(StorageItemData storageItemData, Family owner)
        {
            _storageItemData = storageItemData;
            Owner = owner;
            _containerSprite.sprite = _storageItemData.ItemSprite;
            _name = _storageItemData.ItemName;
            _maxWeight = _storageItemData.MaxWeight;
            _numSlots = _storageItemData.NumSlots;
            
            for (int i = 0; i < _numSlots; i++)
            {
                _slots.Add(new ItemSlot());
            }
        }

        /// <summary>
        /// Adds Items to the storage container
        /// </summary>
        /// <returns>Returns the items that don't fit</returns>
        public List<ItemData> AddItems(List<ItemData> items)
        {
            List<ItemData> itemsToStore = new List<ItemData>(items);
            List<ItemData> itemsStored = new List<ItemData>();
            foreach (var item in items)
            {
                if (AddItemToContainer(item))
                {
                    itemsStored.Add(item);
                }
            }

            foreach (var storedItem in itemsStored)
            {
                itemsToStore.Remove(storedItem);
            }

            return itemsToStore;
        }

        public List<Item> AddItems(List<Item> items)
        {
            // TODO: Build me!
            return null;
        }

        private bool AddItemToContainer(ItemData itemData)
        {
            foreach (var slot in _slots)
            {
                if (slot.TryAddItem(itemData))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void OnClicked()
        {
            // TODO: Build me!
            Debug.Log("Clicked!");
        }
    }

    [Serializable]
    public class ItemSlot
    {
        public List<ItemEntry> StoredEntries = new List<ItemEntry>();

        public List<ItemData> GetEntriesAsItemDatas()
        {
            List<ItemData> result = new List<ItemData>();
            foreach (var itemEntry in StoredEntries)
            {
                result.Add(itemEntry.ItemData);
            }

            return result;
        }

        public bool CanAddItem(ItemData itemData)
        {
            if (StoredEntries.Count == 0)
            {
                return true;
            }

            if (StoredEntries[0].ItemData != itemData) return false;

            var maxStack = itemData.MaxStackSize;
            if (TotalItemsStored() >= maxStack) return false;

            return true;
        }

        public bool TryAddItem(ItemData itemData)
        {
            if (CanAddItem(itemData))
            {
                ItemEntry entry = new ItemEntry(itemData, ItemEntry.ItemEntryStatus.Stored);
                StoredEntries.Add(entry);
                return true;
            }

            return false;
        }

        public int TotalItemsStored()
        {
            return StoredEntries.Count;
        }
    }

    [Serializable]
    public class ItemEntry
    {
        public ItemData ItemData;
        public ItemEntryStatus Status;

        public ItemEntry (ItemData itemData, ItemEntryStatus status)
        {
            ItemData = itemData;
            Status = status;
        }
        
        [Serializable]
        public enum ItemEntryStatus
        {
            Stored,
            Claimed,
            Incoming,
        }
    }

    
}
