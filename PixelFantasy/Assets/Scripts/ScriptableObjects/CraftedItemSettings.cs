using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Systems.Skills.Scripts;
using TaskSystem;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "CraftedItemSettings", menuName = "Settings/Items/Crafted Item Settings")]
    public class CraftedItemSettings : ItemSettings
    {
        [TitleGroup("Crafted Settings")] public CraftRequirements CraftRequirements;
        [TitleGroup("Crafted Settings")] [SerializeField] private List<string> _invalidPlacementTags = new List<string>() { "Water", "Wall", "Obstacle" };
        
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
        public string VarientName;
        public Sprite VarientSprite;
        public Sprite MaterialSelectIcon; // Typically the icon of the material change
        public CraftRequirements CraftRequirements;
        public int Durability = 100;
    }

    [Serializable]
    public class CraftRequirements
    {
        public int MinCraftingSkillLevel;
        public ETaskType CraftingSkill;
        public List<ItemAmount> MaterialCosts;
        public float WorkCost;
        public EToolType RequiredCraftingToolType;
        
        public float GetWorkPerResource()
        {
            int totalQuantity = 0;
            foreach (var resourceCost in MaterialCosts)
            {
                totalQuantity += resourceCost.Quantity;
            }

            return WorkCost / totalQuantity;
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
        
        public List<ItemAmount> GetResourceCosts()
        {
            List<ItemAmount> clone = new List<ItemAmount>(MaterialCosts);
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
    }
}
