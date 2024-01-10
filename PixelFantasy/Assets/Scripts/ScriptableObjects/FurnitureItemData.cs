using System;
using System.Collections.Generic;
using System.ComponentModel;
using Characters;
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
        [TitleGroup("Furniture Item Data")] public Furniture FurniturePrefab;
        [TitleGroup("Furniture Item Data")] public StatChange InUseStatChange;

        [Header("Varients")]
        [TitleGroup("Furniture Item Data")] public List<FurnitureItemData> Varients;
    }

    public enum FurnitureCatergory
    {
        [Description("Furniture")] Furniture,
        [Description("Decoration")] Decorations,
        [Description("Storage")] Storage,
        [Description("Production")] Production,
        [Description("Lighting")] Lighting,
    }
}
