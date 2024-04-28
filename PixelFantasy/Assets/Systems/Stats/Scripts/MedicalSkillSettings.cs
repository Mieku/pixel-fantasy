using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.Stats.Scripts
{
    [CreateAssetMenu(fileName = "Medical Skill Settings", menuName = "Skill System/Medical Skill Settings")]
    public class MedicalSkillSettings : SkillSettings
    {
        public override ESkillType SkillType => ESkillType.Medical; 

        protected override SkillLevelData CreateTableData(int level)
        {
            return new MedicalSkillLevelData { Level = level };
        }

        public override float GetValueForLevel(EAttributeType attributeType, int level)
        {
            if (level > 0 && level <= Table.Count)
            {
                var medicalData = (MedicalSkillLevelData)Table[level - 1];
                return medicalData.GetAttribute(attributeType);
            }
            Debug.LogError($"Level {level} is out of range for skill {SkillType}");
            return 0;
        }
    }
    
    [Serializable]
    public class MedicalSkillLevelData : SkillLevelData
    {
        [TableColumnWidth(100)]
        public float MedicalSpeed;
        
        [TableColumnWidth(100)]
        public float SurgerySuccessChance;
        
        [TableColumnWidth(100)]
        public float TendQuality;

        public override float GetAttribute(EAttributeType attributeType)
        {
            switch (attributeType)
            {
                case EAttributeType.MedicalSpeed: return MedicalSpeed;
                case EAttributeType.SurgerySuccessChance: return SurgerySuccessChance;
                case EAttributeType.TendQuality: return TendQuality;
                default: throw new ArgumentOutOfRangeException(nameof(attributeType), $"Not supported attribute {attributeType} for medical.");
            }
        }
    }
}
