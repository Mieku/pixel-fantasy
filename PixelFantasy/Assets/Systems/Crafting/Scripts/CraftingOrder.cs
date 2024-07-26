using System;
using System.Collections.Generic;
using System.Linq;
using AI;
using Handlers;
using Items;
using Managers;
using Newtonsoft.Json;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;

namespace Systems.Crafting.Scripts
{
    [Serializable]
    public class CraftingOrder
    {
        public string CraftedItemSettingsID;
        [JsonIgnore] public ItemSettings ItemToCraftSettings => GameSettings.Instance.LoadItemSettings(CraftedItemSettingsID);
        
        public EOrderState State;

        [JsonIgnore]
        public EOrderType OrderType
        {
            get
            {
                if (ItemToCraftSettings is MealSettings)
                {
                    return EOrderType.Meal;
                }
                else
                {
                    return EOrderType.Item;
                }
            }
        }

        public float RemainingCraftingWork;
        public float TotalCraftingWork;
        public List<string> ClaimedItemUIDs = new List<string>();
        public List<string> ReceivedItemUIDs = new List<string>();

        public EFulfillmentType FulfillmentType;
        public int Amount;
        public AI.Task CraftingTask;
        public bool IsPaused;
        
        public Action OnOrderComplete;
        public Action OnOrderClaimed;
        public Action OnOrderCancelled;
        
        private List<CostData> _remainingMaterials = new List<CostData>();
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
        
        public CraftingOrder(string craftedItemSettingsID, Action onOrderClaimed = null, Action onOrderComplete = null,
            Action onOrderCancelled = null)
        {
            CraftedItemSettingsID = craftedItemSettingsID;
            OnOrderClaimed = onOrderClaimed;
            OnOrderComplete = onOrderComplete;
            OnOrderCancelled = onOrderCancelled;
            FulfillmentType = EFulfillmentType.Amount;
            Amount = 1;
            IsPaused = false;

            RefreshOrderRequirements();
        }

        [JsonIgnore] public ItemSettings GetOrderedItemSettings => ItemToCraftSettings;

        public void RefreshOrderRequirements()
        {
            switch (OrderType)
            {
                case EOrderType.Item:
                    CraftedItemSettings craftedItem = (CraftedItemSettings)ItemToCraftSettings;
                    TotalCraftingWork = craftedItem.CraftRequirements.WorkCost;
                    RemainingCraftingWork = craftedItem.CraftRequirements.WorkCost;
                    _remainingMaterials = craftedItem.CraftRequirements.GetMaterialCosts();
                    break;
                case EOrderType.Meal:
                    MealSettings craftedMeal = (MealSettings)ItemToCraftSettings;
                    TotalCraftingWork = craftedMeal.MealRequirements.WorkCost;
                    RemainingCraftingWork = craftedMeal.MealRequirements.WorkCost;
                    _remainingIngredients = craftedMeal.MealRequirements.GetIngredients();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public AI.Task CreateTask(Action<AI.Task, bool> onTaskComplete, Action onTaskCancelled, CraftingTable table)
        {
            List<string> claimedMats = null;

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
            
            var taskData = new Dictionary<string, object>
            {
                { "ClaimedMaterials", claimedMats }
            };
            ETaskType taskType;
            List<SkillRequirement> skillRequirements;
            
            switch (OrderType)
            {
                case EOrderType.Item:
                    CraftedItemSettings craftedItem = (CraftedItemSettings)ItemToCraftSettings;
                    taskData.Add("CraftedItemSettingsID", craftedItem.name);
                    taskType = craftedItem.CraftRequirements.CraftingSkill;
                    skillRequirements = craftedItem.CraftRequirements.SkillRequirements;
                    break;
                case EOrderType.Meal:
                    MealSettings craftedMeal = (MealSettings)ItemToCraftSettings;
                    taskData.Add("CraftedItemSettingsID", craftedMeal.name);
                    taskType = craftedMeal.MealRequirements.CraftingSkill;
                    skillRequirements = craftedMeal.MealRequirements.SkillRequirements;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            AI.Task task = new AI.Task("Craft Item", taskType, table, taskData, skillRequirements);
            task.OnCompletedCallback += onTaskComplete;
            task.OnCancelledCallback += onTaskCancelled;
            CraftingTask = task;
            
            OnOrderClaimed?.Invoke();
            
            return task;
        }

        public bool IsOrderFulfilled()
        {
            switch (FulfillmentType)
            {
                case EFulfillmentType.Amount:
                    return Amount == 0;
                case EFulfillmentType.Until:
                    int amountAvailable;
                    amountAvailable = InventoryManager.Instance.GetAmountAvailable(ItemToCraftSettings);
                    return amountAvailable >= Amount;
                case EFulfillmentType.Forever:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private List<string> ClaimRequiredMaterials()
        {
            CraftedItemSettings craftedItem = (CraftedItemSettings)ItemToCraftSettings;
            var requiredItems = craftedItem.CraftRequirements.GetMaterialCosts();
            List<string> claimedItems = new List<string>();
            
            foreach (var requiredItem in requiredItems)
            {
                for (int i = 0; i < requiredItem.Quantity; i++)
                {
                    var claimedItem = InventoryManager.Instance.GetItemOfType(requiredItem.Item);

                    claimedItem.ClaimItem();
                    claimedItems.Add(claimedItem.UniqueID);
                }
            }

            return claimedItems;
        }

        private List<string> ClaimRequiredIngredients()
        {
            MealSettings craftedMeal = (MealSettings)ItemToCraftSettings;
            var ingredients = craftedMeal.MealRequirements.GetIngredients();
            List<string> claimedItems = new List<string>();

            foreach (var ingredient in ingredients)
            {
                for (int i = 0; i < ingredient.Amount; i++)
                {
                    ItemData claimedItem = InventoryManager.Instance.GetFoodItemOfType(ingredient.FoodType);
                    
                    claimedItem.ClaimItem();
                    claimedItems.Add(claimedItem.UniqueID);
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
            foreach (var claimedItemUID in ClaimedItemUIDs)
            {
                var claimedItem = ItemsDatabase.Instance.Query(claimedItemUID);
                if (claimedItem.State == EItemState.Stored)
                {
                    claimedItem.UnclaimItem();
                }
            }
            ClaimedItemUIDs.Clear();
            
            if (CraftingTask != null)
            {
                CraftingTask.Cancel(false);
                RefundUsedCraftingMaterials();
            }
            
            OnOrderCancelled?.Invoke();
        }

        private void RefundUsedCraftingMaterials()
        {
            foreach (var itemUID in ReceivedItemUIDs)
            {
                var itemData = ItemsDatabase.Instance.Query(itemUID);
                itemData.State = EItemState.Loose;
                ItemsDatabase.Instance.CreateItemObject(itemData, itemData.Position, true);
            }
            ReceivedItemUIDs.Clear();
        }

        public bool CanBeCrafted(CraftingTableData tableData)
        {
            if (IsOrderFulfilled() || IsPaused) return false;
            
            switch (OrderType)
            {
                case EOrderType.Item:
                    CraftedItemSettings craftedItem = (CraftedItemSettings)ItemToCraftSettings;
                    return tableData.CanCraftItem(craftedItem) && tableData.CanAffordToCraft(craftedItem);
                case EOrderType.Meal:
                    MealSettings craftedMeal = (MealSettings)ItemToCraftSettings;
                    return tableData.CanCookMeal(craftedMeal) && tableData.CanAffordToCook(craftedMeal);
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
            
            // Clear the items
            foreach (var itemUID in ReceivedItemUIDs)
            {
                var itemData = ItemsDatabase.Instance.Query(itemUID);
                itemData.DeleteItemData();
            }
            ReceivedItemUIDs.Clear();
        }
        
        private void Enter_Cancelled()
        {
            OnOrderCancelled?.Invoke();
        }
        
        [JsonIgnore] public float OrderProgress => (TotalCraftingWork - RemainingCraftingWork) / TotalCraftingWork;

        public void ReceiveItem(ItemData item)
        {
            item.State = EItemState.BeingProcessed;
            ClaimedItemUIDs.Remove(item.UniqueID);
            ReceivedItemUIDs.Add(item.UniqueID);
        }
        
        public float GetPercentMaterialsReceived()
        {
            if (ItemToCraftSettings == null) return 0f;
            if (State != EOrderState.Claimed || ClaimedItemUIDs == null) return 0f;
            
            int numItemsNeeded = 0;
            int remainingAmount = ClaimedItemUIDs.Count;
            
            if(ItemToCraftSettings is MealSettings craftedMeal)
            {
                foreach (var cost in craftedMeal.MealRequirements.GetIngredients())
                {
                    numItemsNeeded += cost.Amount;
                }

                if (numItemsNeeded == 0) return 1f;
                
                return 1f - (remainingAmount / (float)numItemsNeeded);
            }
            
            if(ItemToCraftSettings is CraftedItemSettings craftedItem)
            {
                foreach (var cost in craftedItem.CraftRequirements.GetMaterialCosts())
                {
                    numItemsNeeded += cost.Quantity;
                }
                
                if (numItemsNeeded == 0) return 1f;
                
                return 1f - (remainingAmount / (float)numItemsNeeded);
            }
            
            // Just in case
            Debug.LogError($"Unknown item type: {ItemToCraftSettings.name}");
            return 0f;
        }
    }
}
