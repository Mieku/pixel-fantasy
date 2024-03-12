using System;
using System.Collections.Generic;
using System.Linq;
using Data.Item;
using Sirenix.OdinInspector;
using Systems.Skills.Scripts;
using TaskSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "CraftedItemSettings", menuName = "Settings/Items/Crafted Item Settings")]
    public class CraftedItemSettings : ItemSettings
    {
        [TitleGroup("Crafted Settings")] [SerializeField] private CraftRequirements _craftRequirements;
        private readonly List<string> _invalidPlacementTags = new List<string>() { "Water", "Wall", "Obstacle"};
        
        public CraftRequirements CraftRequirements => _craftRequirements.Clone();
        
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
    }

    [Serializable]
    public class MaterialVarient
    {
        [SerializeField] private string _varientName;
        [SerializeField] private Sprite _varientSprite;
        [SerializeField] private Sprite _materialSelectIcon;
        [SerializeField] private CraftRequirements _craftRequirements;
        [SerializeField] private int _durability = 100;
        
        public string VarientName => _varientName;
        public Sprite VarientSprite => _varientSprite;
        public Sprite MaterialSelectIcon => _materialSelectIcon; // Typically the icon of the material change
        public int Durability => _durability;
        public CraftRequirements CraftRequirements => _craftRequirements.Clone();
    }

    [Serializable]
    public class CraftRequirements
    {
        [SerializeField] private int _minCraftingSkillLevel;
        [SerializeField] private ETaskType _craftingSkill;
        [SerializeField] private List<ItemAmount> _materialCosts;
        [SerializeField] private float _workCost;
        [SerializeField] private EToolType _requiredCraftingToolType;
        
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
