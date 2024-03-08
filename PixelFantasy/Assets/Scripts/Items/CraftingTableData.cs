using System.Collections.Generic;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    public class CraftingTableData : FurnitureData
    {
        [ShowInInspector] public List<ItemAmount> RemainingMaterials { get; set; }
        [ShowInInspector] public float RemainingCraftingWork { get; set; }
        [ShowInInspector] public CraftedItemSettings ItemBeingCrafted { get; set; }

        public CraftingTableSettings TableSettings => Settings as CraftingTableSettings;
        
        public CraftingTableData(CraftingTableSettings settings, FurnitureVarient selectedVariant, DyeSettings selectedDyeSettings) : base(settings, selectedVariant, selectedDyeSettings)
        {
            Debug.Log("Init Table");
        }
        
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
        
        public bool CanCraftItem(CraftedItemSettings item)
        {
            var validToCraft = TableSettings.CraftableItems.Contains(item);
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
    }
}
