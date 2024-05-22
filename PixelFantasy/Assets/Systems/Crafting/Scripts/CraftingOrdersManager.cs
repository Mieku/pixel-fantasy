using System;
using System.Collections.Generic;
using Data.Item;
using Managers;
using UnityEngine;

namespace Systems.Crafting.Scripts
{
    public class CraftingOrdersManager : Singleton<CraftingOrdersManager>
    {
        [SerializeField] private CraftingOrderQueue _queue = new CraftingOrderQueue();

        public MealSettings testerJam;
        public CraftedItemSettings testerGold;
        
        private void Update()
        {
            // TODO: For testing... Remove
            if (Input.GetKeyDown(KeyCode.O))
            {
                Debug.Log("Creating a cooking order");
                CraftingOrder cookingOrder = new CraftingOrder(testerJam, null, 
                    () =>
                    {
                        Debug.Log("Order was claimed");
                    },
                    () =>
                    {
                        Debug.Log("Order was completed");
                    }, () =>
                    {
                        Debug.Log("Order was cancelled");
                    });
                _queue.SubmitOrder(cookingOrder);
            }
            
            // TODO: For testing... Remove
            if (Input.GetKeyDown(KeyCode.P))
            {
                Debug.Log("Creating a crafting order");
                CraftingOrder cookingOrder = new CraftingOrder(testerGold, null, CraftingOrder.EOrderType.Item, 
                    () =>
                    {
                        Debug.Log("Order was claimed");
                    },
                    () =>
                    {
                        Debug.Log("Order was completed");
                    }, () =>
                    {
                        Debug.Log("Order was cancelled");
                    });
                _queue.SubmitOrder(cookingOrder);
            }
        }
        
        public List<CraftingOrder> GetAllOrders(CraftingTableData tableData)
        {
            return _queue.GetOrdersForTable(tableData);
        }
        
        public void SubmitOrder(CraftingOrder order)
        {
            _queue.SubmitOrder(order);
        }
        
        public CraftingOrder GetNextOrder(CraftingTableData tableData)
        {
            return _queue.GetNextOrder(tableData);
        }

        public CraftingOrder GetNextCraftableOrder(CraftingTableData tableData)
        {
            return _queue.GetNextCraftableOrder(tableData);
        }
        
        public void CancelOrder(CraftingOrder order)
        {
            _queue.CancelOrder(order);
        }
        
        public bool IsFirstInQueue(CraftingOrder order, CraftingTableData craftingTableData)
        {
            return _queue.IsFirstInQueue(order, craftingTableData);
        }
        
        public bool IsLastInQueue(CraftingOrder order, CraftingTableData craftingTableData)
        {
            return _queue.IsLastInQueue(order, craftingTableData);
        }

        public CraftingOrderQueue Queue => _queue;
    }

    [Serializable]
    public class CraftingOrderQueue
    {
        public List<CraftingOrder> Orders = new List<CraftingOrder>();
        
        public void SubmitOrder(CraftingOrder order)
        {
            Orders.Add(order);
        }
        
        public CraftingOrder GetNextOrder(CraftingTableData tableData)
        {
            foreach (CraftingOrder order in Orders)
            {
                if (tableData.CanCraftItem(order.CraftedItem) || tableData.CanCookMeal(order.CraftedMeal))
                {
                    int orderIndex = Orders.IndexOf(order);
                    Orders.RemoveAt(orderIndex);
                    return order;
                }
            }
            
            return null;
        }

        public CraftingOrder GetNextCraftableOrder(CraftingTableData tableData)
        {
            foreach (CraftingOrder order in Orders)
            {
                if (order.CanBeCrafted(tableData))
                {
                    int orderIndex = Orders.IndexOf(order);
                    Orders.RemoveAt(orderIndex);
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
