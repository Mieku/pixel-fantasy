using System;
using System.Collections.Generic;
using Characters;
using Managers;
using UnityEngine;

namespace ScriptableObjects
{
    public class ConstructionData : ScriptableObject
    {
        public string ConstructionName;
        public float WorkCost;
        public Sprite Icon;

        [SerializeField] private List<ItemAmount> _resourceCosts;
        [SerializeField] private List<string> _invalidPlacementTags;
        [SerializeField] private PlanningMode _planningMode;
        
        public PlanningMode PlanningMode => _planningMode;
        
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
        
        public Sprite GetSprite()
        {
            return Icon;
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
    
    [Serializable]
    public class ItemAmount
    {
        public ItemData Item;
        public int Quantity;
    }
}
