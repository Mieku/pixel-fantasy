using System;
using System.Collections.Generic;
using Data.Item;
using Items;
using Managers;
using UnityEngine;

namespace Systems.Crafting.Scripts
{
    public class CraftingOrdersManager : Singleton<CraftingOrdersManager>
    {
        // Sort the orders by job
        [SerializeField] private CraftingOrderQueue _queue = new CraftingOrderQueue();

        public MealSettings testerJam;
        public CraftedItemDataSettings testerGold;
        
        private void Update()
        {
            // TODO: For testing... Remove
            if (Input.GetKeyDown(KeyCode.O))
            {
                Debug.Log("Creating a cooking order");
                CraftingOrder cookingOrder = new CraftingOrder(testerJam, null, true, 
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
            }
            
            // TODO: For testing... Remove
            if (Input.GetKeyDown(KeyCode.P))
            {
                Debug.Log("Creating a crafting order");
                CraftingOrder cookingOrder = new CraftingOrder(testerGold, null, CraftingOrder.EOrderType.Item, true, 
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
            }
        }

        public void SubmitOrder(CraftingOrder order)
        {
            _queue.Orders.Add(order);
        }
        
        public CraftingOrder GetNextOrder(CraftingTable table)
        {
            if (_queue.Orders.Count > 0)
            {
                CraftingOrder order = _queue.Orders[0];
                _queue.Orders.RemoveAt(0);
                return order;
            }
            return null;
        }

        public CraftingOrder GetNextCraftableOrder(CraftingTable table)
        {
            foreach (CraftingOrder order in _queue.Orders)
            {
                if (order.CanBeCrafted(table))
                {
                    int orderIndex = _queue.Orders.IndexOf(order);
                    _queue.Orders.RemoveAt(orderIndex);
                    return order;
                }
            }
            
            return null;
        }
        
        public List<CraftingOrder> GetAllOrders(CraftingTable table)
        {
            return new List<CraftingOrder>(_queue.Orders);
        }
        
        public void CancelOrder(CraftingOrder order)
        {
            if (_queue.Orders.Contains(order))
            {
                _queue.Orders.Remove(order);
            }
        }

        public void IncreaseOrderPriority(CraftingOrder order)
        {
            if (_queue.Orders.Contains(order))
            {
                int orderIndex = _queue.Orders.IndexOf(order);
                orderIndex = Mathf.Clamp(orderIndex - 1, 0, _queue.Orders.Count - 1);
                _queue.Orders.Remove(order);
                _queue.Orders.Insert(orderIndex, order);
            }
        }

        public void DecreaseOrderPriority(CraftingOrder order)
        {
            if (_queue.Orders.Contains(order))
            {
                int orderIndex = _queue.Orders.IndexOf(order);
                orderIndex = Mathf.Clamp(orderIndex + 1, 0, _queue.Orders.Count - 1);
                _queue.Orders.Remove(order);
                _queue.Orders.Insert(orderIndex, order);
            }
        }

        public bool IsFirstInQueue(CraftingOrder order)
        {
            if (_queue.Orders.Count > 0)
            {
                return _queue.Orders[0] == order;
            }
            return false;
        }
        
        public bool IsLastInQueue(CraftingOrder order)
        {
            if (_queue.Orders.Count > 0)
            {
                return _queue.Orders[^1] == order;
            }
            return false;
        }
    }

    [Serializable]
    public class CraftingOrderQueue
    {
        public List<CraftingOrder> Orders = new List<CraftingOrder>();
    }
}
