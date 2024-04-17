using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    [CreateAssetMenu(fileName = "Ranged Skill Settings", menuName = "Skill System/Ranged Skill Settings")]
    public class RangedSkillSettings : SkillSettings
    {
        public override ESkillType SkillType => ESkillType.Ranged; 

        protected override SkillLevelData CreateTableData(int level)
        {
            return new RangedSkillLevelData { Level = level };
        }

        public override float GetValueForLevel(EAttributeType attributeType, int level)
        {
            if (level > 0 && level <= Table.Count)
            {
                var rangedData = (RangedSkillLevelData)Table[level - 1];
                return rangedData.GetAttribute(attributeType);
            }
            Debug.LogError($"Level {level} is out of range for skill {SkillType}");
            return 0;
        }
    }
    
    [Serializable]
    public class RangedSkillLevelData : SkillLevelData
    {
        [TableColumnWidth(100)]
        public float HuntingStealth;
        
        [TableColumnWidth(100)]
        public float RangedAccuracy;

        public override float GetAttribute(EAttributeType attributeType)
        {
            switch (attributeType)
            {
                case EAttributeType.RangedAccuracy: return RangedAccuracy;
                case EAttributeType.HuntingStealth: return HuntingStealth;
                default: throw new ArgumentOutOfRangeException(nameof(attributeType), $"Not supported attribute {attributeType} for ranged.");
            }
        }
    }
}
