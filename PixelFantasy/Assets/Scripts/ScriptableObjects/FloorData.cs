using System.Collections.Generic;
using Gods;
using Items;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "FloorData", menuName = "CraftedData/FloorData", order = 1)]
    public class FloorData : ScriptableObject
    {
        public string FloorName;
        public Sprite FloorSprite;
        public float WorkCost;
        public float WalkSpeedModifier; // TODO: Not Implemented Yet
        public float PathfindingPenalty;
        public bool StretchToWall;

        [SerializeField] private List<ItemAmount> _resourceCosts;
        [SerializeField] private List<string> _invalidPlacementTags;
        [SerializeField] private List<Option> _options;
        [SerializeField] private PlanningMode _planningMode;

        public PlanningMode PlanningMode => _planningMode;

        public Sprite Icon => FloorSprite;
        
        public List<ItemAmount> GetResourceCosts()
        {
            List<ItemAmount> clone = new List<ItemAmount>();
            foreach (var resourceCost in _resourceCosts)
            {
                ItemAmount cost = new ItemAmount
                {
                    Item = resourceCost.Item,
                    Quantity = resourceCost.Quantity
                };
                clone.Add(cost);
            }

            return clone;
        }

        public List<string> InvalidPlacementTags
        {
            get
            {
                List<string> clone = new List<string>();
                foreach (var tag in _invalidPlacementTags)
                {
                    clone.Add(tag);
                }

                return clone;
            }
        }

        public List<Option> Options
        {
            get
            {
                List<Option> clone = new List<Option>();
                foreach (var option in _options)
                {
                    clone.Add(option);
                }

                return clone;
            }
        }
        
        public float GetWorkPerResource()
        {
            int totalQuantity = 0;
            foreach (var resourceCost in _resourceCosts)
            {
                totalQuantity += resourceCost.Quantity;
            }

            return WorkCost / totalQuantity;
        }
    }
}
