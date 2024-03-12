using System.Collections.Generic;
using ScriptableObjects;
using Systems.Crafting.Scripts;

namespace Data.Item
{
    public class CraftingTableData : FurnitureData
    {
        public List<CraftedItemData> CraftableItems;
        public List<ItemAmount> RemainingMaterials = new List<ItemAmount>();
        public float RemainingCraftingWork;
        public CraftedItemData ItemBeingCrafted;
        public CraftingOrder CurrentOrder;
        
        
        public float GetPercentCraftingComplete()
        {
            if (ItemBeingCrafted == null) return 0f;
            
            return 1 - (RemainingCraftingWork / ItemBeingCrafted.CraftRequirements.WorkCost);
        }
        
        public float GetPercentMaterialsReceived()
        {
            if (ItemBeingCrafted == null) return 0f;
            
            int numItemsNeeded = 0;
            foreach (var cost in ItemBeingCrafted.CraftRequirements.GetResourceCosts())
            {
                numItemsNeeded += cost.Quantity;
            }

            int numItemsRemaining = 0;
            foreach (var remaining in RemainingMaterials)
            {
                numItemsRemaining += remaining.Quantity;
            }
            
            if (numItemsNeeded == 0)
            {
                return 1f;
            }
            else
            {
                return 1f - (numItemsRemaining / (float)numItemsNeeded);
            }
        }
        
        public bool CanCraftItem(CraftedItemData item)
        {
            var validToCraft = CraftableItems.Contains(item);
            if (!validToCraft) return false;
            
            // Are the mats available?
            foreach (var cost in item.CraftRequirements.GetResourceCosts())
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
