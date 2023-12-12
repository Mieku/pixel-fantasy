using System;
using System.Collections.Generic;
using Buildings;
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
            CraftingOrderQueue queue = _queues.Find(q => q.Job == order.CraftedItem.RequiredCraftingJob);
            if (queue == null)
            {
                queue = new CraftingOrderQueue { Job = order.CraftedItem.RequiredCraftingJob };
                _queues.Add(queue);
            }
            queue.Orders.Add(order);
        }
        
        public CraftingOrder GetNextOrder(JobData job)
        {
            CraftingOrderQueue queue = _queues.Find(q => q.Job == job);
            if (queue != null && queue.Orders.Count > 0)
            {
                CraftingOrder order = queue.Orders[0];
                queue.Orders.RemoveAt(0);
                return order;
            }
            return null;
        }

        public CraftingOrder GetNextCraftableOrder(JobData job, Building building)
        {
            CraftingOrderQueue queue = _queues.Find(q => q.Job == job);
            if (queue != null)
            {
                foreach (CraftingOrder order in queue.Orders)
                {
                    if (order.CanBeCrafted(building))
                    {
                        queue.Orders.RemoveAt(0);
                        return order;
                    }
                }
            }
            return null;
        }
        
        public List<CraftingOrder> GetAllOrders(JobData job)
        {
            CraftingOrderQueue queue = _queues.Find(q => q.Job == job);
            if (queue != null)
            {
                return queue.Orders;
            }
            return null;
        }
        
        public void CancelOrder(CraftingOrder order)
        {
            CraftingOrderQueue queue = _queues.Find(q => q.Job == order.CraftedItem.RequiredCraftingJob);
            if (queue != null && queue.Orders.Contains(order))
            {
                queue.Orders.Remove(order);
            }
        }
    }

    [Serializable]
    public class CraftingOrderQueue
    {
        public JobData Job;
        public List<CraftingOrder> Orders = new List<CraftingOrder>();
    }
}
