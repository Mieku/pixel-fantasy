using System.Collections.Generic;
using Databrain.Attributes;
using Systems.Details.Generic_Details.Scripts;
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
}
