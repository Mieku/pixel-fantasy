using System;
using System.Collections.Generic;
using Buildings;
using Characters;
using Gods;
using Items;
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
        //public List<CraftingBill> AssociatedBills = new List<CraftingBill>(); // Bills needed to craft the items required for this item

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
                        // Does not have the item in storage yet, can it be made?
                        CraftedItemData craftedItemData = cost.Item as CraftedItemData;
                        if (craftedItemData != null)
                        {
                            var additionalItems = ClaimRequiredMaterials(craftedItemData);
                            if (additionalItems == null || additionalItems.Count == 0)
                            {
                                canGetAllItems = false;
                                break;
                            }
                            else
                            {
                                // Add the new items to the list
                                foreach (var additionalItem in additionalItems)
                                {
                                    for (int j = 0; j < additionalItem.Quantity; j++)
                                    {
                                        results = AddToRequestedItemsList(additionalItem.ItemData, additionalItem.Storage, results);
                                    }
                                }
                            }
                        }
                        else
                        {
                            canGetAllItems = false;
                            break;
                        }
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
                // Unclaim them
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

        public bool IsCorrectProfession(Profession profession)
        {
            return ItemToCraft.CraftersProfession == profession;
        }

        public bool HasCorrectCraftingTable(ProductionBuilding building)
        {
            var craftingTable = ItemToCraft.RequiredCraftingTable;
            if (craftingTable == null)
            {
                return true;
            }

            var result = building.GetFurniture(craftingTable);
            if (result != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Task CreateTask(ProductionBuilding building)
        {
            Debug.Log("Create Task was triggered!");
            return null;
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
