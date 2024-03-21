using System;
using System.Collections.Generic;
using Characters;
using Data.Item;
using Databrain;
using Databrain.Attributes;
using Managers;
using UnityEngine;

namespace ScriptableObjects
{
    public class ConstructionSettings : ScriptableObject
    {
        public string ConstructionName;
        [TextArea] public string ConstructionDetails;
        public float WorkCost;
        public Sprite Icon;
        public float MaxDurability;
        public int Price;

        [SerializeField] private List<ItemAmount> _resourceCosts;
        [SerializeField] private List<string> _invalidPlacementTags = new List<string>(){"Water", "Wall", "Obstacle", "Furniture", "Structure"};
        [SerializeField] private PlanningMode _planningMode;
        [SerializeField] private JobSettings _requiredConstructorJob;
        
        public PlanningMode PlanningMode => _planningMode;
        public JobSettings RequiredConstructorJob => _requiredConstructorJob;
        
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
        [DataObjectDropdown("DataLibrary", true)] public ItemDataSettings Item;
        public int Quantity;

        public bool CanAfford()
        {
            return InventoryManager.Instance.CanAfford(Item, Quantity);
        }

        public ItemAmount Clone()
        {
            return new ItemAmount
            {
                Item = this.Item,
                Quantity = this.Quantity
            };
        }
    }
}
