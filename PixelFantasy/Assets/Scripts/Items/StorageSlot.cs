using System;
using System.Collections.Generic;
using Controllers;
using DataPersistence;
using Gods;
using ScriptableObjects;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Items
{
    public class StorageSlot : UniqueObject, IPersistent
    {
        private int _storedAmount;
        private int _claimedAmount;
        private int _numIncoming;
        private ItemData _storedType;
        
        [SerializeField] private TextMeshPro _quantityDisplay;
        [SerializeField] private SpriteRenderer _storedItemRenderer;

        private InventoryController _inventoryController => ControllerManager.Instance.InventoryController;
        
        private void Start()
        {
            UpdateQuantityDisplay();
        }

        public void Init()
        {
            UpdateStoredItemDisplay(null);
        }

        public bool IsEmpty()
        {
            return _storedAmount == 0 && _numIncoming == 0;
        }

        /// <summary>
        /// Returns true if there is an item of the same type, and if there is space to add more,
        /// This returns false if there is nothing in the slot, or not the same time or not enough space
        /// </summary>
        public bool CanStack(Item item)
        {
            if (IsEmpty()) return false;

            if (_storedType == item.GetItemData())
            {
                if (CanHaveMoreIncoming())
                {
                    return true;
                }
            }

            return false;
        }

        public void AddItemIncoming(Item item)
        {
            _storedType = item.GetItemData();
            _numIncoming++;
        }

        public bool CanHaveMoreIncoming()
        {
            if (_storedType == null) return true;
            
            var totalAlloc = _numIncoming + _storedAmount + 1;
            var maxAmount = _storedType.MaxStackSize;

            return totalAlloc <= maxAmount;
        }

        public int SpaceRemaining(Item item)
        {
            if (_storedType == null)
            {
                return item.GetItemData().MaxStackSize;
            }

            if (_storedType != item.GetItemData())
            {
                return 0;
            }
            
            var totalAlloc = _numIncoming + _storedAmount;
            var maxAmount = _storedType.MaxStackSize;
            return maxAmount - totalAlloc;
        }

        public void StoreItem(Item item)
        {
            _storedType = item.GetItemData();
            _storedAmount++;
            _numIncoming--;
            UpdateQuantityDisplay();
            UpdateStoredItemDisplay(_storedType.ItemSprite);
        }

        private void UpdateStoredItemDisplay(Sprite storedItemSprite)
        {
            if (storedItemSprite == null)
            {
                _storedItemRenderer.gameObject.SetActive(false);
            }
            else
            {
                _storedItemRenderer.sprite = storedItemSprite;
                _storedItemRenderer.gameObject.SetActive(true);
            }
        }

        private void UpdateQuantityDisplay()
        {
            if (_storedAmount == 0)
            {
                _quantityDisplay.text = "";
                UpdateStoredItemDisplay(null);
            }
            else
            {
                _quantityDisplay.text = _storedAmount.ToString();
            }
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public ItemData GetStoredType()
        {
            return _storedType;
        }

        public StorageSlot ClaimItem()
        {
            if (_storedAmount - _claimedAmount > 0)
            {
                _claimedAmount++;
                return this;
            }
            else
            {
                return null;
            }
        }

        public Item GetItem()
        {
            var item = Spawner.Instance.SpawnItem(_storedType, transform.position, false);
            RemoveClaimedItem();
            
            return item;
        }

        public void RemoveClaimedItem()
        {
            _claimedAmount--;
            _storedAmount--;

            _inventoryController.RemoveFromInventory(_storedType, 1);
            
            if (IsEmpty())
            {
                _storedType = null;
                UpdateStoredItemDisplay(null);
            }
            else
            {
                UpdateStoredItemDisplay(_storedType.ItemSprite);
            }
            UpdateQuantityDisplay();
        }

        public bool HasItemClaimed(Item item)
        {
            if (_claimedAmount > 0 && _storedType == item.GetItemData())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object CaptureState()
        {
            return new Data
            {
                UID = this.UniqueId,
                Position = transform.position,
                StoredAmount = _storedAmount,
                StoredType = _storedType,
            };
        }

        public void RestoreState(object data)
        {
            var itemState = (Data)data;

            UniqueId = itemState.UID;
            transform.position = itemState.Position;
            _storedAmount = itemState.StoredAmount;
            _storedType = itemState.StoredType;
            
            UpdateQuantityDisplay();
            if (_storedType != null)
            {
                UpdateStoredItemDisplay(_storedType.ItemSprite);
            }
        }

        public struct Data
        {
            public string UID;
            public Vector3 Position;
            public int StoredAmount;
            public ItemData StoredType;
        }
    }
}
