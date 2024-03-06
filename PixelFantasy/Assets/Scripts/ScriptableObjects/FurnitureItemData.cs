using System;
using System.Collections.Generic;
using Characters;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "FurnitureItemData", menuName = "ItemData/CraftedItemData/FurnitureItemData/FurnitureItemData", order = 1)]
    public class FurnitureItemData : CraftedItemData
    {
        [Header("General")] 
        [TitleGroup("Furniture Item Data")] public Furniture FurniturePrefab;
        [TitleGroup("Furniture Item Data")] public NeedChange InUseNeedChange;
        [TitleGroup("Furniture Item Data")] public ColourOptions ColourOptions;
        [TitleGroup("Furniture Item Data")] public List<FurnitureVarient> Varients;
    }
    
    [Serializable]
    public class FurnitureVarient : MaterialVarient
    {
        public Furniture Prefab;
        public NeedChange InUseNeedChange;
    }

    [Serializable]
    public class ColourOptions
    {
        public string ColourOptionsHeader;
        public List<DyePaletteData> DyePalettes;
    }
}
