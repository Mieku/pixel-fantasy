using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.Stats.Scripts
{
    [CreateAssetMenu(fileName = "Botany Skill Settings", menuName = "Skill System/Botany Skill Settings")]
    public class BotanySkillSettings : SkillSettings
    {
        public override ESkillType SkillType => ESkillType.Botany; 

        protected override SkillLevelData CreateTableData(int level)
        {
            return new BotanySkillLevelData { Level = level };
        }

        public override float GetValueForLevel(EAttributeType attributeType, int level)
        {
            if (level > 0 && level <= Table.Count)
            {
                var botanyData = (BotanySkillLevelData)Table[level - 1];
                return botanyData.GetAttribute(attributeType);
            }
            Debug.LogError($"Level {level} is out of range for skill {SkillType}");
            return 0;
        }
    }
    
    
    [Serializable]
    public class BotanySkillLevelData : SkillLevelData
    {
        [TableColumnWidth(100)]
        public float BotanySpeed;
        
        [TableColumnWidth(100)]
        public float BotanyYield;

        public override float GetAttribute(EAttributeType attributeType)
        {
            switch (attributeType)
            {
                case EAttributeType.BotanySpeed: return BotanySpeed;
                case EAttributeType.BotanyYield: return BotanyYield;
                default: throw new ArgumentOutOfRangeException(nameof(attributeType), $"Not supported attribute {attributeType} for botany.");
            }
        }
    }
}
