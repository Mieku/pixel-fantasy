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
            _resourceCost = _itemToCraft.ResourceCosts;
            _isCraftingItem = true;
            ShowCraftOnTable(_itemToCraft);
        }

        public void AddResourceToCrafting(ItemData itemData)
        {
            foreach (var cost in _resourceCost)
            {
                if (cost.Item == itemData && cost.Quantity > 0)
                {
                    cost.Quantity--;
                    if (cost.Quantity <= 0)
                    {
                        _resourceCost.Remove(cost);
                    }

                    return;
                }
            }
        }

        // If all the resources are available, make a crafting task to build it
        public void CheckIfAllResourcesLoadedInCrafting()
        {
            if (_resourceCost.Count == 0)
            {
                CreateCraftItemTask();
            }
        }

        private void CreateCraftItemTask()
        {
            var task = new CarpentryTask.CraftItem
            {
                craftingTable = this,
                craftPosition = UsagePosition.position,
                workAmount = _itemToCraft.WorkToCraft,
                completeWork = CompleteCrafting
            };
            
            _assignedTaskRefs.Add(task.GetHashCode());
            taskMaster.CarpentryTaskSystem.AddTask(task);
        }

        private void CompleteCrafting()
        {
            _isCraftingItem = false;
            ShowCraftOnTable(null);

            if (_itemToCraft.ItemData != null)
            {
                Spawner.Instance.SpawnItem(_itemToCraft.ItemData, transform.position, true);
                _itemToCraft = null;
            }
            // TODO: Add Furniture
            
        }
        
        public override void Init(FurnitureData furnitureData, PlacementDirection direction)
        {
            base.Init(furnitureData, direction);
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

        protected override void CompleteConstruction()
        {
            base.CompleteConstruction();
            craftMaster.AddCraftingTable(this, FurnitureData.CraftingType);
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
                } 
                else if (craft.FurnitureData != null)
                {
                    tableSprite.sprite = craft.FurnitureData.Icon;
                }
                
                tableSprite.gameObject.SetActive(true);
            }
        }
    }
}
