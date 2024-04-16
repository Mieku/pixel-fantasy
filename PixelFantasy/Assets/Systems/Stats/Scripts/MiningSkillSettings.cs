using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    [CreateAssetMenu(fileName = "MiningSkillSettings", menuName = "Skill System/Mining Skill Settings")]
    public class MiningSkillSettings : SkillSettings
    {
        public override ESkillType SkillType => ESkillType.Mining;

        protected override SkillLevelData CreateTableData(int level)
        {
            return new MiningSkillLevelData { Level = level };
        }

        public override float GetValueForLevel(EAttributeType attributeType, int level)
        {
            if (level > 0 && level <= Table.Count)
            {
                var miningData = (MiningSkillLevelData)Table[level - 1];
                return miningData.GetAttribute(attributeType);
            }
            Debug.LogError($"Level {level} is out of range for skill {SkillType}");
            return 0;
        }
    }

    [Serializable]
    public class MiningSkillLevelData : SkillLevelData
    {
        [TableColumnWidth(100)]
        public float MiningSpeed;
        
        [TableColumnWidth(100)]
        public float MiningYield;

        public override float GetAttribute(EAttributeType attributeType)
        {
            switch (attributeType)
            {
                case EAttributeType.MiningSpeed: return MiningSpeed;
                case EAttributeType.MiningYield: return MiningYield;
                default: throw new ArgumentOutOfRangeException(nameof(attributeType), $"Not supported attribute {attributeType} for mining.");
            }
        }
    }
}