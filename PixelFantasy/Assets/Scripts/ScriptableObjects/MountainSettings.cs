using System.Collections.Generic;
using Items;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "MountainSettings", menuName = "Settings/Resources/Mountain Settings")]
    public class MountainSettings : ResourceSettings
    {
        [SerializeField] private int _workToMine;
        [SerializeField] private HarvestableItems _minedResources;
        [SerializeField] private RuleTile _ruleTile;

        public HarvestableItems HarvestableItems => _minedResources;
        public MountainTileType MountainTileType;
        
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
