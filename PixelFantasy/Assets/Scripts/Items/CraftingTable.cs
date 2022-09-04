using System;
using System.Collections.Generic;
using Gods;
using ScriptableObjects;
using Tasks;
using UnityEngine;

namespace Items
{
    public class CraftingTable : Furniture
    {
        private CraftingTask _itemToCraft;
        private bool _isCraftingItem;
 
        public bool IsCraftingItem => _isCraftingItem;
        
        public void AssignCraft(CraftingTask itemToCraft)
        {
            _itemToCraft = itemToCraft;
            _remainingResourceCosts = _itemToCraft.ResourceCosts;
            _isCraftingItem = true;
            ShowCraftOnTable(_itemToCraft);
        }

        public void AddResourceToCrafting(ItemData itemData)
        {
            foreach (var cost in _remainingResourceCosts)
            {
                if (cost.Item == itemData && cost.Quantity > 0)
                {
                    cost.Quantity--;
                    if (cost.Quantity <= 0)
                    {
                        _remainingResourceCosts.Remove(cost);
                    }

                    return;
                }
            }
        }

        // If all the resources are available, make a crafting task to build it
        public void CheckIfAllResourcesLoadedInCrafting()
        {
            if (_remainingResourceCosts.Count == 0)
            {
                CreateCraftItemTask();
            }
        }

        public CarpentryTask.CraftItem CreateCraftItemTask(bool autoAssign = true)
        {
            var task = new CarpentryTask.CraftItem
            {
                TargetUID = UniqueId,
                craftingTable = this,
                craftPosition = UsagePosition.position,
                // workAmount = _itemToCraft.WorkToCraft,
                // completeWork = CompleteCrafting
            };
            
            _assignedTaskRefs.Add(task.GetHashCode());

            if (autoAssign)
            {
                taskMaster.CarpentryTaskSystem.AddTask(task);
            }

            return task;
        }

        private void CompleteCrafting()
        {
            _isCraftingItem = false;
            ShowCraftOnTable(null);

            if (_itemToCraft.ItemData != null)
            {
                Spawner.Instance.SpawnItem(_itemToCraft.ItemData, transform.position, true, _itemToCraft.ItemData.CraftedQuantity);
                _itemToCraft = null;
            } 
        }
        
        public override void Init(FurnitureData furnitureData, PlacementDirection direction)
        {
            base.Init(furnitureData, direction);
            _isCraftingTable = true;
        }

        private void OnDestroy()
        {
            CraftMaster.Instance.RemoveCraftingTable(this, FurnitureData.CraftingType);
        }

        public void AddIncomingItem(Item item)
        {
            _incomingItems.Add(item);
        }

        public void RemoveIncomingItem(Item item)
        {
            _incomingItems.Remove(item);
        }

        public void AddTaskReference(int reference)
        {
            _assignedTaskRefs.Add(reference);
        }

        public override void CompleteConstruction()
        {
            base.CompleteConstruction();
            craftMaster.AddCraftingTable(this, FurnitureData.CraftingType);
        }

        public override void RestoreState(object data)
        {
            base.RestoreState(data);
            
            var furnState = (Data)data;
            var craftingState = furnState.CraftingTableData;

            _isCraftingItem = craftingState.IsCraftingItem;

            if (_isBuilt )
            {
                craftMaster.AddCraftingTable(this, FurnitureData.CraftingType);
            }

            if (_isCraftingItem)
            {
                _itemToCraft = new CraftingTask
                {
                    FurnitureData = craftingState.CraftFurnitureData,
                    ItemData = craftingState.CraftItemData,
                };
                //CraftMissingItems(GetRemainingMissingItems());
                AssignCraft(_itemToCraft);
            }
        }

        public override object CaptureState()
        {
            var state = (Data)base.CaptureState();
            state.CraftingTableData = new CraftingTableData
            {
                IsCraftingItem = _isCraftingItem,
            };
            
            if (_itemToCraft != null)
            {
                var craftItem = _itemToCraft.ItemData;
                var craftFurn = _itemToCraft.FurnitureData;
                
                state.CraftingTableData.CraftItemData = craftItem;
                state.CraftingTableData.CraftFurnitureData = craftFurn;
            }

            return state;
        }

        private void ShowCraftOnTable(CraftingTask craft)
        {
            var tableSprite = _prefab.CraftedItemRenderer;
            
            if (craft == null)
            {
                tableSprite.gameObject.SetActive(false);
            }
            else
            {
                if (craft.ItemData != null)
                {
                    tableSprite.sprite = craft.ItemData.ItemSprite;
                    tableSprite.transform.localScale = craft.ItemData.DefaultSpriteScale;
                }
                
                tableSprite.gameObject.SetActive(true);
            }
        }

        public struct CraftingTableData
        {
            public ItemData CraftItemData;
            public FurnitureData CraftFurnitureData;
            
            public bool IsCraftingItem;
        }
    }
}
