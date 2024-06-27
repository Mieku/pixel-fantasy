using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using Managers;
using TaskSystem;

namespace Systems.Crafting.Scripts
{
    [Serializable]
    public class CraftingOrder
    {
        public CraftedItemSettings CraftedItem;
        public MealSettings CraftedMeal;
        public PlayerInteractable Requestor;
        public EOrderState State;
        public EOrderType OrderType;
        
        public float RemainingCraftingWork;
        public float TotalCraftingWork;
        public List<ItemData> ClaimedItems = new List<ItemData>();

        public EFulfillmentType FulfillmentType;
        public int Amount;
        public Task CraftingTask;
        public bool IsPaused;
        
        public Action OnOrderComplete;
        public Action OnOrderClaimed;
        public Action OnOrderCancelled;
        
        private List<ItemAmount> _remainingMaterials = new List<ItemAmount>();
        private List<Ingredient> _remainingIngredients = new List<Ingredient>();
        
        public enum EOrderType
        {
            Item,
            Meal,
        }

        public enum EOrderState
        {
            None,
            Queued,
            Claimed,
            Completed,
            Cancelled,
        }

        public enum EFulfillmentType
        {
            Amount,
            Until,
            Forever,
        }
        
        public CraftingOrder(CraftedItemSettings itemToCraft, PlayerInteractable requestor, Action onOrderClaimed = null, Action onOrderComplete = null,
            Action onOrderCancelled = null)
        {
            CraftedItem = itemToCraft;
            OrderType = EOrderType.Item;
            Requestor = requestor;
            OnOrderClaimed = onOrderClaimed;
            OnOrderComplete = onOrderComplete;
            OnOrderCancelled = onOrderCancelled;
            FulfillmentType = EFulfillmentType.Amount;
            Amount = 1;
            IsPaused = false;

            RefreshOrderRequirements();
        }
        
        // For Meals
        public CraftingOrder(MealSettings mealToCraft, PlayerInteractable requestor, Action onOrderClaimed = null, Action onOrderComplete = null,
            Action onOrderCancelled = null)
        {
            CraftedMeal = mealToCraft;
            OrderType = EOrderType.Meal;
            Requestor = requestor;
            OnOrderClaimed = onOrderClaimed;
            OnOrderComplete = onOrderComplete;
            OnOrderCancelled = onOrderCancelled;
            FulfillmentType = EFulfillmentType.Amount;
            Amount = 1;
            IsPaused = false;
            
            RefreshOrderRequirements();
        }

        public void RefreshOrderRequirements()
        {
            switch (OrderType)
            {
                case EOrderType.Item:
                    TotalCraftingWork = CraftedItem.CraftRequirements.WorkCost;
                    RemainingCraftingWork = CraftedItem.CraftRequirements.WorkCost;
                    _remainingMaterials = CraftedItem.CraftRequirements.GetMaterialCosts();
                    break;
                case EOrderType.Meal:
                    TotalCraftingWork = CraftedMeal.MealRequirements.WorkCost;
                    RemainingCraftingWork = CraftedMeal.MealRequirements.WorkCost;
                    _remainingIngredients = CraftedMeal.MealRequirements.GetIngredients();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public Task CreateTask(Action<Task> onTaskComplete, Action onTaskCancelled, CraftingTable table)
        {
            List<ItemData> claimedMats = null;

            if (OrderType == EOrderType.Meal)
            {
                claimedMats = ClaimRequiredIngredients();
            }
            else
            {
                claimedMats = ClaimRequiredMaterials();
            }
            
            if (claimedMats == null)
            {
                return null;
            }
            
            switch (OrderType)
            {
                case EOrderType.Item:
                    CraftingTask = new Task("Craft Item", CraftedItem.CraftRequirements.CraftingSkill, table, CraftedItem.CraftRequirements.RequiredCraftingToolType)
                    {
                        Payload = CraftedItem,
                        OnTaskComplete = onTaskComplete,
                        OnTaskCancel = onTaskCancelled,
                        Materials = claimedMats,
                    };
                    break;
                case EOrderType.Meal:
                    CraftingTask = new Task("Cook Meal", CraftedMeal.MealRequirements.CraftingSkill, table,
                        CraftedMeal.MealRequirements.RequiredCraftingToolType)
                    {
                        Payload = CraftedMeal,
                        OnTaskComplete = onTaskComplete,
                        OnTaskCancel = onTaskCancelled,
                        Materials = claimedMats,
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //State = EOrderState.Claimed;
            OnOrderClaimed?.Invoke();
            return CraftingTask;
        }

        public bool IsOrderFulfilled()
        {
            switch (FulfillmentType)
            {
                case EFulfillmentType.Amount:
                    return Amount == 0;
                case EFulfillmentType.Until:
                    int amountAvailable;
                    if (OrderType == EOrderType.Meal)
                    {
                        amountAvailable = InventoryManager.Instance.GetAmountAvailable(CraftedMeal);
                    }
                    else
                    {
                        amountAvailable = InventoryManager.Instance.GetAmountAvailable(CraftedItem);
                    }
                    
                    return amountAvailable >= Amount;
                case EFulfillmentType.Forever:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private List<ItemData> ClaimRequiredMaterials()
        {
            var requiredItems = CraftedItem.CraftRequirements.GetMaterialCosts();
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

        private List<ItemData> ClaimRequiredIngredients()
        {
            var ingredients = CraftedMeal.MealRequirements.GetIngredients();
            List<ItemData> claimedItems = new List<ItemData>();

            foreach (var ingredient in ingredients)
            {
                for (int i = 0; i < ingredient.Amount; i++)
                {
                    ItemData claimedItem = InventoryManager.Instance.GetFoodItemOfType(ingredient.FoodType);
                    
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

        public void CancelOrder()
        {
            foreach (var claimedItem in ClaimedItems)
            {
                claimedItem.UnclaimItem();
            }
            ClaimedItems.Clear();
            
            if (CraftingTask != null)
            {
                CraftingTask.Cancel();
            }
            
            OnOrderCancelled?.Invoke();
        }

        public bool CanBeCrafted(CraftingTableData tableData)
        {
            if (IsOrderFulfilled() || IsPaused) return false;
            
            switch (OrderType)
            {
                case EOrderType.Item:
                    return tableData.CanCraftItem(CraftedItem) && tableData.CanAffordToCraft(CraftedItem);
                case EOrderType.Meal:
                    return tableData.CanCookMeal(CraftedMeal) && tableData.CanAffordToCook(CraftedMeal);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public bool AreMaterialsAvailable()
        {
            switch (OrderType)
            {
                case EOrderType.Item:
                    if (_remainingMaterials.Any(itemAmount => !itemAmount.CanAfford()))
                    {
                        return false;
                    }
                    return true;
                case EOrderType.Meal:
                    if (_remainingIngredients.Any(ingredient => !ingredient.CanAfford()))
                    {
                        return false;
                    }
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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

        public bool IsOrderInProgress()
        {
            if (State == EOrderState.Claimed) return true;

            return false;
        }

        private void Enter_Queued()
        {
            
        }

        public void SubmitOrder(CraftingTableData tableData)
        {
            tableData.SubmitOrder(this);
            
            SetOrderState(EOrderState.Queued);
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
            OnOrderCancelled?.Invoke();
        }

        private void OnTaskComplete(Task task)
        {
            SetOrderState(EOrderState.Completed);
        }

        public float OrderProgress => (TotalCraftingWork - RemainingCraftingWork) / TotalCraftingWork;

        public void ReceiveItem(ItemData item)
        {
            ClaimedItems.Remove(item);
        }
        
        public float GetPercentMaterialsReceived()
        {
            if (CraftedItem == null && CraftedMeal == null) return 0f;
            if (State != EOrderState.Claimed || ClaimedItems == null) return 0f;
            
            int numItemsNeeded = 0;
            int remainingAmount = ClaimedItems.Count;
            
            if(CraftedMeal != null)
            {
                foreach (var cost in CraftedMeal.MealRequirements.GetIngredients())
                {
                    numItemsNeeded += cost.Amount;
                }

                if (numItemsNeeded == 0) return 1f;
                
                return 1f - (remainingAmount / (float)numItemsNeeded);
            }
            else
            {
                foreach (var cost in CraftedItem.CraftRequirements.GetMaterialCosts())
                {
                    numItemsNeeded += cost.Quantity;
                }
                
                if (numItemsNeeded == 0) return 1f;
                
                return 1f - (remainingAmount / (float)numItemsNeeded);
            }
        }
    }
}
