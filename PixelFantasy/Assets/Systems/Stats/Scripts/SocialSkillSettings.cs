using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    [CreateAssetMenu(fileName = "Social Skill Settings", menuName = "Skill System/Social Skill Settings")]
    public class SocialSkillSettings : SkillSettings
    {
        public override ESkillType SkillType => ESkillType.Social; 

        protected override SkillLevelData CreateTableData(int level)
        {
            return new SocialSkillLevelData { Level = level };
        }

        public override float GetValueForLevel(EAttributeType attributeType, int level)
        {
            if (level > 0 && level <= Table.Count)
            {
                var socialData = (SocialSkillLevelData)Table[level - 1];
                return socialData.GetAttribute(attributeType);
            }
            Debug.LogError($"Level {level} is out of range for skill {SkillType}");
            return 0;
        }
    }
    
    [Serializable]
    public class SocialSkillLevelData : SkillLevelData
    {
        [TableColumnWidth(100)]
        public float TradePriceBuyModifier;
        
        [TableColumnWidth(100)]
        public float TradePriceSellModifier;
        
        [TableColumnWidth(100)]
        public float SocialImpact;

        public override float GetAttribute(EAttributeType attributeType)
        {
            switch (attributeType)
            {
                case EAttributeType.TradePriceBuy: return TradePriceBuyModifier;
                case EAttributeType.TradePriceSell: return TradePriceSellModifier;
                case EAttributeType.SocialImpact: return SocialImpact;
                default: throw new ArgumentOutOfRangeException(nameof(attributeType), $"Not supported attribute {attributeType} for social.");
            }
        }
    }
}
