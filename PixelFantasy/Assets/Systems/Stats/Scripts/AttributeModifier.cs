using System;
using Characters;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    [Serializable]
    public abstract class Modifier
    {
        [field: SerializeField] public virtual EModifierType ModifierType { get; private set; }

        public abstract void ApplyModifier(KinlingData kinlingData);
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

        public bool AvailableForSkill(ESkillType? skillType)
        {
            if (IsGlobal || skillType == null) return true;

            return SpecificSkill == skillType;
        }
    }
    
    public enum EAttributeType
    {
        WorkModifier = 0,
        YieldModifier = 1,
        WalkSpeed = 2,
        FoodPoisonChance = 3, 
        MeleeChanceToHit = 4,
        MeleeChanceToDodge = 5,
        HuntingStealth = 6,
        RangedAccuracy = 7,
        ConstructionSuccessChance = 8,
        TameBeastChance = 9,
        TrainBeastChance = 10,
        SurgerySuccessChance = 11,
        TendQuality = 12,
        TradePriceBuy = 13,
        TradePriceSell = 14,
        SocialImpact = 15,
        LearningModifier = 16,
        SkillDecay = 17,
        QualityModifier = 18,
    }
}
