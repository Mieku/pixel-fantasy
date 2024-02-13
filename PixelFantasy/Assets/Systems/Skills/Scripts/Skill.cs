using System;
using System.ComponentModel;

namespace Systems.Skills.Scripts
{
    [Serializable]
    public class Skill
    {
        public SkillType SkillType;
        public int Amount;

        public Skill(SkillType skillType, int amount)
        {
            SkillType = skillType;
            Amount = amount;
        }
        
        public override string ToString()
        {
            if (Amount > 0)
            {
                return $"{SkillType.GetDescription()}: +{Amount}";
            }
            else if (Amount < 0)
            {
                return $"{SkillType.GetDescription()}: -{Amount}";
            }
            else
            {
                return $"{SkillType.GetDescription()}: {Amount}";
            }
        }

        public string AmountString()
        {
            if (Amount > 0)
            {
                return $"+{Amount}";
            }
            else if (Amount < 0)
            {
                return $"-{Amount}";
            }
            else
            {
                return $"{Amount}";
            }
        }
    }

    [Serializable]
    public enum SkillType
    {
        [Description("None")] None = 0,
        [Description("Woodcutting")] Woodcutting = 1,
        [Description("Mining")] Mining = 2,
        [Description("Cooking")] Cooking = 3, 
        [Description("Farming")] Farming = 4,
        [Description("Medicine")] Medicine = 5,
        [Description("Construction")] Construction = 6,
        [Description("Gathering")] Gathering = 7,
        [Description("Carpentry")] Carpentry = 8,
        [Description("Blacksmithing")] Blacksmithing = 9,
    }
}
