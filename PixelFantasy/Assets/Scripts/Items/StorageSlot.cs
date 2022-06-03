using System;
using System.Collections.Generic;
using Gods;
using ScriptableObjects;
using TMPro;
using UnityEngine;

namespace Items
{
    public class StorageSlot : MonoBehaviour
    {
        private int _storedAmount;
        private int _claimedAmount;
        
        private int _numIncoming;
        private int _stackedAmount;
        private ItemData _storedType;

        [SerializeField] private TextMeshPro _quantityDisplay;
        [SerializeField] private SpriteRenderer _storedItemRenderer;

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
            return _storedAmount == 0 && _numIncoming == 0 && _stackedAmount == 0;
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

        public void HasItemIncoming(Item item)
        {
            _storedType = item.GetItemData();
            _numIncoming++;
        }

        public bool CanHaveMoreIncoming()
        {
            if (_storedType == null) return true;
            
            var totalAlloc = _numIncoming + _stackedAmount + 1;
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
            
            var totalAlloc = _numIncoming + _stackedAmount;
            var maxAmount = _storedType.MaxStackSize;
            return maxAmount - totalAlloc;
        }

        public void SetItem(Item item)
        {
            _storedType = item.GetItemData();
            _storedAmount++;
            _stackedAmount++;
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
            if (_stackedAmount == 0)
            {
                _quantityDisplay.text = "";
                UpdateStoredItemDisplay(null);
            }
            else
            {
                _quantityDisplay.text = _stackedAmount.ToString();
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

        public Item ClaimItem()
        {
            if (_storedAmount > 0)
            {
                _storedAmount--;
                _claimedAmount++;
                var item = Spawner.Instance.SpawnItem(_storedType, transform.position, false);
                item.gameObject.SetActive(false);
                return item;
            }
            else
            {
                return null;
            }
        }

        public void RemoveClaimedItem(Item claimedItem)
        {
            _stackedAmount--;
            UpdateQuantityDisplay();
            _claimedAmount--;

            if (IsEmpty())
            {
                _storedType = null;
            }
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
    }
}
