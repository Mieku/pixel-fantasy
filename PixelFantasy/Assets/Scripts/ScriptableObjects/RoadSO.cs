using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "RoadSO", menuName = "CraftedData/RoadSO", order = 1)]
    public class RoadSO : ScriptableObject
    {
        [field: SerializeField] public string RoadName { get; protected set; }
        [field: SerializeField] public List<RoadOption> RoadOptions { get; protected set; }
        public List<string> InvalidPlacementTags => new List<string>() { "Water", "Wall", "Floor", "Obstacle" };

        public RoadOption OptionByIndex(int index)
        {
            if (index > RoadOptions.Count - 1)
            {
                Debug.LogError($"Reqested non-existant option index: {index}");
                return null;
            }

            return RoadOptions[index];
        }
    }

    [Serializable]
    public class RoadOption
    {
        [SerializeField] private string _optionName;
        [SerializeField] private Sprite _optionIcon;
        [SerializeField] private TileBase _roadRuleTile;
        [SerializeField] private float _workCost;
        [SerializeField] private JobData _requiredJob;
        [SerializeField] private List<ItemAmount> _optionResourceCosts;
        
        public string OptionName => _optionName;
        public Sprite OptionIcon => _optionIcon;
        public TileBase RoadRuleTile => _roadRuleTile;
        public float WorkCost => _workCost;
        public JobData RequiredJob => _requiredJob;

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
