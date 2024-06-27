using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.Crafting.Scripts
{
    [Serializable]
    public class CraftingOrderQueue
    {
        public List<CraftingOrder> Orders = new List<CraftingOrder>();
        
        public void SubmitOrder(CraftingOrder order)
        {
            Orders.Add(order);
        }

        public CraftingOrder GetNextCraftableOrder(CraftingTableData tableData)
        {
            foreach (CraftingOrder order in Orders)
            {
                if (order.CanBeCrafted(tableData))
                {
                    return order;
                }
            }
            
            return null;
        }
        
        public void CancelOrder(CraftingOrder order)
        {
            if (Orders.Contains(order))
            {
                Orders.Remove(order);
            }

            order.CancelOrder();
        }

        public void IncreaseOrderPriority(CraftingOrder order)
        {
            if (Orders.Contains(order))
            {
                int orderIndex = Orders.IndexOf(order);
                orderIndex = Mathf.Clamp(orderIndex - 1, 0, Orders.Count - 1);
                Orders.Remove(order);
                Orders.Insert(orderIndex, order);
            }
        }

        public void DecreaseOrderPriority(CraftingOrder order)
        {
            if (Orders.Contains(order))
            {
                int orderIndex = Orders.IndexOf(order);
                orderIndex = Mathf.Clamp(orderIndex + 1, 0, Orders.Count - 1);
                Orders.Remove(order);
                Orders.Insert(orderIndex, order);
            }
        }

        public bool IsFirstInQueue(CraftingOrder order, CraftingTableData tableData = null)
        {
            if (tableData == null)
            {
                if (Orders.Count > 0)
                {
                    return Orders[0] == order;
                }
                return false;
            }

            var ordersForTable = GetOrdersForTable(tableData);
            if (ordersForTable.Count > 0)
            {
                return ordersForTable[0] == order;
            }

            return false;
        }
        
        public bool IsLastInQueue(CraftingOrder order, CraftingTableData tableData = null)
        {
            if (tableData == null)
            {
                if (Orders.Count > 0)
                {
                    return Orders[^1] == order;
                }
                return false;
            }
            
            var ordersForTable = GetOrdersForTable(tableData);
            if (ordersForTable.Count > 0)
            {
                return ordersForTable[^1] == order;
            }

            return false;
        }

        public List<CraftingOrder> GetOrdersForTable(CraftingTableData tableData)
        {
            List<CraftingOrder> results = new List<CraftingOrder>();

            foreach (var order in Orders)
            {
                if (tableData.CanCraftItem(order.CraftedItem) || tableData.CanCookMeal(order.CraftedMeal))
                {
                    results.Add(order);
                }
            }

            return results;
        }
    }
}
