using System;
using System.Collections.Generic;
using Characters;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "FurnitureSettings", menuName = "Settings/Furniture/Basic Furniture Settings")]
    public class FurnitureSettings : CraftedItemSettings
    {
        [Header("General")] 
        [TitleGroup("Furniture Settings")] public Furniture FurniturePrefab;
        [TitleGroup("Furniture Settings")] public NeedChange InUseNeedChange;
        [TitleGroup("Furniture Settings")] public ColourOptions ColourOptions;
        [TitleGroup("Furniture Settings")] public List<FurnitureVarient> Varients;
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
        public List<DyeSettings> DyePalettes;
    }
}
