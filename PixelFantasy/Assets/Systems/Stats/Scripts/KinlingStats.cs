using System.Collections.Generic;
using Characters;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    public class KinlingStats : MonoBehaviour
    {
        [SerializeField] private Kinling _kinling;
        [SerializeField] private List<SkillSettings> _skillSettings;

        public float GetAttributeValue(ESkillType skillType, EAttributeType attributeType)
        {
            var settings = _skillSettings.Find(s => s.SkillType == skillType);
            if (settings != null)
            {
                return settings.GetValueForLevel(attributeType, _kinling.RuntimeData.GetLevelForSkill(skillType));
            }
            Debug.LogError($"SkillSettings not found for {skillType}");
            return 0;
        }
    }
}