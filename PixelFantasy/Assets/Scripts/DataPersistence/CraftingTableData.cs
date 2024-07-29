using System;
using DataPersistence;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Systems.Crafting.Scripts;
using Systems.Stats.Scripts;

public class CraftingTableData : FurnitureData
{
    public string CurrentOrderID;
    [JsonIgnore] public CraftingOrder CurrentOrder => LocalCraftingQueue.Query(CurrentOrderID);
    [ShowInInspector] public CraftingOrderQueue LocalCraftingQueue = new CraftingOrderQueue();
    
    [JsonIgnore] public CraftingTableSettings CraftingTableSettings => FurnitureSettings as CraftingTableSettings;
    
    public bool CanCraftItem(CraftedItemSettings settings)
    {
        var validToCraft = CraftingTableSettings.CraftableItems.Contains(settings);
        return validToCraft;
    }

    public bool CanAffordToCraft(CraftedItemSettings settings)
    {
        foreach (var cost in settings.CraftRequirements.CostSettings)
        {
            if (!cost.CanAfford())
            {
                return false;
            }
        }

        return true;
    }
    
    public bool CanAffordToCook(MealSettings mealSettings)
    {
        foreach (var cost in mealSettings.MealRequirements.GetIngredients())
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
        return validToCraft;
    }

    public ESkillType CraftingSkillType()
    {
        switch (CurrentOrder.OrderType)
        {
            case CraftingOrder.EOrderType.Item:
                return ESkillType.Crafting;
            case CraftingOrder.EOrderType.Meal:
                return ESkillType.Cooking;
            default:
                throw new ArgumentOutOfRangeException();
        } 
    }
    
    public void SubmitOrder(CraftingOrder order)
    {
        LocalCraftingQueue.Orders.Add(order);
    }
}