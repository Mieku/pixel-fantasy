using System;
using System.Collections.Generic;
using System.Linq;
using Handlers;
using TMPro;
using UnityEngine;

namespace Items
{
    public class ItemStack : PlayerInteractable
    {
        [SerializeField] private SpriteRenderer _itemRenderer;
        [SerializeField] private TextMeshProUGUI _stackAmountText;

        public override string UniqueID => _uid;
        public override string DisplayName => $"{ItemSettings.ItemName} Stack (x{StackAmount})";
        public override string PendingTaskUID { get; set; }
        public ItemSettings ItemSettings => ItemsDatabase.Instance.Query(_stackedItemDataUIDs.First()).Settings;
        public int StackAmount => _stackedItemDataUIDs.Count;
        public bool IsAllowed { get; set; }
        public Vector2 Position =>  Helper.SnapToGridPos(transform.position);
        
        private List<string> _stackedItemDataUIDs = new List<string>();
        private string _uid;
        
        public void InitStack(List<string> itemDataUIDs)
        {
            ItemsDatabase.Instance.RegisterStack(this);
            
            _stackedItemDataUIDs = itemDataUIDs;
            _uid = CreateUID();

            IsAllowed = true;
            foreach (var itemDataUID in _stackedItemDataUIDs)
            {
                var itemData = ItemsDatabase.Instance.Query(itemDataUID);
                if (!itemData.IsAllowed)
                {
                    IsAllowed = false;
                    break;
                }
            }
            
            Refresh();
            
            PlayerInteractableDatabase.Instance.RegisterPlayerInteractable(this);
        }

        public void AddItemToStack(Item item)
        {
            var itemData = item.RuntimeData;
            Destroy(item.gameObject);
            AddItemToStack(itemData);
        }

        public void AddItemToStack(ItemData itemData)
        {
            itemData.State = EItemState.Stacked;
            _stackedItemDataUIDs.Add(itemData.UniqueID);
            
            Refresh();
        }

        public Item RemoveItemFromStack(ItemData itemData)
        {
            if (!_stackedItemDataUIDs.Contains(itemData.UniqueID))
            {
                // Should never happen
                Debug.LogError("Attempted to remove item that isn't in stack");
                return null;
            }

            itemData.State = EItemState.Carried;
            _stackedItemDataUIDs.Remove(itemData.UniqueID);
            
            Refresh();

            var item = ItemsDatabase.Instance.CreateItemObject(itemData, transform.position);
            return item;
        }

        // If the stack only has 1 item, convert it into an item
        private void ConvertStackToItem()
        {
            var itemData = ItemsDatabase.Instance.Query(_stackedItemDataUIDs.First());
            itemData.State = EItemState.Loose;
            ItemsDatabase.Instance.CreateItemObject(itemData, transform.position);
            
            ItemsDatabase.Instance.DeregisterStack(this);
            
            Destroy(gameObject);
        }

        public bool CanItemJoinStack(ItemData itemData)
        {
            if (!IsAllowed) return false;
            if (ItemSettings != itemData.Settings) return false;

            int maxStack = ItemSettings.MaxStackSize;
            return StackAmount < maxStack;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (!_isQuitting)
            {
                PlayerInteractableDatabase.Instance.DeregisterPlayerInteractable(this);
            }
        }

        private void Refresh()
        {
            if (StackAmount == 0)
            {
                Destroy(gameObject);
            } 
            else if (StackAmount == 1)
            {
                ConvertStackToItem();
            }
            else
            {
                _itemRenderer.sprite = ItemSettings.ItemSprite;
                _stackAmountText.text = $"{StackAmount}";
            
                InformChanged();
            }
        }
        
        public override bool IsSimilar(PlayerInteractable otherPI)
        {
            if (otherPI is Item item)
            {
                if (item.RuntimeData.Settings == ItemSettings)
                {
                    return true;
                }
            }

            if (otherPI is ItemStack stack)
            {
                if (stack.ItemSettings == ItemSettings)
                {
                    return true;
                }
            }

            return false;
        }

        public override Vector2? UseagePosition(Vector2 requestorPosition)
        {
            return transform.position;
        }
        
        private string CreateUID()
        {
            return $"{ItemSettings.ItemName}_Stack_{Guid.NewGuid()}";
        }
    }
}
