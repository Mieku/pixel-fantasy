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
        public CraftedItemSettings CraftedItem;
        public PlayerInteractable Requestor;
        public CraftingTable AssignedTable;
        public EOrderState State;
        public EOrderType OrderType;
        public bool IsGlobal;
        
        public Action OnOrderComplete;
        public Action OnOrderClaimed;
        public Action OnOrderCancelled;
        
        private List<ItemAmount> _remainingMaterials = new List<ItemAmount>();

        public enum EOrderType
        {
            Furniture,
            Item,
        }

        public enum EOrderState
        {
            None,
            Queued,
            Claimed,
            Completed,
            Cancelled,
        }

        public CraftingOrder(CraftedItemSettings craftedItem, PlayerInteractable requestor, EOrderType orderType, bool isGlobal, Action onOrderClaimed, Action onOrderComplete,
            Action onOrderCancelled)
        {
            CraftedItem = craftedItem;
            Requestor = requestor;
            OrderType = orderType;
            IsGlobal = isGlobal;
            OnOrderClaimed = onOrderClaimed;
            OnOrderComplete = onOrderComplete;
            OnOrderCancelled = onOrderCancelled;
            
            _remainingMaterials = CraftedItem.CraftRequirements.GetResourceCosts();
            
            SetOrderState(EOrderState.Queued);
        }

        public Task CreateTask(Action<Task> onTaskComplete)
        {
            List<Item> claimedMats = ClaimRequiredMaterials();
            if (claimedMats == null)
            {
                return null;
            }

            Task task;
            switch (OrderType)
            {
                case EOrderType.Furniture:
                    task = new Task("Craft Furniture Order", CraftedItem.CraftRequirements.CraftingSkill, Requestor, CraftedItem.CraftRequirements.RequiredCraftingToolType)
                    {
                        Payload = CraftedItem.ItemName,
                        OnTaskComplete = onTaskComplete,
                        Materials = claimedMats,
                    };
                    break;
                case EOrderType.Item:
                    task = new Task("Craft Item", CraftedItem.CraftRequirements.CraftingSkill, Requestor, CraftedItem.CraftRequirements.RequiredCraftingToolType)
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
            OnOrderClaimed?.Invoke();
            return task;
        }

        private List<Item> ClaimRequiredMaterials()
        {
            var requiredItems = CraftedItem.CraftRequirements.GetResourceCosts();
            List<Item> claimedItems = new List<Item>();
            
            foreach (var requiredItem in requiredItems)
            {
                for (int i = 0; i < requiredItem.Quantity; i++)
                {
                    // Check building storage first, then check global
                    var claimedItem = InventoryManager.Instance.ClaimItem(requiredItem.Item);

                    if (claimedItem == null)
                    {
                        // If for some reason they can't get everything, unclaim all the materials and return null
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
            OnOrderClaimed?.Invoke();
        }

        public bool CanBeCrafted(CraftingTable table)
        {
            // if (CraftedItem.RequiredCraftingTableOptions.Count > 0)
            // {
            //     var hasCraftingTable = building.ContainsCraftingTableForItem(CraftedItem);
            //     if (!hasCraftingTable)
            //     {
            //         return false;
            //     }
            // }

            return table.TableData.CanCraftItem(CraftedItem);
            //
            // if (!AreMaterialsAvailable())
            // {
            //     return false;
            // }
            //
            // return true;
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
            if (IsGlobal)
            {
                CraftingOrdersManager.Instance.SubmitOrder(this);
            }
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
            if (IsGlobal)
            {
                CraftingOrdersManager.Instance.CancelOrder(this);
            }
            
            OnOrderCancelled?.Invoke();
        }

        private void OnTaskComplete(Task task)
        {
            SetOrderState(EOrderState.Completed);
        }
    }
}
