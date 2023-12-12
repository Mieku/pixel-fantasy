using System;
using System.Collections.Generic;
using Buildings;
using Characters;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace TaskSystem
{
    [Serializable]
    public class CraftingBill
    {
        public CraftedItemData ItemToCraft;
        public PlayerInteractable Requestor;
        public Action OnCancelled;
        public List<RequestedItemInfo> RequestedItemInfos = new List<RequestedItemInfo>();

        private CraftingTable _assignedCraftingTable;

        private List<RequestedItemInfo> ClaimRequiredMaterials(CraftedItemData itemData)
        {
            bool canGetAllItems = true;
            List<RequestedItemInfo> results = new List<RequestedItemInfo>();
            
            var costs = itemData.GetResourceCosts();
            foreach (var cost in costs)
            {
                for (int i = 0; i < cost.Quantity; i++)
                {
                    var item = InventoryManager.Instance.ClaimItem(cost.Item);
                    if (item != null)
                    {
                        results = AddToRequestedItemsList(item, results);
                    }
                    else
                    {
                        canGetAllItems = false;
                        break;
                    }
                }

                if (!canGetAllItems)
                {
                    break;
                }
            }

            if (canGetAllItems)
            {
                return results;
            }
            else
            {
                foreach (var resultItemInfo in results)
                {
                    for (int i = 0; i < resultItemInfo.Quantity; i++)
                    {
                        resultItemInfo.Item.AssignedStorage.RestoreClaimed(resultItemInfo.Item);
                    }
                }
                
                return null;
            }
        }
        
        public bool IsPossible()
        {
            var requiredMaterials = ClaimRequiredMaterials(ItemToCraft);
            if (requiredMaterials != null)
            {
                RequestedItemInfos = requiredMaterials;
                return true;
            }

            return false;
        }

        private List<RequestedItemInfo> AddToRequestedItemsList(Item item, List<RequestedItemInfo> currentList)
        {
            List<RequestedItemInfo> results = new List<RequestedItemInfo>(currentList);
            
            foreach (var itemInfo in results)
            {
                if (itemInfo.Item.Equals(item))
                {
                    itemInfo.Quantity++;
                    return results;
                }
            }
            
            results.Add(new RequestedItemInfo(item, 1));

            return results;
        }
        
        // public bool HasCorrectCraftingTable(ProductionBuildingOld buildingOld)
        // {
        //     var craftingTable = ItemToCraft.RequiredCraftingTable;
        //     if (craftingTable == null)
        //     {
        //         _assignedCraftingTable = null;
        //         return true;
        //     }
        //
        //     var result = buildingOld.GetFurniture(craftingTable) as CraftingTable;
        //     if (result != null && !result.IsInUse)
        //     {
        //         _assignedCraftingTable = result;
        //         return true;
        //     }
        //     else
        //     {
        //         _assignedCraftingTable = null;
        //         return false;
        //     }
        // }

        public Task CreateTask(CraftingTable craftingTable)
        {
            Task task = new Task("Craft Placed Item", Requestor)
            {
                Payload = craftingTable.UniqueId,
                TaskType = TaskType.Craft,
                //Materials = RequestedItemInfos,
            };
            
            return task;
        }

        // TODO: Get rid of this
        [Serializable]
        public class RequestedItemInfo
        {
            public Item Item;
            public int Quantity;

            public RequestedItemInfo(Item item, int quantity)
            {
                Item = item;
                Quantity = quantity;
            }
        }
    }
}
