using System;
using System.Collections.Generic;
using System.Linq;
using Databrain.Attributes;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;

namespace Data.Item
{
    public class CraftedItemData : ItemData
    {
        // Crafted Item Settings
        [SerializeField] protected CraftRequirements _craftRequirements;

        // Accessors
        public CraftRequirements CraftRequirements => _craftRequirements;
        
        // Runtime
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public string CraftersUID;
    }
    
    [Serializable]
    public class CraftRequirements
    {
        [SerializeField] private int _minCraftingSkillLevel;
        [SerializeField] private ETaskType _craftingSkill = ETaskType.Crafting;
        [SerializeField] private float _workCost;
        [SerializeField] private EToolType _requiredCraftingToolType;
        [SerializeField] private List<ItemAmount> _materialCosts;
        
        public float WorkCost => _workCost;
        public EToolType RequiredCraftingToolType => _requiredCraftingToolType;
        public int MinCraftingSkillLevel => _minCraftingSkillLevel;
        public ETaskType CraftingSkill => _craftingSkill;
        public List<ItemAmount> MaterialCosts => GetMaterialCosts();
        
        public float GetWorkPerResource()
        {
            int totalQuantity = 0;
            foreach (var resourceCost in MaterialCosts)
            {
                totalQuantity += resourceCost.Quantity;
            }

            return WorkCost / totalQuantity;
        }

        public List<ItemAmount> GetMaterialCosts()
        {
            List<ItemAmount> clone = new List<ItemAmount>(_materialCosts);
            return clone;
        }
        
        public string MaterialsList
        {
            get
            {
                string materialsList = "";
                foreach (var cost in MaterialCosts)
                {
                    materialsList += cost.Quantity + "x " + cost.Item.title + "\n";
                }
                return materialsList;
            }
        }
        
        public List<ItemAmount> GetResourceCosts()
        {
            List<ItemAmount> clone = new List<ItemAmount>(_materialCosts);
            return clone;
        }

        public bool MaterialsAreAvailable
        {
            get
            {
                foreach (var cost in MaterialCosts)
                {
                    if (!cost.CanAfford()) return false;
                }

                return true;
            }
        }

        public bool SomeoneHasCraftingSkillNeeded
        {
            get
            { 
                // TODO: Build me!
                return true;
            }
        }

        public bool CanBeCrafted => MaterialsAreAvailable && SomeoneHasCraftingSkillNeeded;
        
        public CraftRequirements Clone()
        {
            CraftRequirements copy = (CraftRequirements)this.MemberwiseClone();
            copy._materialCosts = this._materialCosts.Select(itemAmount => itemAmount.Clone()).ToList();
            return copy;
        }
    }
}
