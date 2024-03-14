using System.Collections.Generic;
using Databrain.Attributes;
using Items;
using ScriptableObjects;
using UnityEngine;

namespace Data.Resource
{
    public class MountainResourceData : ResourceData
    {
        // Settings
        [SerializeField] protected int _workToMine;
        [SerializeField] protected HarvestableItems _minedResources;
        [SerializeField] protected RuleTile _ruleTile;
        [SerializeField] protected MountainTileType _mountainTileType;
        
        // Accessors
        public new HarvestableItems HarvestableItems => _minedResources;
        public MountainTileType MountainTileType => _mountainTileType;
        
        // Runtime
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public float RemainingWork;

        public override void InitData()
        {
            base.InitData();

            RemainingWork = _workToMine;
        }

        public RuleTile GetRuleTile()
        {
            return _ruleTile;
        }

        public List<ItemAmount> GetMineDrop()
        {
            if (_minedResources != null)
            {
                return _minedResources.GetItemDrop();
            }

            return new List<ItemAmount>();
        }

        public int GetWorkAmount()
        {
            return _workToMine;
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
