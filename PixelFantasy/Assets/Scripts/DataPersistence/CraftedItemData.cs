using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Newtonsoft.Json;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;

public class CraftedItemData : ItemData
{
    public string CraftersUID;
    
    [JsonIgnore] public CraftedItemSettings CraftedItemSettings => (CraftedItemSettings) Settings;

    public override List<DetailsText> GetDetailsTexts()
    {
        var results = base.GetDetailsTexts();

        if (!string.IsNullOrEmpty(CraftersUID))
        {
            // Add crafter
            var crafter = KinlingsDatabase.Instance.GetUnit(CraftersUID);
            results.Add(new DetailsText("Crafted By:", crafter.FullName));
        }

        return results;
    }
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
                    materialsList += cost.Quantity + "x " + cost.Item.ItemName + "\n";
                }
                return materialsList;
            }
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
