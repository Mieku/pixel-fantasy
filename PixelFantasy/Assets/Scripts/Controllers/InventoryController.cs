using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using Gods;
using UnityEngine;

namespace Controllers
{
    public class InventoryController : God<InventoryController>
    {
        private List<ItemSlot> _itemSlots = new List<ItemSlot>();
        private Dictionary<ItemData, int> _inventory = new Dictionary<ItemData, int>();

        [SerializeField] private Sprite whitePixel;

        public Dictionary<ItemData, int> Inventory => _inventory;

        public ItemSlot GetAvailableItemSlot()
        {
            foreach (var itemSlot in _itemSlots)
            {
                if (itemSlot.IsEmpty())
                {
                    return itemSlot;
                }
            }

            return null;
        }

        public void AddNewItemSlot(ItemSlot newSlot)
        {
            _itemSlots.Add(newSlot);
        }

        /// <summary>
        /// Spawns an item slot in the game at the target location
        /// </summary>
        public void SpawnItemSlot()
        {
            var position = UtilsClass.GetMouseWorldPosition();
            
            GameObject itemSlotGO = new GameObject("Item Slot", typeof(SpriteRenderer));
            itemSlotGO.GetComponent<SpriteRenderer>().sprite = whitePixel;
            itemSlotGO.GetComponent<SpriteRenderer>().color = new Color(.5f, .5f, .5f);
            itemSlotGO.GetComponent<SpriteRenderer>().sortingLayerName = "Ground";
            itemSlotGO.GetComponent<SpriteRenderer>().sortingOrder = 10;
            itemSlotGO.transform.position = position;
            itemSlotGO.transform.localScale = new Vector3(1, 1);
            
            ItemSlot itemSlot = new ItemSlot(itemSlotGO.transform);
            AddNewItemSlot(itemSlot);
        }

        public void AddToInventory(ItemData itemData, int quantity)
        {
            if (_inventory.ContainsKey(itemData))
            {
                _inventory[itemData] += quantity;
            }
            else
            {
                _inventory.Add(itemData, quantity);
            }
            
            
            GameEvents.Trigger_OnInventoryAdded(itemData, _inventory[itemData]);
        }

        public void RemoveFromInventory(ItemData itemData, int quantity)
        {
            if (_inventory.ContainsKey(itemData))
            {
                _inventory[itemData] -= quantity;
            
                GameEvents.Trigger_OnInventoryRemoved(itemData, _inventory[itemData]);
            }
            else
            {
                Debug.LogError($"Tried removing {itemData.ItemName} from inventory, but it doesn't exist!");
            }
        }
    }
    
    [Serializable]
    public class ItemSlot
    {
        private Transform itemSlotTransform;
        private Transform itemTransform;
        private bool hasItemIncoming;

        public ItemSlot(Transform itemSlotTransform)
        {
            this.itemSlotTransform = itemSlotTransform;
            SetItemTransform(null);
        }

        public bool IsEmpty()
        {
            return itemTransform == null && !hasItemIncoming;
        }

        public void HasItemIncoming(bool hasItemIncoming)
        {
            this.hasItemIncoming = hasItemIncoming;
        }

        public void SetItemTransform(Transform itemTransform)
        {
            this.itemTransform = itemTransform;
            hasItemIncoming = false;
            UpdateSprite();
        }

        public Vector3 GetPosition()
        {
            return itemSlotTransform.position;
        }

        public void UpdateSprite()
        {
            itemSlotTransform.GetComponent<SpriteRenderer>().color = IsEmpty() ? Color.gray : Color.red;
        }
    }
}
