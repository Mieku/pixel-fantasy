using System;
using System.Collections.Generic;
using System.Linq;
using AI;
using Managers;
using Newtonsoft.Json;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
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
            var crafter = KinlingsDatabase.Instance.GetKinling(CraftersUID);
            results.Add(new DetailsText("Crafted By:", crafter.FullName));
        }

        return results;
    }
}

 [Serializable]
    public class CraftRequirements
    {
        [SerializeField] private List<SkillRequirement> _skillRequirements;
        [SerializeField] private int _minCraftingSkillLevel;
        [SerializeField] private ETaskType _craftingSkill = ETaskType.Crafting;
        [SerializeField] private float _workCost;
        [SerializeField] private EToolType _requiredCraftingToolType;
        [FormerlySerializedAs("_materialCosts")] [SerializeField] private List<CostSettings> _costSettings;
        
        public float WorkCost => _workCost;
        public EToolType RequiredCraftingToolType => _requiredCraftingToolType;
        public int MinCraftingSkillLevel => _minCraftingSkillLevel;
        public ETaskType CraftingSkill => _craftingSkill;
        public List<CostSettings> CostSettings => _costSettings;
        public List<SkillRequirement> SkillRequirements => _skillRequirements;
        
        public float GetWorkPerResource()
        {
            int totalQuantity = 0;
            foreach (var resourceCost in _costSettings)
            {
                totalQuantity += resourceCost.Quantity;
            }

            return WorkCost / totalQuantity;
        }

        public List<CostData> GetMaterialCosts()
        {
            List<CostData> results = new List<CostData>();
            foreach (var cost in _costSettings)
            {
                results.Add(new CostData(cost));
            }

            return results;
        }
        
        public string MaterialsList
        {
            get
            {
                string materialsList = "";
                foreach (var cost in _costSettings)
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
                foreach (var cost in _costSettings)
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
            copy._costSettings = this._costSettings.Select(itemAmount => itemAmount.Clone()).ToList();
            return copy;
        }
    }
