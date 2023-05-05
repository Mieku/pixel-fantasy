using System.Collections.Generic;
using Items;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "FurnitureItemData", menuName = "CraftedData/FurnitureItemData", order = 1)]
    public class FurnitureItemData : CraftedItemData
    {
        public List<Furniture> PlacedFurnitureOptions;

        public Furniture GetRandomFurnitureOption()
        {
            var rand = Random.Range(0, PlacedFurnitureOptions.Count);
            return PlacedFurnitureOptions[rand];
        }
    }
}
