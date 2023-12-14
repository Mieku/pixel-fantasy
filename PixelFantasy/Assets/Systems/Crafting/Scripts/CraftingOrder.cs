using System;
using System.Collections.Generic;
using Buildings;
using Items;
using Managers;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;

namespace Systems.Crafting.Scripts
{
    [Serializable]
    public class CraftingOrder
    {
        public CraftedItemData CraftedItem;
        public PlayerInteractable Requestor;
        public EOrderState State;
        public EOrderType OrderType;
        
        public Action OnOrderComplete;
        public Action OnOrderClaimed;
        public Action OnOrderCancelled;
        
        private List<ItemAmount> _remainingMaterials = new List<ItemAmount>();

        public enum EOrderType
        {
            Furniture,
        }

        public enum EOrderState
        {
            None,
            Queued,
            Claimed,
            Completed,
            Cancelled,
        }

        public CraftingOrder(CraftedItemData craftedItem, PlayerInteractable requestor, EOrderType orderType, Action onOrderClaimed, Action onOrderComplete,
            Action onOrderCancelled)
        {
            CraftedItem = craftedItem;
            Requestor = requestor;
            OrderType = orderType;
            OnOrderClaimed = onOrderClaimed;
            OnOrderComplete = onOrderComplete;
            OnOrderCancelled = onOrderCancelled;
            
            _remainingMaterials = CraftedItem.GetResourceCosts();
            
            SetOrderState(EOrderState.Queued);
        }

        public Task CreateTask(CraftingBuilding building, Action<Task> onTaskComplete)
        {
            List<Item> claimedMats = ClaimRequiredMaterials(building);
            if (claimedMats == null)
            {
                return null;
            }

            Task task;
            switch (OrderType)
            {
                case EOrderType.Furniture:
                    task = new Task("Craft Furniture Order", Requestor, building.GetBuildingJob())
                    {
                        Payload = CraftedItem.ItemName,
                        OnTaskComplete = onTaskComplete,
                        Materials = claimedMats,
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            State = EOrderState.Claimed;
            OnOrderClaimed.Invoke();
            return task;
        }

        private List<Item> ClaimRequiredMaterials(Building building)
        {
            var requiredItems = CraftedItem.GetResourceCosts();
            List<Item> claimedItems = new List<Item>();
            
            foreach (var requiredItem in requiredItems)
            {
                for (int i = 0; i < requiredItem.Quantity; i++)
                {
                    // Check building storage first, then check global
                    var claimedItem = InventoryManager.Instance.ClaimItemBuilding(requiredItem.Item, building);
                    if (claimedItem == null)
                    {
                        claimedItem = InventoryManager.Instance.ClaimItemGlobal(requiredItem.Item);
                    }

                    if (claimedItem == null)
                    {
                        // If for some reason the building can't get everything, unclaim all the materials and return null
                        foreach (var itemToUnclaim in claimedItems)
                        {
                            itemToUnclaim.UnclaimItem();
                        }

                        return null;
                    }
                    else
                    {
                        claimedItems.Add(claimedItem);
                    }
                }
            }

            return claimedItems;
        }

        public void ClaimOrder()
        {
            OnOrderClaimed.Invoke();
        }

        public bool CanBeCrafted(Building building)
        {
            if (CraftedItem.RequiredCraftingTable != null)
            {
                var craftingTable = building.GetAvailableFurniture(CraftedItem.RequiredCraftingTable);
                if (craftingTable == null)
                {
                    return false;
                }
            }

            if (!AreMaterialsAvailable())
            {
                return false;
            }

            return true;
        }
        
        public bool AreMaterialsAvailable()
        {
            foreach (var itemAmount in _remainingMaterials)
            {
                if (!itemAmount.CanAfford())
                {
                    return false;
                }
            }

            return true;
        }

        public void SetOrderState(EOrderState state)
        {
            if (State == state) return;
            State = state;
            
            switch (state)
            {
                case EOrderState.Queued:
                    Enter_Queued();
                    break;
                case EOrderState.Claimed:
                    Enter_Claimed();
                    break;
                case EOrderState.Completed:
                    Enter_Completed();
                    break;
                case EOrderState.Cancelled:
                    Enter_Cancelled();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void Enter_Queued()
        {
            CraftingOrdersManager.Instance.SubmitOrder(this);
        }

        private void Enter_Claimed()
        {
            
        }

        private void Enter_Completed()
        {
            OnOrderComplete?.Invoke();
        }
        
        private void Enter_Cancelled()
        {
            CraftingOrdersManager.Instance.CancelOrder(this);
            OnOrderCancelled?.Invoke();
        }

        private void OnTaskComplete(Task task)
        {
            SetOrderState(EOrderState.Completed);
        }
    }
}
