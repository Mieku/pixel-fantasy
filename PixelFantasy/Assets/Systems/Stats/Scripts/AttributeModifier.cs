using System;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    [Serializable]
    public abstract class Modifier
    {
        [field: SerializeField] public EModifierType ModifierType { get; private set; }
    }

    public enum EModifierType
    {
        Attribute,
    }
    
    [Serializable]
    public class AttributeModifier : Modifier
    {
        public EAttributeType AttributeType;
        public float Modifier;
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
    }
}
