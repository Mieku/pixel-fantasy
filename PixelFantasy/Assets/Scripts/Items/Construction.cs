using System.Collections.Generic;
using Actions;
using DataPersistence;
using Gods;
using Interfaces;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    public class Construction : Interactable, IPersistent, IClickableObject
    {
        [SerializeField] protected ActionTakeResourceToBlueprint _takeResourceToBlueprintAction;
        [SerializeField] protected ActionConstructStructure _constructStructureAction;
    
        protected List<ItemAmount> _remainingResourceCosts;
        protected List<ItemAmount> _pendingResourceCosts; // Claimed by a task but not used yet
        protected List<ItemAmount> _incomingResourceCosts; // The item is on its way
        protected List<Item> _incomingItems = new List<Item>();
    
        protected bool _isBuilt;
    
        protected TaskMaster taskMaster => TaskMaster.Instance;

        public void AddResourceToBlueprint(ItemData itemData)
        {
            RemoveFromPendingResourceCosts(itemData);
            
            foreach (var cost in _remainingResourceCosts)
            {
                if (cost.Item == itemData && cost.Quantity > 0)
                {
                    cost.Quantity--;
                    if (cost.Quantity <= 0)
                    {
                        _remainingResourceCosts.Remove(cost);
                    }

                    return;
                }
            }
        }
        
        public virtual float GetWorkPerResource()
        {
            return 0;
        }
        
        public virtual void CompleteConstruction()
        {
        
        }
        
        public void AddToIncomingItems(Item item)
        {
            _incomingItems ??= new List<Item>();
            _incomingItems.Add(item);
            
            _incomingResourceCosts ??= new List<ItemAmount>();

            foreach (var cost in _incomingResourceCosts)
            {
                if (cost.Item == item.GetItemData())
                {
                    cost.Quantity += 1;
                    return;
                }
            }
            
            _incomingResourceCosts.Add(new ItemAmount
            {
                Item = item.GetItemData(),
                Quantity = 1
            });
        }
        
        public void RemoveFromIncomingItems(Item item)
        {
            _incomingItems ??= new List<Item>();
            _incomingItems.Remove(item);
            
            foreach (var cost in _incomingResourceCosts)
            {
                if (cost.Item == item.GetItemData())
                {
                    cost.Quantity -= 1;
                    if (cost.Quantity <= 0)
                    {
                        _incomingResourceCosts.Remove(cost);
                    }

                    return;
                }
            }
        }
        
        public void AddToPendingResourceCosts(ItemData itemData, int quantity = 1)
        {
            _pendingResourceCosts ??= new List<ItemAmount>();

            foreach (var cost in _pendingResourceCosts)
            {
                if (cost.Item == itemData)
                {
                    cost.Quantity += quantity;
                    return;
                }
            }
            
            _pendingResourceCosts.Add(new ItemAmount
            {
                Item = itemData,
                Quantity = quantity
            });
        }

        public void RemoveFromPendingResourceCosts(ItemData itemData, int quantity = 1)
        {
            foreach (var cost in _pendingResourceCosts)
            {
                if (cost.Item == itemData)
                {
                    cost.Quantity -= quantity;
                    if (cost.Quantity <= 0)
                    {
                        _pendingResourceCosts.Remove(cost);
                    }

                    return;
                }
            }
        }

        protected List<ItemAmount> GetRemainingMissingItems()
        {
            _pendingResourceCosts ??= new List<ItemAmount>();
            _incomingResourceCosts ??= new List<ItemAmount>();
            List<ItemAmount> result = new List<ItemAmount>();

            foreach (var remainingResourceCost in _remainingResourceCosts)
            {
                ItemAmount amount = new ItemAmount
                {
                    Item = remainingResourceCost.Item,
                    Quantity = remainingResourceCost.Quantity,
                };
                
                result.Add(amount);
            }
            
            foreach (var pendingCost in _pendingResourceCosts)
            {
                foreach (var resultCost in result)
                {
                    if (resultCost.Item == pendingCost.Item)
                    {
                        resultCost.Quantity -= pendingCost.Quantity;
                        if (resultCost.Quantity <= 0)
                        {
                            result.Remove(resultCost);
                        }
                    }
                }
            }

            foreach (var incomingCost in _incomingResourceCosts)
            {
                foreach (var resultCost in result)
                {
                    if (resultCost.Item == incomingCost.Item)
                    {
                        resultCost.Quantity -= incomingCost.Quantity;
                        if (resultCost.Quantity <= 0)
                        {
                            result.Remove(resultCost);
                        }
                    }
                }
            }
            
            return result;
        }
        
        public void CheckIfAllResourcesLoaded()
        {
            if (_isBuilt) return;
            
            if (_remainingResourceCosts.Count == 0)
            {
                CreateConstructTask();
            }
        }
        
        public void CreateConstructTask(bool autoAssign = true)
        {
            _constructStructureAction.CreateTask(this, autoAssign);
        }
    
        public virtual object CaptureState()
        {
            throw new System.NotImplementedException();
        }

        public virtual void RestoreState(object data)
        {
            throw new System.NotImplementedException();
        }

        public ClickObject GetClickObject()
        {
            throw new System.NotImplementedException();
        }

        public bool IsClickDisabled { get; set; }
        public bool IsAllowed { get; set; }
        public void ToggleAllowed(bool isAllowed)
        {
            throw new System.NotImplementedException();
        }

        public List<Order> GetOrders()
        {
            throw new System.NotImplementedException();
        }

        public virtual List<ActionBase> GetActions()
        {
            throw new System.NotImplementedException();
        }

        public void AssignOrder(ActionBase orderToAssign)
        {
            throw new System.NotImplementedException();
        }

        public bool IsOrderActive(Order order)
        {
            throw new System.NotImplementedException();
        }
    }
}
