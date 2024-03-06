using System.Collections.Generic;
using Items;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "WallFurnitureSettings", menuName = "Settings/Items/Wall Furniture Settings")]
    public class WallFurnitureSettings : CraftedItemSettings
    {
        public List<WallFurniture> PlacedFurnitureOptions;

        public WallFurniture GetRandomFurnitureOption()
        {
            var rand = Random.Range(0, PlacedFurnitureOptions.Count);
            return PlacedFurnitureOptions[rand];
        }
    }
}
