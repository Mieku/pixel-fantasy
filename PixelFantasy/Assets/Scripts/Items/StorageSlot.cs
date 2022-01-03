using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Items
{
    public class StorageSlot : MonoBehaviour
    {
        private List<Item> _storedItems = new List<Item>();
        private List<Item> _claimedItems = new List<Item>();
        private int _numIncoming;
        private int _stackedAmount;
        private ItemData _storedType;

        [SerializeField] private TextMeshPro _quantityDisplay;

        private void Start()
        {
            UpdateQuantityDisplay();
        }

        public bool IsEmpty()
        {
            return _storedItems.Count == 0 && _numIncoming == 0;
        }

        /// <summary>
        /// Returns true if there is an item of the same type, and if there is space to add more,
        /// This returns false if there is nothing in the slot, or not the same time or not enough space
        /// </summary>
        public bool CanStack(Item item)
        {
            if (IsEmpty()) return false;

            if (_storedItems.Count > 0 && _storedType == item.GetItemData())
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

        public void SetItem(Item item)
        {
            _storedItems.Add(item);
            _stackedAmount++;
            UpdateQuantityDisplay();
            _numIncoming--;
        }

        private void UpdateQuantityDisplay()
        {
            if (_stackedAmount == 0)
            {
                _quantityDisplay.text = "";
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
            if (_storedItems.Count > 0)
            {
                var item = _storedItems[0];
                _storedItems.Remove(item);
                _claimedItems.Add(item);
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
            _claimedItems.Remove(claimedItem);
        }

        public bool HasItemClaimed(Item item)
        {
            return _claimedItems.Contains(item);
        }
    }
}
