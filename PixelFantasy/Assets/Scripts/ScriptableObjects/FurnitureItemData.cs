using System.Collections.Generic;
using System.ComponentModel;
using Items;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "FurnitureItemData", menuName = "CraftedData/FurnitureItemData", order = 1)]
    public class FurnitureItemData : CraftedItemData
    {
        public FurnitureCatergory Catergory;
        public Furniture FurniturePrefab;
        public List<FurnitureItemData> Varients;

        
        // public Furniture GetRandomFurnitureOption()
        // {
        //     var rand = Random.Range(0, PlacedFurnitureOptions.Count);
        //     return PlacedFurnitureOptions[rand];
        // }
    }

    public enum FurnitureCatergory
    {
        [Description("Furniture")] Furniture,
        [Description("Decorations")] Decorations,
        [Description("Storages")] Storage,
        [Description("Production")] Production,
        [Description("Lighting")] Lighting,
    }
}
