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

        private void Start()
        {
            GameEvents.DayTick += DailyExpDecay;
        }

        private void OnDestroy()
        {
            GameEvents.DayTick -= DailyExpDecay;
        }

        public float GetSkillAttributeValue(ESkillType skillType, EAttributeType attributeType)
        {
            var settings = _skillSettings.Find(s => s.SkillType == skillType);
            if (settings != null)
            {
                return settings.GetValueForLevel(attributeType, _kinling.RuntimeData.GetLevelForSkill(skillType));
            }
            Debug.LogError($"SkillSettings not found for {skillType}");
            return 1;
        }
        
        public float GetActionWorkForSkill(ESkillType skillType, bool autoAddExp)
        {
            float baseActionWork = GameSettings.Instance.BaseWorkPerAction;
            
            // Modifiers
            float moddedWork = baseActionWork;
            moddedWork += baseActionWork * GetSkillAttributeValue(skillType, EAttributeType.WorkModifier);
            moddedWork += GetAttributeModifierBonus(EAttributeType.WorkModifier, baseActionWork, skillType);

            if (autoAddExp)
            {
                float expGain = moddedWork;
                expGain *= GameSettings.Instance.ExpSettings.BaseExpPerWork;
                AddExpToSkill(skillType, expGain);
            }
            
            return moddedWork;
        }

        public int DetermineAmountYielded(ESkillType skillType, int dropAmount)
        {
            float moddedYield = dropAmount;
            
            moddedYield += dropAmount * GetSkillAttributeValue(skillType, EAttributeType.YieldModifier);
            moddedYield += GetAttributeModifierBonus(EAttributeType.YieldModifier, dropAmount, skillType);

            return (int) Math.Ceiling(moddedYield);
        }

        public void AddExpToSkill(ESkillType skillType, float amount)
        {
            float moddedExp = amount;
            
            moddedExp += amount * GetSkillAttributeValue(ESkillType.Intelligence, EAttributeType.LearningModifier);
            moddedExp += GetAttributeModifierBonus(EAttributeType.LearningModifier, amount, ESkillType.Intelligence);
            
            _kinling.RuntimeData.StatsData.AddExpToSkill(skillType, moddedExp);
        }

        private void DailyExpDecay()
        {
            float decayModifier = _kinling.RuntimeData.GetTotalAttributeModifier(EAttributeType.SkillDecay, ESkillType.Intelligence);
            _kinling.RuntimeData.StatsData.DoDailyExpDecay(decayModifier);
        }

        public float GetAttributeModifierBonus(EAttributeType attributeType, float originalAmount, ESkillType? skillType)
        {
            var totalModifier = _kinling.RuntimeData.GetTotalAttributeModifier(attributeType, skillType);
            var result = originalAmount * totalModifier;
            return result;
        }
    }
}