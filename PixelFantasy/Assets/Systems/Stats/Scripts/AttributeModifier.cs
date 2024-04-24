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
        public ESkillType SpecificSkill;
        public bool IsGlobal;
        public override EModifierType ModifierType => EModifierType.Attribute;
        
        public override void ApplyModifier(KinlingData kinlingData)
        {
            if (!kinlingData.StatsData.AttributeModifiers.Contains(this))
            {
                kinlingData.StatsData.AttributeModifiers.Add(this);
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
            if (!IsGlobal)
            {
                result += $" for {SpecificSkill.GetDescription()}";
            }

            return result;
        }

        public bool AvailableForSkill(ESkillType? skillType)
        {
            if (IsGlobal || skillType == null) return true;

            return SpecificSkill == skillType;
        }
    }
    
    public enum EAttributeType
    {
        [Description("Work Modifier")] WorkModifier = 0,
        [Description("Yield Modifier")] YieldModifier = 1,
        [Description("Walk Speed")] WalkSpeed = 2,
        [Description("Food Poison Chance")] FoodPoisonChance = 3, 
        [Description("Melee Chance To Hit")] MeleeChanceToHit = 4,
        [Description("Melee Chance To Dodge")] MeleeChanceToDodge = 5,
        [Description("Hunting Stealth")] HuntingStealth = 6,
        [Description("Ranged Accuracy")] RangedAccuracy = 7,
        [Description("Construction Success Chance")] ConstructionSuccessChance = 8,
        [Description("Tame Beast Chance")] TameBeastChance = 9,
        [Description("Train Beast Chance")] TrainBeastChance = 10,
        [Description("Surgery Success Chance")] SurgerySuccessChance = 11,
        [Description("Tend Quality")] TendQuality = 12,
        [Description("Trade Price Buy")] TradePriceBuy = 13,
        [Description("Trade Price Sell")] TradePriceSell = 14,
        [Description("Social Impact")] SocialImpact = 15,
        [Description("Leanering Modifier")] LearningModifier = 16,
        [Description("Skill Decay")] SkillDecay = 17,
        [Description("Quality Modifier")] QualityModifier = 18,
    }
}
