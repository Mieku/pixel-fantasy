using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    [CreateAssetMenu(fileName = "Melee Skill Settings", menuName = "Skill System/Melee Skill Settings")]
    public class MeleeSkillSettings : SkillSettings
    {
        public override ESkillType SkillType => ESkillType.Melee; 

        protected override SkillLevelData CreateTableData(int level)
        {
            return new MeleeSkillLevelData { Level = level };
        }

        public override float GetValueForLevel(EAttributeType attributeType, int level)
        {
            if (level > 0 && level <= Table.Count)
            {
                var meleeData = (MeleeSkillLevelData)Table[level - 1];
                return meleeData.GetAttribute(attributeType);
            }
            Debug.LogError($"Level {level} is out of range for skill {SkillType}");
            return 0;
        }
    }
    
    [Serializable]
    public class MeleeSkillLevelData : SkillLevelData
    {
        [TableColumnWidth(100)]
        public float ChanceToHit;
        
        [TableColumnWidth(100)]
        public float ChanceToDodge;

        public override float GetAttribute(EAttributeType attributeType)
        {
            switch (attributeType)
            {
                case EAttributeType.MeleeChanceToHit: return ChanceToHit;
                case EAttributeType.MeleeChanceToDodge: return ChanceToDodge;
                default: throw new ArgumentOutOfRangeException(nameof(attributeType), $"Not supported attribute {attributeType} for melee.");
            }
        }
    }
}
