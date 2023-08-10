using System;
using System.Collections;
using System.Collections.Generic;
using Buildings;
using Items;
using Managers;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;

[Serializable]
public class ProductOrder
{
    public CraftedItemData craftedItemData;
    public bool isSuspended;
    public ProductionType productionType;
    public int valueSet;
    public int amountMade;
    public int amountInProgress;

    public ProductOrder(CraftedItemData craftedItemData)
    {
        this.craftedItemData = craftedItemData;
        isSuspended = false;
        productionType = ProductionType.Finite;
        valueSet = 1;
        amountMade = 0;
    }

    public Task CreateTask(CraftingTable craftingTable)
    {
        List<CraftingBill.RequestedItemInfo> claimedMats = ClaimRequiredMaterials(craftedItemData);
        
        Task task = new Task()
        {
            TaskId = "Craft Item",
            Requestor = craftingTable,
            Payload = craftedItemData.ItemName,
            TaskType = TaskType.Craft,
            OnTaskComplete = OnTaskCompleted,
            Materials = claimedMats,
            
        };
        amountInProgress++;
        return task;
    }
    
    public Task CreateTask(ProductionBuildingOld buildingOld)
    {
        List<CraftingBill.RequestedItemInfo> claimedMats = ClaimRequiredMaterials(craftedItemData);
        
        Task task = new Task()
        {
            TaskId = "Craft Item",
            Requestor = buildingOld,
            Payload = craftedItemData.ItemName,
            TaskType = TaskType.Craft,
            OnTaskComplete = OnTaskCompleted,
            Materials = claimedMats,
        };
        amountInProgress++;
        return task;
    }

    private void OnTaskCompleted(Task task)
    {
        amountMade++;
        amountInProgress--;
    }

    public enum ProductionType
    {
        Finite,
        MaintainAmount,
        Infinite,
    }
    
    private List<CraftingBill.RequestedItemInfo> ClaimRequiredMaterials(CraftedItemData itemData)
    {
        bool canGetAllItems = true;
        List<CraftingBill.RequestedItemInfo> results = new List<CraftingBill.RequestedItemInfo>();
            
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
    
    private List<CraftingBill.RequestedItemInfo> AddToRequestedItemsList(ItemData itemData, Storage storage, List<CraftingBill.RequestedItemInfo> currentList)
    {
        List<CraftingBill.RequestedItemInfo> results = new List<CraftingBill.RequestedItemInfo>(currentList);
            
        foreach (var itemInfo in results)
        {
            if (itemInfo.ItemData == itemData && itemInfo.Storage == storage)
            {
                itemInfo.Quantity++;
                return results;
            }
        }
            
        results.Add(new CraftingBill.RequestedItemInfo(itemData, storage, 1));

        return results;
    }

    /// <summary>
    /// Does not count in progress
    /// </summary>
    public int AmountLeftToMake()
    {
        switch (productionType)
        {
            case ProductionType.Finite:
                return valueSet - amountInProgress - amountMade;
            case ProductionType.MaintainAmount:
                int amountAvailable = InventoryManager.Instance.GetAmountAvailable(craftedItemData);
                return (valueSet + amountInProgress) - amountAvailable;
            case ProductionType.Infinite:
                return 100;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

[Serializable]
public class ProductOrderQueue
{
    private List<ProductOrder> _orders = new List<ProductOrder>();

    public List<ProductOrder> AllOrders => _orders;
    public int Count => _orders.Count;


    public Task RequestTask(CraftingTable craftingTable)
    {
        if (_orders.Count == 0) return null;
        
        foreach (var order in _orders)
        {
            if (!order.isSuspended)
            {
                if (order.AmountLeftToMake() > 0)
                {
                    if (order.craftedItemData.AreResourcesAvailable())
                    {
                        return order.CreateTask(craftingTable);
                    }
                }
            }
        }

        return null;
    }
    
    public Task RequestTask(ProductionBuildingOld buildingOld)
    {
        if (_orders.Count == 0) return null;

        foreach (var order in _orders)
        {
            if (!order.isSuspended)
            {
                if (order.AmountLeftToMake() > 0)
                {
                    if (order.craftedItemData.AreResourcesAvailable() &&
                        order.craftedItemData.CanBuildingCraftThis(buildingOld))
                    {
                        return order.CreateTask(buildingOld);
                    }
                }
            }
        }

        return null;
    }
    
    public void DeleteOrder(ProductOrder order)
    {
        _orders.Remove(order);
    }
    
    public void AddOrder(ProductOrder order)
    {
        _orders.Add(order);
    }

    public void IncreaseOrderPriority(ProductOrder order)
    {
        if (_orders.Contains(order))
        {
            int index = _orders.IndexOf(order);
            _orders.RemoveAt(index);
            index--;
            _orders.Insert(index, order);
        }
    }

    public void DecreaseOrderPriority(ProductOrder order)
    {
        if (_orders.Contains(order))
        {
            int index = _orders.IndexOf(order);
            _orders.RemoveAt(index);
            index++;
            _orders.Insert(index, order);
        }
    }
}
