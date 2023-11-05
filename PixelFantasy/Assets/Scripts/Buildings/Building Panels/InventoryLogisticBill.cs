using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;

namespace Buildings.Building_Panels
{
    [Serializable]
    public class InventoryLogisticBill
    {
        public LogisticType Type;
        public ItemData Item;
        public int Value;
        public Building Building;

        private RequestType _requestType;

        public InventoryLogisticBill(LogisticType type, ItemData item, int value, Building building)
        {
            Type = type;
            Item = item;
            Value = value;
            Building = building;
        }

        public void CheckBill(Building building)
        {
            if (Item == null) return;
            
            int amountStored = 0;
            if (building.GetBuildingInventory().ContainsKey(Item))
            {
                amountStored = building.GetBuildingInventory()[Item].Count;
            }

            switch (Type)
            {
                case LogisticType.Maintain:
                    CheckMaintain(amountStored);
                    break;
                case LogisticType.StoreMin:
                    CheckStoreMin(amountStored);
                    break;
                case LogisticType.StoreMax:
                    CheckStoreMax(amountStored);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }

        private void RequestMore()
        {
            if (_requestType == RequestType.More) return; // Don't request the same multiple times
            _requestType = RequestType.More;
            
            Task task = new Task("Stock Item", Building)
            {
                TaskType = TaskType.Haul,
                Payload = Item.ItemName,
                OnTaskComplete = RequestDone,
            };
            
            TaskManager.Instance.AddTask(task);
        }

        private void RequestLess()
        {
            if (_requestType == RequestType.Less) return; // Don't request the same multiple times
            _requestType = RequestType.Less;
            
            Task task = new Task("Unstock Item", Building)
            {
                TaskType = TaskType.Haul,
                Payload = Item.ItemName,
                OnTaskComplete = RequestDone,
            };
            
            TaskManager.Instance.AddTask(task);
        }

        private void RequestDone(Task task)
        {
            _requestType = RequestType.None;
        }

        private void CheckMaintain(int storedAmount)
        {
            if (storedAmount < Value)
            {
                RequestMore();
            } 
            else if (storedAmount > Value)
            {
                RequestLess();
            }
        }

        private void CheckStoreMin(int storedAmount)
        {
            if (storedAmount < Value)
            {
                RequestMore();
            }
        }

        private void CheckStoreMax(int storedAmount)
        {
            if (storedAmount > Value)
            {
                RequestLess();
            }
        }

        public string Suffix
        {
            get
            {
                switch (Type)
                {
                    case LogisticType.Maintain:
                        return $"/{Value}";
                    case LogisticType.StoreMax:
                        return $"<{Value}";
                    case LogisticType.StoreMin:
                        return $">{Value}";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public bool IsEqualTo(InventoryLogisticBill otherBill)
        {
            return  Type == otherBill.Type &&
                    Item == otherBill.Item &&
                    Value == otherBill.Value &&
                    Building == otherBill.Building;
        }
        
        public enum LogisticType
        {
            Maintain = 0,
            StoreMin = 1,
            StoreMax = 2,
        }

        public enum RequestType
        {
            None,
            More,
            Less,
        }
    }
}
