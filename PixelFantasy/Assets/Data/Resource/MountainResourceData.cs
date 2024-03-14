using System.Collections.Generic;
using Databrain.Attributes;
using ScriptableObjects;
using UnityEngine;

namespace Data.Resource
{
    public class MountainResourceData : ResourceData
    {
        // Settings
        [SerializeField] protected RuleTile _ruleTile;
        [SerializeField] protected MountainTileType _mountainTileType;
        
        // Accessors
        public MountainTileType MountainTileType => _mountainTileType;
        
        // Runtime
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public float RemainingWork;

        public override void InitData()
        {
            base.InitData();

            RemainingWork = _workToExtract;
        }

        public RuleTile GetRuleTile()
        {
            return _ruleTile;
        }

        public List<ItemAmount> GetMineDrop()
        {
            if (_harvestableItems != null)
            {
                return _harvestableItems.GetItemDrop();
            }

            return new List<ItemAmount>();
        }
    }
    
    public enum MountainTileType
    {
        Empty,
        Stone,
        Copper,
        Coal,
        Tin,
        Iron,
        Gold,
    }
}
