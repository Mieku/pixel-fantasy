using System;
using System.Collections.Generic;
using Data.Item;
using Items;
using Managers;
using ScriptableObjects;
using TaskSystem;

namespace Systems.Crafting.Scripts
{
    [Serializable]
    public class CraftingOrder
    {
        public CraftedItemData CraftedItem;
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
        
        public CraftingOrder(CraftedItemData itemToCraft, PlayerInteractable requestor, EOrderType orderType, bool isGlobal, Action onOrderClaimed, Action onOrderComplete,
            Action onOrderCancelled)
        {
            CraftedItem = itemToCraft;
            OrderType = orderType;
            IsGlobal = isGlobal;
            OnOrderClaimed = onOrderClaimed;
            OnOrderComplete = onOrderComplete;
            OnOrderCancelled = onOrderCancelled;
            
            _remainingMaterials = itemToCraft.CraftedItemDataSettings.CraftRequirements.GetMaterialCosts();
            
            SetOrderState(EOrderState.Queued);
        }
        
        public Task CreateTask(Action<Task> onTaskComplete, CraftingTable table)
        {
            List<ItemData> claimedMats = ClaimRequiredMaterials();
            if (claimedMats == null)
            {
                return null;
            }

            Task task;
            switch (OrderType)
            {
                case EOrderType.Furniture:
                    task = new Task("Craft Furniture Order", CraftedItem.CraftedItemDataSettings.CraftRequirements.CraftingSkill, table, CraftedItem.CraftedItemDataSettings.CraftRequirements.RequiredCraftingToolType)
                    {
                        Payload = CraftedItem,
                        OnTaskComplete = onTaskComplete,
                        Materials = claimedMats,
                    };
                    break;
                case EOrderType.Item:
                    task = new Task("Craft Item", CraftedItem.CraftedItemDataSettings.CraftRequirements.CraftingSkill, table, CraftedItem.CraftedItemDataSettings.CraftRequirements.RequiredCraftingToolType)
                    {
                        Payload = CraftedItem,
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

        private List<ItemData> ClaimRequiredMaterials()
        {
            var requiredItems = CraftedItem.CraftedItemDataSettings.CraftRequirements.GetMaterialCosts();
            List<ItemData> claimedItems = new List<ItemData>();
            
            foreach (var requiredItem in requiredItems)
            {
                for (int i = 0; i < requiredItem.Quantity; i++)
                {
                    var claimedItem = InventoryManager.Instance.GetItemOfType(requiredItem.Item);

                    claimedItem.ClaimItem();
                    claimedItems.Add(claimedItem);
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
            return table.RuntimeTableData.CanCraftItem(CraftedItem.CraftedItemDataSettings);
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
