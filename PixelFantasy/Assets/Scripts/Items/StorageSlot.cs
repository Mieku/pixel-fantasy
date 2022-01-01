using System;
using TMPro;
using UnityEngine;

namespace Items
{
    public class StorageSlot : MonoBehaviour
    {
        private Item _storedItem;
        private int _numIncoming;
        private int _stackedAmount;

        [SerializeField] private TextMeshPro _quantityDisplay;

        private void Start()
        {
            UpdateQuantityDisplay();
        }

        public bool IsEmpty()
        {
            return _storedItem == null && _numIncoming == 0;
        }

        /// <summary>
        /// Returns true if there is an item of the same type, and if there is space to add more,
        /// This returns false if there is nothing in the slot, or not the same time or not enough space
        /// </summary>
        public bool CanStack(Item item)
        {
            if (IsEmpty()) return false;

            if (_storedItem != null && _storedItem.IsSameItemType(item))
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
            _storedItem = item;
            _numIncoming++;
        }

        public bool CanHaveMoreIncoming()
        {
            if (_storedItem == null) return true;
            
            var totalAlloc = _numIncoming + _stackedAmount + 1;
            var maxAmount = _storedItem.GetItemData().MaxStackSize;

            return totalAlloc <= maxAmount;
        }

        public void SetItem(Item item)
        {
            _storedItem = item;
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
    }
}
