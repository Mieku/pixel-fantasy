using System;
using System.Collections.Generic;
using Characters;
using Handlers;
using Newtonsoft.Json;
using ScriptableObjects;
using TMPro;
using UnityEngine;

namespace Items
{
    [Serializable]
    public class ItemStackData
    {
        public string UniqueID;
        public bool IsAllowed;
        public List<string> StackedItemDataUIDs = new List<string>();
        public string SettingsID;
        public string CarryingKinlingUID;
        
        [JsonRequired] private float _posX;
        [JsonRequired] private float _posY;
    
        [JsonIgnore]
        public Vector2 Position
        {
            get => new(_posX, _posY);
            set
            {
                _posX = value.x;
                _posY = value.y;
            }
        }
    }
    
    public class ItemStack : PlayerInteractable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private TextMeshProUGUI _stackAmountText;
        
        private Transform _originalParent;
        private string _pendingTaskUID;

        public ItemStackData StackData;
        public int StackAmount => StackData.StackedItemDataUIDs.Count;
        public override string UniqueID => StackData.UniqueID;
        public override string DisplayName => Settings.ItemName;
        public override List<SpriteRenderer> SpritesToOutline => new List<SpriteRenderer> { _spriteRenderer };

        public List<ItemData> ItemDatas
        {
            get
            {
                List<ItemData> results = new List<ItemData>();
                foreach (var itemDataUID in StackData.StackedItemDataUIDs)
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

        public ItemSettings Settings => GameSettings.Instance.LoadItemSettings(StackData.SettingsID);

        public bool IsAllowed
        {
            get => StackData.IsAllowed;
            set
            {
                StackData.IsAllowed = value;

                foreach (var itemData in ItemDatas)
                {
                    itemData.IsAllowed = value;

                    if (value == false)
                    {
                        itemData.CancelCurrentTask();
                    }
                }
                
                RefreshAllowCommands();
                Refresh();
                InformChanged();
            }
        }

        public bool ContainsItemData(string itemDataUID)
        {
            return StackData.StackedItemDataUIDs.Contains(itemDataUID);
        }

        public override bool IsSimilar(PlayerInteractable otherPI)
        {
            if (otherPI is ItemStack otherItem)
            {
                return Settings == otherItem.Settings;
            }

            return false;
        }

        public void LoadStackData(ItemStackData stackData)
        {
            StackData = stackData;
            
            PlayerInteractableDatabase.Instance.RegisterPlayerInteractable(this);
            
            Refresh();
            RefreshAllowCommands();
        }

        public void InitItemStack(ItemData itemData, Vector2 pos)
        {
            StackData = new ItemStackData();
            StackData.StackedItemDataUIDs = new List<string>() { itemData.UniqueID };
            StackData.SettingsID = itemData.SettingsID;
            StackData.Position = pos;
            StackData.IsAllowed = true;
            
            StackData.UniqueID = CreateUID();
            
            ItemsDatabase.Instance.RegisterStack(StackData);
            PlayerInteractableDatabase.Instance.RegisterPlayerInteractable(this);
            
            if (itemData.State == EItemState.Loose)
            {
                PlaceOnGround();
            }
            
            Refresh();
            RefreshAllowCommands();
        }
        
        private string CreateUID()
         {
             return $"{Settings.ItemName}_ItemObj_{Guid.NewGuid()}";
         }

        private void Refresh()
        {
            DisplayItemSprite();
            int amount = StackData.StackedItemDataUIDs.Count;

            if (amount > 1)
            {
                _stackAmountText.text = $"{amount}";
            }
            else
            {
                _stackAmountText.text = "";
            }

            if (!IsAllowed)
            {
                var forbidCmd = GameSettings.Instance.LoadCommand("Forbid");
                AssignTaskIcon(forbidCmd);
            }
            else
            {
                AssignTaskIcon(null);
            }
        }

        private void RefreshAllowCommands()
        {
            if (IsAllowed)
            {
                AddCommand("Forbid", true);
                RemoveCommand("Allow");
            }
            else
            {
                AddCommand("Allow", true);
                RemoveCommand("Forbid");
            }
        }

        public override void AssignCommand(Command command)
        {
            if (command.CommandID == "Forbid")
            {
                CancelPendingTask();
                IsAllowed = false;
            }
            else if (command.CommandID == "Allow")
            {
                IsAllowed = true;
            }
            else
            {
                base.AssignCommand(command);
            }
        }

        public ItemStack PickUpItem(Kinling kinling, string itemDataUID)
        {
            if (StackAmount == 1)
            {
                // This item can be picked up
                var itemData = ItemsDatabase.Instance.Query(itemDataUID);
                itemData.CarryingKinlingUID = kinling.RuntimeData.UniqueID;
                itemData.State = EItemState.Carried;
                StackData.CarryingKinlingUID = kinling.UniqueID;
                Refresh();
                InformChanged();
                return this;
            }
            else
            {
                // Split off a new item
                var itemData = ItemsDatabase.Instance.Query(itemDataUID);
                StackData.StackedItemDataUIDs.Remove(itemDataUID);
                Refresh();
                itemData.CarryingKinlingUID = kinling.RuntimeData.UniqueID;
                itemData.State = EItemState.Carried;
                var splitItem = ItemsDatabase.Instance.CreateItemObject(itemData, Helper.SnapToGridPos(transform.position));
                splitItem.StackData.CarryingKinlingUID = kinling.UniqueID;
                InformChanged();
                return splitItem;
            }
        }

        public void MergeItems(ItemStack stackToBeMerged)
        {
            var itemDataUIDs = stackToBeMerged.StackData.StackedItemDataUIDs;
            Destroy(stackToBeMerged.gameObject);

            foreach (var itemDataUID in itemDataUIDs)
            {
                StackData.StackedItemDataUIDs.Add(itemDataUID);
            }

            Refresh();
        }

        public bool CanMergeIntoItem(ItemStack stackToBeMerged)
        {
            if (Settings != stackToBeMerged.Settings) return false;
            if (!IsAllowed) return false;

            int maxStack = Settings.MaxStackSize;
            int combinedStack = StackAmount + stackToBeMerged.StackAmount;
            return combinedStack <= maxStack;
        }

        public void ItemDropped()
        {
            StackData.CarryingKinlingUID = null;
            
            foreach (var itemData in ItemDatas)
            {
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

        private void Update()
        {
            StackData.Position = transform.position;
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
                    var looseItem = itemDatasAtPos.Find(i => !StackData.StackedItemDataUIDs.Contains(i.UniqueID) && i.State == EItemState.Loose);
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
                    foreach (var itemData in ItemDatas)
                    {
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
            foreach (var itemData in ItemDatas)
            {
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
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_isQuitting) return;
            
            ItemsDatabase.Instance.DeregisterStack(StackData);
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

        public override int GetStackSize()
        {
            return StackAmount;
        }
    }
}
