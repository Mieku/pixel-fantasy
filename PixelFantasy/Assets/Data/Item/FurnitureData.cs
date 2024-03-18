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
        // Furniture Settings
        [SerializeField] protected List<string> _invalidPlacementTags = new List<string>() { "Water", "Wall", "Obstacle"};
        [SerializeField] protected Furniture _furniturePrefab;
        [SerializeField] protected NeedChange _inUseNeedChange;
        [SerializeField] protected ColourOptions _colourOptions;
        [SerializeField] protected List<FurnitureVariant> _varients;
        [SerializeField] protected PlacementDirection _defaultDirection;
        [DataObjectDropdown][SerializeField] protected DyeData _defaultDye;
        
        // Accessors
        public List<string> InvalidPlacementTags => _invalidPlacementTags;
        public Furniture FurniturePrefab => _furniturePrefab;
        public NeedChange InUseNeedChange => _inUseNeedChange;
        public ColourOptions ColourOptions => _colourOptions;
        public List<FurnitureVariant> Varients => _varients;
        public PlacementDirection DefaultDirection => _defaultDirection;
        public DyeData DefaultDye => _defaultDye;
        
        // Runtime
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public EFurnitureState State;
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public float RemainingWork;
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public PlacementDirection Direction;
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public bool InUse;
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public Furniture LinkedFurniture; // Not a fan of this...
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public DyeData DyeOverride;

        public override void InitData()
        {
            base.InitData();
            RemainingWork = _craftRequirements.WorkCost;
            Direction = _defaultDirection;
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
    [SerializeField] private FurnitureData _furnitureData;

    public Sprite MaterialSelectIcon => _materialSelectIcon; // Typically the icon of the material change
    public FurnitureData FurnitureData => _furnitureData.GetInitialDataObject() as FurnitureData;
}

[Serializable]
public class ColourOptions
{
    public string ColourOptionsHeader;
    [DataObjectDropdown] public List<DyeData> DyePalettes;
}
