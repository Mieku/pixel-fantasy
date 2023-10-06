using System;
using System.Collections.Generic;
using System.ComponentModel;
using Items;
using Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "FurnitureItemData", menuName = "ItemData/CraftedItemData/FurnitureItemData/FurnitureItemData", order = 1)]
    public class FurnitureItemData : CraftedItemData
    {
        [Header("General")]
        [TitleGroup("Furniture Item Data")] public FurnitureCatergory Catergory;

        [Header("Different Directions")] 
        [TitleGroup("Furniture Item Data")] public PlacementDirection DefaultDirection = PlacementDirection.Down;
        [TitleGroup("Furniture Item Data")] public Furniture North_FurniturePrefab;
        [TitleGroup("Furniture Item Data")] public Furniture East_FurniturePrefab;
        [TitleGroup("Furniture Item Data")] public Furniture South_FurniturePrefab;
        [TitleGroup("Furniture Item Data")] public Furniture West_FurniturePrefab;
        
        [Header("Varients")]
        [TitleGroup("Furniture Item Data")] public List<FurnitureItemData> Varients;

        public Furniture GetDefaultFurniturePrefab()
        {
            return GetFurniturePrefab(DefaultDirection);
        }
        
        public Furniture GetFurniturePrefab(PlacementDirection direction)
        {
            switch (direction)
            {
                case PlacementDirection.Up:
                    return North_FurniturePrefab;
                case PlacementDirection.Right:
                    return East_FurniturePrefab;
                case PlacementDirection.Down:
                    return South_FurniturePrefab;
                case PlacementDirection.Left:
                    return West_FurniturePrefab;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }    
        }
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
