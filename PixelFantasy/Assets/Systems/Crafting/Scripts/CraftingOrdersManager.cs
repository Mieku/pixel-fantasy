using System;
using System.Collections.Generic;
using Buildings;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace Systems.Crafting.Scripts
{
    public class CraftingOrdersManager : Singleton<CraftingOrdersManager>
    {
        // Sort the orders by job
        [SerializeField] private List<CraftingOrderQueue> _queues = new List<CraftingOrderQueue>();

        public void SubmitOrder(CraftingOrder order)
        {
            CraftingOrderQueue queue = _queues.Find(q => q.CraftingTable == order.AssignedTable);
            if (queue == null)
            {
                queue = new CraftingOrderQueue { CraftingTable = order.AssignedTable };
                _queues.Add(queue);
            }
            queue.Orders.Add(order);
        }
        
        public CraftingOrder GetNextOrder(CraftingTable table)
        {
            CraftingOrderQueue queue = _queues.Find(q => q.CraftingTable == table);
            if (queue != null && queue.Orders.Count > 0)
            {
                CraftingOrder order = queue.Orders[0];
                queue.Orders.RemoveAt(0);
                return order;
            }
            return null;
        }

        public CraftingOrder GetNextCraftableOrder(CraftingTable table)
        {
            CraftingOrderQueue queue = _queues.Find(q => q.CraftingTable == table);
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
            return null;
        }
        
        public List<CraftingOrder> GetAllOrders(CraftingTable table)
        {
            CraftingOrderQueue queue = _queues.Find(q => q.CraftingTable == table);
            if (queue != null)
            {
                return queue.Orders;
            }

            return new List<CraftingOrder>();
        }
        
        public void CancelOrder(CraftingOrder order)
        {
            CraftingOrderQueue queue = _queues.Find(q => q.CraftingTable == order.AssignedTable);
            if (queue != null && queue.Orders.Contains(order))
            {
                queue.Orders.Remove(order);
            }
        }

        public void IncreaseOrderPriority(CraftingOrder order)
        {
            CraftingOrderQueue queue = _queues.Find(q => q.CraftingTable == order.AssignedTable);
            if (queue != null && queue.Orders.Contains(order))
            {
                int orderIndex = queue.Orders.IndexOf(order);
                orderIndex = Mathf.Clamp(orderIndex - 1, 0, queue.Orders.Count - 1);
                queue.Orders.Remove(order);
                queue.Orders.Insert(orderIndex, order);
            }
        }

        public void DecreaseOrderPriority(CraftingOrder order)
        {
            CraftingOrderQueue queue = _queues.Find(q => q.CraftingTable == order.AssignedTable);
            if (queue != null && queue.Orders.Contains(order))
            {
                int orderIndex = queue.Orders.IndexOf(order);
                orderIndex = Mathf.Clamp(orderIndex + 1, 0, queue.Orders.Count - 1);
                queue.Orders.Remove(order);
                queue.Orders.Insert(orderIndex, order);
            }
        }

        public bool IsFirstInQueue(CraftingOrder order)
        {
            CraftingOrderQueue queue = _queues.Find(q => q.CraftingTable == order.AssignedTable);
            if (queue != null && queue.Orders.Count > 0)
            {
                return queue.Orders[0] == order;
            }
            return false;
        }
        
        public bool IsLastInQueue(CraftingOrder order)
        {
            CraftingOrderQueue queue = _queues.Find(q => q.CraftingTable == order.AssignedTable);
            if (queue != null && queue.Orders.Count > 0)
            {
                return queue.Orders[^1] == order;
            }
            return false;
        }
    }

    [Serializable]
    public class CraftingOrderQueue
    {
        public CraftingTable CraftingTable;
        public List<CraftingOrder> Orders = new List<CraftingOrder>();
    }
}
