using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    [CreateAssetMenu(fileName = "CookingSkillSettings", menuName = "Skill System/Cooking Skill Settings")]
    public class CookingSkillSettings : SkillSettings
    {
        public override ESkillType SkillType => ESkillType.Cooking; 

        protected override SkillLevelData CreateTableData(int level)
        {
            return new CookingSkillLevelData { Level = level };
        }

        public override float GetValueForLevel(EAttributeType attributeType, int level)
        {
            if (level > 0 && level <= Table.Count)
            {
                var cookingData = (CookingSkillLevelData)Table[level - 1];
                return cookingData.GetAttribute(attributeType);
            }
            Debug.LogError($"Level {level} is out of range for skill {SkillType}");
            return 0;
        }
    }

    [Serializable]
    public class CookingSkillLevelData : SkillLevelData
    {
        [TableColumnWidth(100)]
        public float CookingSpeed;
        
        [TableColumnWidth(100)]
        public float ButcheringYield;
        
        [TableColumnWidth(100)]
        public float FoodPoisonChance;

        public override float GetAttribute(EAttributeType attributeType)
        {
            switch (attributeType)
            {
                case EAttributeType.CookingSpeed: return CookingSpeed;
                case EAttributeType.ButcheringYield: return ButcheringYield;
                case EAttributeType.FoodPoisonChance: return FoodPoisonChance;
                default: throw new ArgumentOutOfRangeException(nameof(attributeType), $"Not supported attribute {attributeType} for cooking.");
            }
        }
    }
}