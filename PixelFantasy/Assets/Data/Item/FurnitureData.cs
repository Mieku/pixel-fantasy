using System;
using System.Collections.Generic;
using Characters;
using Databrain.Attributes;
using Items;
using Managers;
using ScriptableObjects;

namespace Data.Item
{
    public class FurnitureData : CraftedItemData
    {
        public Furniture LinkedFurniture;
        
        // Furniture Settings
        public Items.Furniture FurniturePrefab;
        public NeedChange InUseNeedChange;
        public ColourOptions ColourOptions;
        
        // Furniture Data
        public EFurnitureState State;
        public float RemainingWork;
        public PlacementDirection Direction;
        public bool InUse;
        public List<FurnitureVarient> Varients;
    }
}

[Serializable]
public enum EFurnitureState
{
    InProduction,
    Built,
}
