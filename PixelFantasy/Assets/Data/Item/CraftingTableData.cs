using System.Collections.Generic;
using Databrain.Attributes;
using Managers;
using ScriptableObjects;
using Systems.Crafting.Scripts;
using UnityEngine;

namespace Data.Item
{
    public class CraftingTableData : FurnitureData
    {
        // Runtime
        [ExposeToInspector, DatabrainSerialize] public List<ItemData> RemainingMaterials = new List<ItemData>();
        [ExposeToInspector, DatabrainSerialize] public float RemainingCraftingWork;
        [ExposeToInspector, DatabrainSerialize] public CraftedItemDataSettings ItemBeingCrafted;
        [ExposeToInspector, DatabrainSerialize] public MealSettings MealBeingCooked;
        [ExposeToInspector, DatabrainSerialize] public CraftingOrder CurrentOrder;
        
        public CraftingTableSettings CraftingTableSettings => Settings as CraftingTableSettings;
        
        public float GetPercentCraftingComplete()
        {
            if (ItemBeingCrafted == null) return 0f;
            
            return 1 - (RemainingCraftingWork / ItemBeingCrafted.CraftRequirements.WorkCost);
        }
        
        public float GetPercentMaterialsReceived()
        {
            if (ItemBeingCrafted == null && MealBeingCooked == null) return 0f;

            int numItemsNeeded = 0;
            if (MealBeingCooked != null)
            {
                foreach (var cost in MealBeingCooked.MealRequirements.GetIngredients())
                {
                    numItemsNeeded += cost.Amount;
                }
            }
            else
            {
                foreach (var cost in ItemBeingCrafted.CraftRequirements.GetMaterialCosts())
                {
                    numItemsNeeded += cost.Quantity;
                }
            }
            
            int numItemsRemaining = RemainingMaterials.Count;
            
            if (numItemsNeeded == 0)
            {
                return 1f;
            }
            else
            {
                return 1f - (numItemsRemaining / (float)numItemsNeeded);
            }
        }
        
        public bool CanCraftItem(CraftedItemDataSettings settings)
        {
            var validToCraft = CraftingTableSettings.CraftableItems.Contains(settings);
            if (!validToCraft) return false;
            
            // Are the mats available?
            foreach (var cost in settings.CraftRequirements.GetMaterialCosts())
            {
                if (!cost.CanAfford())
                {
                    return false;
                }
            }

            return true;
        }

        public bool CanCookMeal(MealSettings mealSettings)
        {
            var validToCraft = CraftingTableSettings.CookableMeals.Contains(mealSettings);
            if (!validToCraft) return false;
            
            // Are the mats available?
            foreach (var cost in mealSettings.MealRequirements.GetIngredients())
            {
                if (!cost.CanAfford())
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsAvailable()
        {
            if (State == EFurnitureState.InProduction) return false;
            if (ItemBeingCrafted != null) return false;

            return true;
        }
    }
}
