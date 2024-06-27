using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Crafting Table Settings", menuName = "Settings/Crafting Table Settings")]
public class CraftingTableSettings : FurnitureSettings
{
    // Settings
    [SerializeField] protected List<CraftedItemSettings> _craftableItems;
    [SerializeField] protected List<MealSettings> _cookableMeals;
        
    // Accessors
    public List<CraftedItemSettings> CraftableItems => _craftableItems;
    public List<MealSettings> CookableMeals => _cookableMeals;

    public List<ItemSettings> GetCraftableItems
    {
        get
        {
            List<ItemSettings> results = new List<ItemSettings>();
            results.AddRange(_craftableItems);
            results.AddRange(_cookableMeals);
            return results;
        }
    }
}
