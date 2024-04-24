using System;
using System.Collections.Generic;
using Characters;

namespace Systems.Stats.Scripts
{
    [Serializable]
    public class SkillModifier : Modifier
    {
        public override EModifierType ModifierType => EModifierType.Skill;
        public ESkillType SkillType;
        public int LevelBoost;
        public ESkillPassion PassionOverride;
        
        public override void ApplyModifier(KinlingData kinlingData)
        {
            int curLevel = kinlingData.GetLevelForSkill(SkillType);
            int boostedLevel = Math.Clamp(curLevel + LevelBoost, 0, 10);
            
            kinlingData.SetLevelForSkill(SkillType, boostedLevel);

            if (PassionOverride != ESkillPassion.None)
            {
                var skill = kinlingData.StatsData.GetSkillByType(SkillType);
                skill.Passion = PassionOverride;
            }
        }

        public override string GetModifierString()
        {
            string modifier;
            if (LevelBoost > 0)
            {
                modifier = $"+{LevelBoost}";
            }
            else
            {
                modifier = $"{LevelBoost}";
            }
            
            return $"{SkillType.GetDescription()} {modifier}";
        }
    }

    [Serializable]
    public class IncapableSkillModifier : Modifier
    {
        public override EModifierType ModifierType => EModifierType.IncapableSkill;
        public ESkillType IncapableSkill;
        
        public override void ApplyModifier(KinlingData kinlingData)
        {
            kinlingData.SetLevelForSkill(IncapableSkill, 0);
        }

        public override string GetModifierString()
        {
            return $"Incapable Skill: {IncapableSkill.GetDescription()}";
        }
    }

    [Serializable]
    public class AddTraitModifier : Modifier
    {
        public override EModifierType ModifierType => EModifierType.AddTrait;
        public Trait TraitToAdd;
        
        public override void ApplyModifier(KinlingData kinlingData)
        {
            kinlingData.AssignTraits(new List<Trait>() { TraitToAdd });
        }

        public override string GetModifierString()
        {
            return $"Add Trait: {TraitToAdd.TraitName}";
        }
    }
}
