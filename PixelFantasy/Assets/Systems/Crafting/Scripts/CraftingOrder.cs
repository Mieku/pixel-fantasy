using System;
using System.Collections.Generic;
using System.Linq;
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
        public CraftedItemSettings CraftedItem;
        public MealSettings CraftedMeal;
        public PlayerInteractable Requestor;
        public EOrderState State;
        public EOrderType OrderType;
        public bool IsGlobal;
        
        public Action OnOrderComplete;
        public Action OnOrderClaimed;
        public Action OnOrderCancelled;
        
        private List<ItemAmount> _remainingMaterials = new List<ItemAmount>();
        private List<Ingredient> _remainingIngredients = new List<Ingredient>();

        public enum EOrderType
        {
            Furniture,
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
        
        public CraftingOrder(CraftedItemSettings itemToCraft, PlayerInteractable requestor, EOrderType orderType, bool isGlobal, Action onOrderClaimed, Action onOrderComplete,
            Action onOrderCancelled)
        {
            CraftedItem = itemToCraft;
            OrderType = orderType;
            Requestor = requestor;
            IsGlobal = isGlobal;
            OnOrderClaimed = onOrderClaimed;
            OnOrderComplete = onOrderComplete;
            OnOrderCancelled = onOrderCancelled;
            
            _remainingMaterials = itemToCraft.CraftRequirements.GetMaterialCosts();
            
            SetOrderState(EOrderState.Queued);
        }
        
        // For Meals
        public CraftingOrder(MealSettings mealToCraft, PlayerInteractable requestor, bool isGlobal, Action onOrderClaimed, Action onOrderComplete,
            Action onOrderCancelled)
        {
            CraftedMeal = mealToCraft;
            OrderType = EOrderType.Meal;
            IsGlobal = isGlobal;
            Requestor = requestor;
            OnOrderClaimed = onOrderClaimed;
            OnOrderComplete = onOrderComplete;
            OnOrderCancelled = onOrderCancelled;

            _remainingIngredients = mealToCraft.MealRequirements.GetIngredients();
            
            SetOrderState(EOrderState.Queued);
        }
        
        public Task CreateTask(Action<Task> onTaskComplete, CraftingTable table)
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

            Task task;
            switch (OrderType)
            {
                case EOrderType.Furniture:
                    task = new Task("Craft Furniture Order", CraftedItem.CraftRequirements.CraftingSkill, Requestor, CraftedItem.CraftRequirements.RequiredCraftingToolType)
                    {
                        Payload = CraftedItem,
                        OnTaskComplete = onTaskComplete,
                        Materials = claimedMats,
                    };
                    break;
                case EOrderType.Item:
                    task = new Task("Craft Item", CraftedItem.CraftRequirements.CraftingSkill, table, CraftedItem.CraftRequirements.RequiredCraftingToolType)
                    {
                        Payload = CraftedItem,
                        OnTaskComplete = onTaskComplete,
                        Materials = claimedMats,
                    };
                    break;
                case EOrderType.Meal:
                    task = new Task("Cook Meal", CraftedMeal.MealRequirements.CraftingSkill, table,
                        CraftedMeal.MealRequirements.RequiredCraftingToolType)
                    {
                        Payload = CraftedMeal,
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

        public bool CanBeCrafted(CraftingTable table)
        {
            switch (OrderType)
            {
                case EOrderType.Furniture:
                case EOrderType.Item:
                    return table.RuntimeTableData.CanCraftItem(CraftedItem) && table.RuntimeTableData.CanAffordToCraft(CraftedItem);
                case EOrderType.Meal:
                    return table.RuntimeTableData.CanCookMeal(CraftedMeal) && table.RuntimeTableData.CanAffordToCraft(CraftedItem);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
        
        public bool AreMaterialsAvailable()
        {
            switch (OrderType)
            {
                case EOrderType.Furniture:
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
