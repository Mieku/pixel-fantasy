using System.Collections.Generic;
using Databrain.Attributes;
using UnityEngine;

namespace Data.Item
{
    public class CraftingTableSettings : FurnitureDataSettings
    {
        // Settings
        [DataObjectDropdown(true)] [SerializeField] protected List<CraftedItemDataSettings> _craftableItems;
        
        // Accessors
        public List<CraftedItemDataSettings> CraftableItems => _craftableItems;
    }
}
