using System;
using System.ComponentModel;
using Characters;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    [Serializable]
    public abstract class Modifier
    {
        [field: SerializeField] public virtual EModifierType ModifierType { get; private set; }

        public abstract void ApplyModifier(KinlingData kinlingData);
        public abstract string GetModifierString();
    }

    public enum EModifierType
    {
        Attribute,
        Skill,
        IncapableSkill,
        AddTrait,
        PermanentMood,
    }
    
    [Serializable]
    public class AttributeModifier : Modifier
    {
        public EAttributeType AttributeType;
        public float Modifier;
 
        public override EModifierType ModifierType => EModifierType.Attribute;
        
        public override void ApplyModifier(KinlingData kinlingData)
        {
            if (!kinlingData.Stats.AttributeModifiers.Contains(this))
            {
                kinlingData.Stats.AttributeModifiers.Add(this);
            }
        }

        public override string GetModifierString()
        {
            string modifier;
            if (Modifier > 0)
            {
                modifier = $"+{(Modifier * 100f)}%";
            } 
            else
            {
                modifier = $"{(Modifier * 100f)}%";
            }
            
            string result = $"{AttributeType.GetDescription()} {modifier}";

            return result;
        }
    }
    
    public enum EAttributeType
    {
        // General
        [Description("Global Work Speed")] GlobalWorkSpeed = 0,
        [Description("Walk Speed")] WalkSpeed = 1,
        [Description("Skill Decay")] SkillDecay = 2,
        [Description("Appetite")] Appetite = 3,
        [Description("Attractiveness")] Attractiveness = 4,
        [Description("Social Frequency")] SocialFrequency = 5,
        
        // Beast Mastery
        [Description("Beast Work Speed")] BeastWorkSpeed = 20,
        [Description("Beast Gather Yield")] BeastGatherYield = 21,
        [Description("Tame Beast Chance")] TameBeastChance = 22,
        [Description("Train Beast Chance")] TrainBeastChance = 23,
        
        // Botany
        [Description("Botany Speed")] BotanySpeed = 30,
        [Description("Botany Yield")] BotanyYield = 31,
        
        // Construction
        [Description("Construction Speed")] ConstructionSpeed = 40,
        [Description("Construction Chance")] ConstructionSuccessChance = 41,
        
        // Cooking
        [Description("Cooking Speed")] CookingSpeed = 50,
        [Description("Butchering Yield")] ButcheringYield = 51,
        [Description("Food Poison Chance")] FoodPoisonChance = 52, 
        
        // Crafting
        [Description("Crafting Speed")] CraftingSpeed = 60,
        [Description("Crafting Quality")] CraftingQuality = 61,
        
        // Intelligence
        [Description("Research Speed")] ResearchSpeed = 70,
        [Description("Learning Speed")] LearningModifier = 71,
        
        // Medical
        [Description("Medical Speed")] MedicalSpeed = 80,
        [Description("Surgery Success Chance")] SurgerySuccessChance = 81,
        [Description("Tend Quality")] TendQuality = 82,
        
        // Melee
        [Description("Melee Chance To Hit")] MeleeChanceToHit = 90,
        [Description("Melee Chance To Dodge")] MeleeChanceToDodge = 91,
        
        // Mining
        [Description("Mining Speed")] MiningSpeed = 100,
        [Description("Mining Yield")] MiningYield = 101,
        
        // Ranged
        [Description("Hunting Stealth")] HuntingStealth = 110,
        [Description("Ranged Accuracy")] RangedAccuracy = 111,
        
        // Social
        [Description("Trade Price Buy")] TradePriceBuy = 120,
        [Description("Trade Price Sell")] TradePriceSell = 121,
        [Description("Social Impact")] SocialImpact = 123,

    }
}
