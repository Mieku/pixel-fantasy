using System;
using System.Collections.Generic;
using Characters;
using Data.Dye;
using Data.Item;
using Databrain.Attributes;
using Items;
using Managers;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;

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

        public FurnitureDataSettings FurnitureSettings => Settings as FurnitureDataSettings;

        public override void InitData(ItemDataSettings itemDataSettings)
        {
            base.InitData(itemDataSettings);
            var furnitureSettings = itemDataSettings as FurnitureDataSettings;
            RemainingWork = furnitureSettings.CraftRequirements.WorkCost;
            Direction = furnitureSettings.DefaultDirection;
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
    [SerializeField] private FurnitureDataSettings _furnitureData;

    public Sprite MaterialSelectIcon => _materialSelectIcon; // Typically the icon of the material change
    public FurnitureDataSettings FurnitureData => _furnitureData;
}

[Serializable]
public class ColourOptions
{
    public string ColourOptionsHeader;
    [DataObjectDropdown] public List<DyeData> DyePalettes;
}
