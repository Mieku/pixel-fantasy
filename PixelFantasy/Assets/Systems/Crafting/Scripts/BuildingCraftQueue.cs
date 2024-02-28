using System;
using System.Collections.Generic;
using Buildings;
using Items;
using UnityEngine;

namespace Systems.Crafting.Scripts
{
    [Serializable]
    public class BuildingCraftQueue
    {
        [SerializeField] private List<TableCraftingOrders> _queues = new List<TableCraftingOrders>();

        public void SubmitOrder(CraftingOrder order, CraftingTable table)
        {
            TableCraftingOrders queue = _queues.Find(o => o.AssignedTable == table);
            if (queue == null)
            {
                queue = new TableCraftingOrders
                    { AssignedTable = table, Orders = new List<CraftingOrder> { order } };
                _queues.Add(queue);
            }
            else
            {
                queue.Orders.Add(order);
            }
        }

        public CraftingOrder GetNextOrder(Building building)
        {
            var tables = building.CraftingTables;
            foreach (var table in tables)
            {
                TableCraftingOrders queue = _queues.Find(o => o.AssignedTable == table);
                if (queue != null && queue.Orders.Count > 0)
                {
                    CraftingOrder order = queue.Orders[0];
                    queue.Orders.RemoveAt(0);
                    return order;
                }
            }
           

            return null;
        }

        public CraftingOrder GetNextCraftableOrder(Building building)
        {
            var tables = building.CraftingTables;
            foreach (var table in tables)
            {
                TableCraftingOrders queue = _queues.Find(o => o.AssignedTable == table);
                if (queue != null)
                {
                    foreach (CraftingOrder order in queue.Orders)
                    {
                        if (order.CanBeCrafted())
                        {
                            int orderIndex = queue.Orders.IndexOf(order);
                            queue.Orders.RemoveAt(orderIndex);
                            return order;
                        }
                    }
                }
            }

            return null;
        }

        public List<CraftingOrder> GetAllOrders(Building building)
        {
            List<CraftingOrder> results = new List<CraftingOrder>();
            var tables = building.CraftingTables;
            foreach (var table in tables)
            {
                TableCraftingOrders queue = _queues.Find(o => o.AssignedTable == table);
                if (queue != null)
                {
                    results.AddRange(queue.Orders);
                }
            }

            return results;
        }

        public void CancelOrder(CraftingOrder order)
        {
            TableCraftingOrders queue = _queues.Find(o => o.Orders.Contains(order));
            if (queue != null)
            {
                queue.Orders.Remove(order);
            }
        }

        public void IncreaseOrderPriority(CraftingOrder order)
        {
            TableCraftingOrders queue = _queues.Find(o => o.Orders.Contains(order));
            if (queue != null)
            {
                int orderIndex = queue.Orders.IndexOf(order);
                orderIndex = Mathf.Clamp(orderIndex - 1, 0, queue.Orders.Count - 1);
                queue.Orders.Remove(order);
                queue.Orders.Insert(orderIndex, order);
            }
        }

        public void DecreaseOrderPriority(CraftingOrder order)
        {
            TableCraftingOrders queue = _queues.Find(o => o.Orders.Contains(order));
            if (queue != null)
            {
                int orderIndex = queue.Orders.IndexOf(order);
                orderIndex = Mathf.Clamp(orderIndex + 1, 0, queue.Orders.Count - 1);
                queue.Orders.Remove(order);
                queue.Orders.Insert(orderIndex, order);
            }
        }

        public bool IsFirstInQueue(CraftingOrder order)
        {
            TableCraftingOrders queue = _queues.Find(o => o.Orders.Contains(order));
            if (queue != null && queue.Orders.Count > 0)
            {
                return queue.Orders[0] == order;
            }

            return false;
        }

        public bool IsLastInQueue(CraftingOrder order)
        {
            TableCraftingOrders queue = _queues.Find(o => o.Orders.Contains(order));
            if (queue != null && queue.Orders.Count > 0)
            {
                return queue.Orders[^1] == order;
            }

            return false;
        }
    }

    [Serializable]
    public class TableCraftingOrders
    {
        public CraftingTable AssignedTable;
        public List<CraftingOrder> Orders = new List<CraftingOrder>();
    }
}