using System;
using System.Collections.Generic;
using System.Linq;
using AI;
using Handlers;
using Items;
using Managers;
using Newtonsoft.Json;
using ScriptableObjects;
using UnityEngine;

namespace Systems.Crafting.Scripts
{
    [Serializable]
    public class CraftingOrder
    {
        public string UniqueID;
        public string TaskUID;
        public string CraftedItemSettingsID;
        public EOrderState State;
        
        public float RemainingCraftingWork;
        public float TotalCraftingWork;
        public List<string> ClaimedItemUIDs = new List<string>();
        public List<string> ReceivedItemUIDs = new List<string>();

        public EFulfillmentType FulfillmentType;
        public int Amount;
        public bool IsPaused;
        
        [JsonIgnore] public ItemSettings ItemToCraftSettings => GameSettings.Instance.LoadItemSettings(CraftedItemSettingsID);
        [JsonIgnore] public AI.Task CraftingTask => TasksDatabase.Instance.QueryTask(TaskUID);
        [JsonIgnore] public ItemSettings GetOrderedItemSettings => ItemToCraftSettings;
        [JsonIgnore] public float OrderProgress => (TotalCraftingWork - RemainingCraftingWork) / TotalCraftingWork;

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
            UniqueID = Guid.NewGuid().ToString();
            CraftedItemSettingsID = craftedItemSettingsID;
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
                    CraftedItemSettings craftedItem = (CraftedItemSettings)ItemToCraftSettings;
                    TotalCraftingWork = craftedItem.CraftRequirements.WorkCost;
                    RemainingCraftingWork = craftedItem.CraftRequirements.WorkCost;
                    break;
                case EOrderType.Meal:
                    MealSettings craftedMeal = (MealSettings)ItemToCraftSettings;
                    TotalCraftingWork = craftedMeal.MealRequirements.WorkCost;
                    RemainingCraftingWork = craftedMeal.MealRequirements.WorkCost;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public AI.Task CreateTask(CraftingTable table)
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
            
            AI.Task task = new AI.Task("Craft Item", $"Crafting {ItemToCraftSettings.ItemName}" ,taskType, table, taskData, skillRequirements);
            TaskUID = task.UniqueID;
            
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

        public void ClearMaterialsReceived()
        {
            // Clear the items
            foreach (var itemUID in ReceivedItemUIDs)
            {
                var itemData = ItemsDatabase.Instance.Query(itemUID);
                itemData.DeleteItemData();
            }
            ReceivedItemUIDs.Clear();
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

        public void CancelOrder()
        {
            if (ClaimedItemUIDs != null)
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
            }
            
            if (CraftingTask != null)
            {
                CraftingTask.Cancel();
                RefundUsedCraftingMaterials();
            }
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
                    CraftedItemSettings craftedItem = (CraftedItemSettings)ItemToCraftSettings;
                    if (craftedItem.CraftRequirements.GetMaterialCosts().Any(itemAmount => !itemAmount.CanAfford()))
                    {
                        return false;
                    }
                    return true;
                case EOrderType.Meal:
                    MealSettings craftedMeal = (MealSettings)ItemToCraftSettings;
                    if (craftedMeal.MealRequirements.GetIngredients().Any(ingredient => !ingredient.CanAfford()))
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

        private void Enter_Claimed()
        {
            
        }

        private void Enter_Completed()
        {
            if (FulfillmentType == EFulfillmentType.Amount)
            {
                Amount = Mathf.Clamp(Amount - 1, 0, 999);
            }
            
            State = EOrderState.None;
            RefreshOrderRequirements();
        }
        
        private void Enter_Cancelled()
        {
            
        }

        public void ReceiveItem(ItemData item)
        {
            item.State = EItemState.BeingProcessed;
            ClaimedItemUIDs.Remove(item.UniqueID);
            ReceivedItemUIDs.Add(item.UniqueID);
        }
        
        public float GetPercentMaterialsReceived()
        {
            if (ItemToCraftSettings == null) return 0f;
            if (State != EOrderState.Claimed || ReceivedItemUIDs == null) return 0f;
            
            int numItemsNeeded = 0;
            
            if (ItemToCraftSettings is MealSettings craftedMeal)
            {
                foreach (var cost in craftedMeal.MealRequirements.GetIngredients())
                {
                    numItemsNeeded += cost.Amount;
                }

                if (numItemsNeeded == 0) return 1f;

                return (ReceivedItemUIDs.Count / (float) numItemsNeeded);
            }
            
            if (ItemToCraftSettings is CraftedItemSettings craftedItem)
            {
                foreach (var cost in craftedItem.CraftRequirements.GetMaterialCosts())
                {
                    numItemsNeeded += cost.Quantity;
                }
                
                if (numItemsNeeded == 0) return 1f;
                
                return (ReceivedItemUIDs.Count / (float) numItemsNeeded);
            }
            
            // Just in case
            Debug.LogError($"Unknown item type: {ItemToCraftSettings.name}");
            return 0f;
        }
    }
}
