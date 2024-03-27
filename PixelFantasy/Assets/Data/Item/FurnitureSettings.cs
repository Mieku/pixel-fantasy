using System.Collections.Generic;
using Characters;
using Data.Dye;
using Databrain.Attributes;
using Items;
using Managers;
using UnityEngine;

namespace Data.Item
{
    public class FurnitureSettings : CraftedItemSettings
    {
        // Furniture Settings
        [SerializeField] protected Furniture _furniturePrefab;
        [SerializeField] protected NeedChange _inUseNeedChange;
        [SerializeField] protected ColourOptions _colourOptions;
        [SerializeField] protected List<FurnitureVariant> _varients;
        [SerializeField] protected PlacementDirection _defaultDirection;
        [DataObjectDropdown][SerializeField] protected DyeData _defaultDye;
        
        // Accessors
        public Furniture FurniturePrefab => _furniturePrefab;
        public NeedChange InUseNeedChange => _inUseNeedChange;
        public ColourOptions ColourOptions => _colourOptions;
        public List<FurnitureVariant> Varients => _varients;
        public PlacementDirection DefaultDirection => _defaultDirection;
        public DyeData DefaultDye => _defaultDye;
    }
}
