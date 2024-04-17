using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    [CreateAssetMenu(fileName = "Intelligence Skill Settings", menuName = "Skill System/Intelligence Skill Settings")]
    public class IntelligenceSkillSettings : SkillSettings
    {
        public override ESkillType SkillType => ESkillType.Intelligence; 

        protected override SkillLevelData CreateTableData(int level)
        {
            return new IntelligenceSkillLevelData { Level = level };
        }

        public override float GetValueForLevel(EAttributeType attributeType, int level)
        {
            if (level > 0 && level <= Table.Count)
            {
                var intelligenceData = (IntelligenceSkillLevelData)Table[level - 1];
                return intelligenceData.GetAttribute(attributeType);
            }
            Debug.LogError($"Level {level} is out of range for skill {SkillType}");
            return 0;
        }
    }
    
    [Serializable]
    public class IntelligenceSkillLevelData : SkillLevelData
    {
        [TableColumnWidth(100)]
        public float ResearchWorkModifier;
        
        [TableColumnWidth(100)]
        public float LearningModifier;

        public override float GetAttribute(EAttributeType attributeType)
        {
            switch (attributeType)
            {
                case EAttributeType.WorkModifier: return ResearchWorkModifier;
                case EAttributeType.LearningModifier: return LearningModifier;
                default: throw new ArgumentOutOfRangeException(nameof(attributeType), $"Not supported attribute {attributeType} for intelligence.");
            }
        }
    }
}
