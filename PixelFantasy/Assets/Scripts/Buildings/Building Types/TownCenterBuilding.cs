using System.Collections.Generic;
using System.Linq;
using Items;
using ScriptableObjects;

namespace Buildings.Building_Types
{
    public class TownCenterBuilding : Building, IStockpileBuilding, IProductionBuilding, IEateryBuilding
    {
        public override BuildingType BuildingType => BuildingType.TownHall;
        
        public override string OccupantAdjective => "Workers";
        
        private List<ItemData> _unallowedItems = new List<ItemData>();
        
        public CraftingTable CraftingTable { get; set; }
        public List<ProductionSettings> ProductionSettings { get; } = new List<ProductionSettings>();
        
        public bool IsItemStockpileAllowed(ItemData itemData)
        {
            return !_unallowedItems.Contains(itemData);
        }
        
        public void SetAllowedStockpileItem(ItemData itemData, bool isAllowed)
        {
            if (isAllowed)
            {
                if (_unallowedItems.Contains(itemData))
                {
                    _unallowedItems.Remove(itemData);
                }
            }
            else
            {
                if (!_unallowedItems.Contains(itemData))
                {
                    _unallowedItems.Add(itemData);
                }
            }
        }
        
        public float GetProductionProgress(CraftedItemData item)
        {
            if (CraftingTable == null) return 0;

            if (CraftingTable.ItemBeingCrafted != item) return 0;

            return CraftingTable.GetPercentCraftingComplete();
        }
        
        public CraftingTable FindCraftingTable(CraftedItemData craftedItemData)
        {
            var allCraftingTables = CraftingTables;
            foreach (var table in allCraftingTables)
            {
                if (table.ItemBeingCrafted == craftedItemData)
                {
                    return table;
                }
            }

            return null;
        }

        public Item ClaimBestAvailableFood()
        {
            var storages = GetBuildingStorages();
            List<Item> storedFood = new List<Item>();
            foreach (var storage in storages)
            {
                var food = storage.GetAllFoodItems(false, false);
                storedFood.AddRange(food);
            }

            if (storedFood.Count == 0)
            {
                return null;
            }
            
            var selectedFood = storedFood.OrderByDescending(food => ((RawFoodItemData)food.GetItemData()).FoodNutrition).ToList()[0];
            selectedFood.ClaimItem();
            return selectedFood;
        }
    }
}
