using System.Collections.Generic;
using Actions;
using Items;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "MountainResourceData", menuName = "ResourceData/MountainResourceData", order = 1)]
    public class MountainData : ScriptableObject
    {
        public string ResourceName;
        [SerializeField] private List<ActionBase> _availableActions;
        [SerializeField] private int _workToMine;
        [SerializeField] private HarvestableItems _minedResources;
        [SerializeField] private RuleTile _ruleTile;
        
        public List<ActionBase> AvailableActions => _availableActions;

        public RuleTile GetRuleTile()
        {
            return _ruleTile;
        }
    }
}
