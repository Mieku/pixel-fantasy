using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Handlers;
using TMPro;
using UnityEngine;

namespace Items
{
    public class Item : PlayerInteractable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private TextMeshProUGUI _stackAmountText;
        
        private Transform _originalParent;
        private List<string> _itemDatas = new List<string>();
        private string _settingsID;
        private string _uniqueID;
        private string _pendingTaskUID;

        public int StackAmount => _itemDatas.Count;
        public override string UniqueID => _uniqueID;
        public override string DisplayName => Settings.ItemName;

        public List<ItemData> ItemDatas
        {
            get
            {
                List<ItemData> results = new List<ItemData>();
                foreach (var itemDataUID in _itemDatas)
                {
                    var itemData = ItemsDatabase.Instance.Query(itemDataUID);
                    results.Add(itemData);
                }

                return results;
            }
        }
        
        public override string PendingTaskUID
        {
            get => _pendingTaskUID;
            set => _pendingTaskUID = value;
        }

        public ItemSettings Settings
        {
            get
            {
                var itemData = ItemsDatabase.Instance.Query(_itemDatas.First());
                return itemData.Settings;
            }
        }
        
        public bool IsAllowed { get; set; }
        public bool IsClickDisabled { get; set; }

        public bool ContainsItemData(string itemDataUID)
        {
            return _itemDatas.Contains(itemDataUID);
        }

        public override bool IsSimilar(PlayerInteractable otherPI)
        {
            if (otherPI is Item otherItem)
            {
                return Settings == otherItem.Settings;
            }

            return false;
        }

        public void LoadItemData(ItemData itemData)
        {
            _itemDatas.Add(itemData.UniqueID);
            _uniqueID = CreateUID();
           
            PlayerInteractableDatabase.Instance.RegisterPlayerInteractable(this);

            IsAllowed = itemData.IsAllowed;

            if (itemData.State == EItemState.Loose)
            {
                PlaceOnGround();
            }
            
            Refresh();
        }
        
        private string CreateUID()
         {
             return $"{Settings.ItemName}_ItemObj_{Guid.NewGuid()}";
         }

        private void Refresh()
        {
            DisplayItemSprite();
            int amount = _itemDatas.Count;

            if (amount > 1)
            {
                _stackAmountText.text = $"{amount}";
            }
            else
            {
                _stackAmountText.text = "";
            }
        }

        public Item PickUpItem(Kinling kinling, string itemDataUID)
        {
            if (_itemDatas.Count == 1)
            {
                // This item can be picked up
                var itemData = ItemsDatabase.Instance.Query(itemDataUID);
                itemData.CarryingKinlingUID = kinling.RuntimeData.UniqueID;
                itemData.State = EItemState.Carried;
                Refresh();
                return this;
            }
            else
            {
                // Split off a new item
                var itemData = ItemsDatabase.Instance.Query(itemDataUID);
                _itemDatas.Remove(itemDataUID);
                Refresh();
                itemData.CarryingKinlingUID = kinling.RuntimeData.UniqueID;
                itemData.State = EItemState.Carried;
                var splitItem = ItemsDatabase.Instance.CreateItemObject(itemData, Helper.SnapToGridPos(transform.position));
                return splitItem;
            }
        }

        public void MergeItems(Item itemToBeMerged)
        {
            var itemDataUIDs = itemToBeMerged._itemDatas;
            Destroy(itemToBeMerged.gameObject);

            foreach (var itemDataUID in itemDataUIDs)
            {
                _itemDatas.Add(itemDataUID);
            }

            Refresh();
        }

        public bool CanMergeIntoItem(Item itemToBeMerged)
        {
            if (Settings != itemToBeMerged.Settings) return false;
            if (!IsAllowed) return false;

            int maxStack = Settings.MaxStackSize;
            int combinedStack = _itemDatas.Count + itemToBeMerged._itemDatas.Count;
            return combinedStack <= maxStack;
        }

        public void ItemDropped()
        {
            foreach (var itemDataUID in _itemDatas)
            {
                var itemData = ItemsDatabase.Instance.Query(itemDataUID);
                itemData.AssignedStorageID = null;
                itemData.CarryingKinlingUID = null;
                itemData.State = EItemState.Loose;
            }
            
            PlaceOnGround();
                        
            Refresh();
        }

        public void UpdatePosition()
        {
            foreach (var itemData in ItemDatas)
            {
                itemData.Position = transform.position;
            }
        }

        private void PlaceOnGround()
        {
            var groundPos = Helper.SnapToGridPos(transform.position);
            var checkedPositions = new HashSet<Vector2>(); // To track checked positions
            int radius = 0;
            int maxRadius = 5; // Adjust the max radius as needed

            while (radius <= maxRadius)
            {
                var positionsToCheck = GetPositionsInRadius(groundPos, radius, checkedPositions);

                foreach (var pos in positionsToCheck)
                {
                    // Check if this position has already been checked
                    if (checkedPositions.Contains(pos))
                    {
                        continue;
                    }

                    // Mark this position as checked
                    checkedPositions.Add(pos);

                    // Check pos for item or stack
                    bool isPosInvalid = Helper.DoesGridContainTag(pos, "Obstacle");
                    if (isPosInvalid)
                    {
                        continue; // No position available, try next adjacent pos
                    }
                    
                    var itemDatasAtPos = ItemsDatabase.Instance.FindAllItemDatasAtPosition(pos);
                    var looseItem = itemDatasAtPos.Find(i => !_itemDatas.Contains(i.UniqueID) && i.State == EItemState.Loose);
                    if (looseItem != null)
                    {
                        var item = ItemsDatabase.Instance.FindItemObject(looseItem.UniqueID);
                        
                        if (item.CanMergeIntoItem(this))
                        {
                            // Create Stack
                            item.MergeItems(this);
                            return;
                        }
                        continue; // No position available, try next adjacent pos
                    }
                    
                    // Spot is free
                    transform.position = pos;
                    foreach (var itemDataUID in _itemDatas)
                    {
                        var itemData = ItemsDatabase.Instance.Query(itemDataUID);
                        itemData.Position = pos;
                        itemData.State = EItemState.Loose;
                    }
                    return;
                }

                // Increase the search radius if no valid position was found
                radius++;
            }

            // If no valid position found after expanding the search radius
            Debug.LogWarning("No valid position found to place the item, placing it loosely on the ground");
            transform.position = groundPos;
            foreach (var itemDataUID in _itemDatas)
            {
                var itemData = ItemsDatabase.Instance.Query(itemDataUID);
                itemData.Position = groundPos;
                itemData.State = EItemState.Loose;
            }
        }

        private List<Vector2> GetPositionsInRadius(Vector2 center, int radius, HashSet<Vector2> checkedPositions)
        {
            var positions = new List<Vector2>();

            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    // Only include positions at the edges of the current radius
                    if (Mathf.Abs(x) == radius || Mathf.Abs(y) == radius)
                    {
                        var pos = new Vector2(center.x + x, center.y + y);

                        if (!checkedPositions.Contains(pos))
                        {
                            positions.Add(pos);
                        }
                    }
                }
            }

            return positions;
        }

        private void DisplayItemSprite()
        {
            _spriteRenderer.sprite = Settings.ItemSprite;
        }
        
        public void ToggleAllowed(bool isAllowed)
        {
            IsAllowed = isAllowed;
            if (IsAllowed)
            {
                //_icon.gameObject.SetActive(false);
                //_icon.sprite = null;
                //CreateHaulTask();
            }
            else
            {
                //_icon.gameObject.SetActive(true);
                //_icon.sprite = Librarian.Instance.GetSprite("Lock");
                //CancelAssignedTask();
            }
            
            RefreshSelection();
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_isQuitting) return;
            
            PlayerInteractableDatabase.Instance.DeregisterPlayerInteractable(this);
        }

        private void RefreshSelection()
        {
            if (IsSelected)
            {
                GameEvents.Trigger_RefreshSelection();
            }
        }

        public override Vector2? UseagePosition(Vector2 requestorPosition)
        {
            return transform.position;
        }
    }
}
