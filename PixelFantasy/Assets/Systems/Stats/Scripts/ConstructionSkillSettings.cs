using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    [CreateAssetMenu(fileName = "Construction Skill Settings", menuName = "Skill System/Construction Skill Settings")]
    public class ConstructionSkillSettings : SkillSettings
    {
        public override ESkillType SkillType => ESkillType.Construction; 

        protected override SkillLevelData CreateTableData(int level)
        {
            return new ConstructionSkillLevelData { Level = level };
        }

        public override float GetValueForLevel(EAttributeType attributeType, int level)
        {
            if (level > 0 && level <= Table.Count)
            {
                var constructionData = (ConstructionSkillLevelData)Table[level - 1];
                return constructionData.GetAttribute(attributeType);
            }
            Debug.LogError($"Level {level} is out of range for skill {SkillType}");
            return 0;
        }
    }
    
    [Serializable]
    public class ConstructionSkillLevelData : SkillLevelData
    {
        [TableColumnWidth(100)]
        public float ConstructionWorkModifier;
        
        [TableColumnWidth(100)]
        public float SuccessChance;

        public override float GetAttribute(EAttributeType attributeType)
        {
            switch (attributeType)
            {
                case EAttributeType.WorkModifier: return ConstructionWorkModifier;
                case EAttributeType.ConstructionSuccessChance: return SuccessChance;
                default: throw new ArgumentOutOfRangeException(nameof(attributeType), $"Not supported attribute {attributeType} for construction.");
            }
        }
    }
}
