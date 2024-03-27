using System.Collections.Generic;
using Databrain.Attributes;
using UnityEngine;

namespace Data.Item
{
    public class CraftingTableSettings : FurnitureSettings
    {
        // Settings
        [DataObjectDropdown(true)] [SerializeField] protected List<CraftedItemSettings> _craftableItems;
        [DataObjectDropdown(true)] [SerializeField] protected List<MealSettings> _cookableMeals;
        
        // Accessors
        public List<CraftedItemSettings> CraftableItems => _craftableItems;
        public List<MealSettings> CookableMeals => _cookableMeals;
    }
}
