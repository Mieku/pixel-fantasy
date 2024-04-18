using System;
using System.Collections.Generic;
using Characters;
using ScriptableObjects;
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
            return 1;
        }
        
        public float GetActionWorkForSkill(ESkillType skillType)
        {
            float baseActionWork = GameSettings.Instance.BaseWorkPerAction;
            float workModifier = GetAttributeValue(skillType, EAttributeType.WorkModifier);
            float result = baseActionWork * workModifier;
            
            return result;
        }

        public int DetermineAmountYielded(ESkillType skillType, int dropAmount)
        {
            float yield = GetAttributeValue(skillType, EAttributeType.YieldModifier);
            int result = (int) Math.Ceiling(dropAmount * yield);
            return result;
        }

        public void AddExpToSkill(ESkillType skillType, int amount)
        {
            float modifier = GetAttributeValue(ESkillType.Intelligence, EAttributeType.LearningModifier);
            int modifiedAmount = (int) Math.Ceiling(amount * modifier);
            _kinling.RuntimeData.StatsData.AddExpToSkill(skillType, modifiedAmount);
        }
    }
}