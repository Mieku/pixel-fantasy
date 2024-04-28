using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.Stats.Scripts
{
    [CreateAssetMenu(fileName = "Beast Mastery Skill Settings", menuName = "Skill System/Beast Mastery Skill Settings")]
    public class BeastMasterySkillSettings : SkillSettings
    {
        public override ESkillType SkillType => ESkillType.BeastMastery; 

        protected override SkillLevelData CreateTableData(int level)
        {
            return new BeastMasterySkillLevelData { Level = level };
        }

        public override float GetValueForLevel(EAttributeType attributeType, int level)
        {
            if (level > 0 && level <= Table.Count)
            {
                var beastMasteryData = (BeastMasterySkillLevelData)Table[level - 1];
                return beastMasteryData.GetAttribute(attributeType);
            }
            Debug.LogError($"Level {level} is out of range for skill {SkillType}");
            return 0;
        }
    }
    
    [Serializable]
    public class BeastMasterySkillLevelData : SkillLevelData
    {
        [TableColumnWidth(100)]
        public float BeastWorkSpeed;
        
        [TableColumnWidth(100)]
        public float BeastGatherYield;
        
        [TableColumnWidth(100)]
        public float TameBeastChance;
        
        [TableColumnWidth(100)]
        public float TrainBeastChance;

        public override float GetAttribute(EAttributeType attributeType)
        {
            switch (attributeType)
            {
                case EAttributeType.BeastWorkSpeed: return BeastWorkSpeed;
                case EAttributeType.BeastGatherYield: return BeastGatherYield;
                case EAttributeType.TameBeastChance: return TameBeastChance;
                case EAttributeType.TrainBeastChance: return TrainBeastChance;
                default: throw new ArgumentOutOfRangeException(nameof(attributeType), $"Not supported attribute {attributeType} for beast master.");
            }
        }
    }
}
