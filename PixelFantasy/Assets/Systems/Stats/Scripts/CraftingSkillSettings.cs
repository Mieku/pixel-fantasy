using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    [CreateAssetMenu(fileName = "Crafting Skill Settings", menuName = "Skill System/Crafting Skill Settings")]
    public class CraftingSkillSettings : SkillSettings
    {
        public override ESkillType SkillType => ESkillType.Crafting; 

        protected override SkillLevelData CreateTableData(int level)
        {
            return new CraftingSkillLevelData { Level = level };
        }

        public override float GetValueForLevel(EAttributeType attributeType, int level)
        {
            if (level > 0 && level <= Table.Count)
            {
                var craftingData = (CraftingSkillLevelData)Table[level - 1];
                return craftingData.GetAttribute(attributeType);
            }
            Debug.LogError($"Level {level} is out of range for skill {SkillType}");
            return 0;
        }
    }
    
    [Serializable]
    public class CraftingSkillLevelData : SkillLevelData
    {
        [TableColumnWidth(100)]
        public float CraftingWorkModifier;

        public override float GetAttribute(EAttributeType attributeType)
        {
            switch (attributeType)
            {
                case EAttributeType.WorkModifier: return CraftingWorkModifier;
                default: throw new ArgumentOutOfRangeException(nameof(attributeType), $"Not supported attribute {attributeType} for crafting.");
            }
        }
    }
}
