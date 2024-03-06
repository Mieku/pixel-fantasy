using System;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Buildings.Scripts
{
    [CreateAssetMenu(fileName = "WallSettings", menuName = "Settings/Construction/Wall Settings")]
    public class WallSettings : ScriptableObject
    {
        [field: SerializeField] public string WallName { get; protected set; }
        [field: SerializeField] public List<WallOption> WallOptions { get; protected set; }
        public List<string> InvalidPlacementTags => new List<string>() { "Water", "Wall", "Structure", "Obstacle" };
        
    }
    
    [Serializable]
    public class WallOption
    {
        [SerializeField] private string _optionName;
        [SerializeField] private Sprite _optionIcon;
        [SerializeField] private RuleTile _exteriorRuleTile;
        [SerializeField] private RuleTile _interiorRuleTile;
        [SerializeField] private float _workCost;
        [SerializeField] private JobSettings _requiredJob;
        [SerializeField] private List<ItemAmount> _optionResourceCosts;
        
        public string OptionName => _optionName;
        public Sprite OptionIcon => _optionIcon;
        public RuleTile ExteriorWallRules => _exteriorRuleTile;
        public RuleTile InteriorWallRules => _interiorRuleTile;
        public float WorkCost => _workCost;
        public JobSettings RequiredJob => _requiredJob;

        public List<ItemAmount> OptionResourceCosts
        {
            get
            {
                List<ItemAmount> clone = new List<ItemAmount>();
                foreach (var cost in _optionResourceCosts)
                {
                    var cloneAmount = new ItemAmount
                    {
                        Item = cost.Item,
                        Quantity = cost.Quantity
                    };
                    clone.Add(cloneAmount);
                }

                return clone;
            }
        }
    }
}
