using System.Collections.Generic;
using Items;
using ScriptableObjects;

namespace Buildings.Building_Types
{
    public class TownCenterBuilding : Building, IStockpileBuilding, IProductionBuilding
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
    }
}
