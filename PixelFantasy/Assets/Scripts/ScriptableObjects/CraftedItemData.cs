using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "CraftedItemData", menuName = "ItemData/CraftedItemData/CraftedItemData", order = 1)]
    public class CraftedItemData : ItemData
    {
        [TitleGroup("Crafted Data")] public List<FurnitureItemData> RequiredCraftingTableOptions;
        [TitleGroup("Crafted Data")] public JobData RequiredCraftingJob;
        [TitleGroup("Crafted Data")] public float WorkCost;
        [TitleGroup("Crafted Data")] [SerializeField] private List<ItemAmount> _resourceCosts;
        [TitleGroup("Crafted Data")] [SerializeField] private List<string> _invalidPlacementTags = new List<string>() { "Water", "Wall", "Obstacle" };

        public bool IsCraftingTableValid(FurnitureItemData furnitureItemData)
        {
            return RequiredCraftingTableOptions.Contains(furnitureItemData);
        }
        
        public List<ItemAmount> GetResourceCosts()
        {
            List<ItemAmount> clone = new List<ItemAmount>();
            foreach (var resourceCost in _resourceCosts)
            {
                ItemAmount cost = new ItemAmount
                {
                    Item = resourceCost.Item,
                    Quantity = resourceCost.Quantity
                };
                clone.Add(cost);
            }

            return clone;
        }

        public string MaterialsList
        {
            get
            {
                string materialsList = "";
                foreach (var cost in _resourceCosts)
                {
                    materialsList += cost.Quantity + "x " + cost.Item.ItemName + "\n";
                }
                return materialsList;
            }
        }
        
        public List<string> InvalidPlacementTags
        {
            get
            {
                List<string> clone = new List<string>();
                foreach (var tag in _invalidPlacementTags)
                {
                    clone.Add(tag);
                }

                return clone;
            }
        }
        
        public float GetWorkPerResource()
        {
            int totalQuantity = 0;
            foreach (var resourceCost in _resourceCosts)
            {
                totalQuantity += resourceCost.Quantity;
            }

            return WorkCost / totalQuantity;
        }
    }
}
