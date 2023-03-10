using System.Collections.Generic;
using Items;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "MountainResourceData", menuName = "ResourceData/MountainResourceData", order = 1)]
    public class MountainData : ResourceData
    {
        [SerializeField] private int _workToMine;
        [SerializeField] private HarvestableItems _minedResources;
        [SerializeField] private RuleTile _ruleTile;

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
}
