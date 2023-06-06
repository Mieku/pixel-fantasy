using System.Collections.Generic;
using Items;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "WallFurnitureItemData", menuName = "CraftedData/WallFurnitureItemData", order = 1)]
    public class WallFurnitureItemData : CraftedItemData
    {
        public List<WallFurniture> PlacedFurnitureOptions;

        public WallFurniture GetRandomFurnitureOption()
        {
            var rand = Random.Range(0, PlacedFurnitureOptions.Count);
            return PlacedFurnitureOptions[rand];
        }
    }
}
