using System;
using System.Collections.Generic;
using Characters;
using Data.Dye;
using Data.Item;
using Databrain.Attributes;
using Items;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data.Item
{
    public class FurnitureData : CraftedItemData
    {
        // Runtime
        [ExposeToInspector, DatabrainSerialize] public EFurnitureState State;
        [ExposeToInspector, DatabrainSerialize] public float RemainingWork;
        [ExposeToInspector, DatabrainSerialize] public PlacementDirection Direction;
        [ExposeToInspector, DatabrainSerialize] public bool InUse;
        [ExposeToInspector, DatabrainSerialize] public Furniture LinkedFurniture;
        [ExposeToInspector, DatabrainSerialize] public DyeData DyeOverride;

        public FurnitureSettings FurnitureSettings => Settings as FurnitureSettings;

        public override void InitData(ItemSettings itemSettings)
        {
            base.InitData(itemSettings);
            var furnitureSettings = itemSettings as FurnitureSettings;
            RemainingWork = furnitureSettings.CraftRequirements.WorkCost;
            Direction = furnitureSettings.DefaultDirection;
        }
        
        protected void OnChanged()
        {
            LinkedFurniture?.OnChanged?.Invoke();
        }
    }
}

[Serializable]
public enum EFurnitureState
{
    InProduction,
    Built,
}

[Serializable]
public class FurnitureVariant
{
    [SerializeField] private Sprite _materialSelectIcon;
    [FormerlySerializedAs("_furnitureData")] [SerializeField] private FurnitureSettings _furnitureSettings;

    public Sprite MaterialSelectIcon => _materialSelectIcon; // Typically the icon of the material change
    public FurnitureSettings FurnitureSettings => _furnitureSettings;
}

[Serializable]
public class ColourOptions
{
    public string ColourOptionsHeader;
    [DataObjectDropdown] public List<DyeData> DyePalettes;
}
