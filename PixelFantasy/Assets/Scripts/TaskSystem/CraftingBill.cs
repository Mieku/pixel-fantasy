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
        public Interactable Requestor;
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
                    var storage = InventoryManager.Instance.ClaimItem(cost.Item);
                    if (storage != null)
                    {
                        results = AddToRequestedItemsList(cost.Item, storage, results);
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
                        resultItemInfo.Storage.RestoreClaimed(resultItemInfo.ItemData, 1);
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

        private List<RequestedItemInfo> AddToRequestedItemsList(ItemData itemData, Storage storage, List<RequestedItemInfo> currentList)
        {
            List<RequestedItemInfo> results = new List<RequestedItemInfo>(currentList);
            
            foreach (var itemInfo in results)
            {
                if (itemInfo.ItemData == itemData && itemInfo.Storage == storage)
                {
                    itemInfo.Quantity++;
                    return results;
                }
            }
            
            results.Add(new RequestedItemInfo(itemData, storage, 1));

            return results;
        }

        public bool IsCorrectProfession(ProfessionData profession)
        {
            return ItemToCraft.CraftersProfession == profession;
        }

        public bool HasCorrectCraftingTable(ProductionBuilding building)
        {
            var craftingTable = ItemToCraft.RequiredCraftingTable;
            if (craftingTable == null)
            {
                _assignedCraftingTable = null;
                return true;
            }

            var result = building.GetFurniture(craftingTable) as CraftingTable;
            if (result != null && !result.IsInUse)
            {
                _assignedCraftingTable = result;
                return true;
            }
            else
            {
                _assignedCraftingTable = null;
                return false;
            }
        }

        public Task CreateTask()
        {
            Task task = new Task()
            {
                TaskId = "Craft Placed Item",
                Requestor = Requestor,
                Payload = _assignedCraftingTable.UniqueId,
                Profession = ItemToCraft.CraftersProfession,
                Materials = RequestedItemInfos,
            };
            
            return task;
        }

        [Serializable]
        public class RequestedItemInfo
        {
            public ItemData ItemData;
            public Storage Storage;
            public int Quantity;

            public RequestedItemInfo(ItemData itemData, Storage storage, int quantity)
            {
                ItemData = itemData;
                Storage = storage;
                Quantity = quantity;
            }
        }
    }
}
