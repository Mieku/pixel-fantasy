using System;
using ParadoxNotion.Design;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

namespace Characters
{
    [Serializable]
    public class Stat
    {
        public int Level;

        public Stat(int level)
        {
            Level = level;
        }
    }

    [Serializable]
    public class Stats
    {
        public Stat Strength;
        public Stat Vitality;
        public Stat Intelligence;
        public Stat Expertise;

        public Stat GetStatByType(StatType type)
        {
            switch (type)
            {
                case StatType.Strength:
                    return Strength;
                case StatType.Vitality:
                    return Vitality;
                case StatType.Intelligence:
                    return Intelligence;
                case StatType.Expertise:
                    return Expertise;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        [Button("Randomize Scores")]
        public void RandomizeScores()
        {
            Strength.Level = Random.Range(2, 8);
            Vitality.Level = Random.Range(2, 8);
            Intelligence.Level = Random.Range(2, 8);
            Expertise.Level = Random.Range(2, 8);
        }

        private Stat GetGeneticStat(Stat motherStat, Stat fatherStat)
        {
            Stat stat;
            if (Helper.RollDice(50))
            {
                // Inherit
                stat = Helper.RollDice(50) ? motherStat : fatherStat;
                return stat;
            }
            else
            {
                // Not Inherited
                var statLvl = Random.Range(2, 8);
                stat = new Stat(statLvl);
                return stat;
            }
        }

        public Stats(Stats motherStats, Stats fatherStats)
        {
            Strength = GetGeneticStat(motherStats.GetStatByType(StatType.Strength),
                fatherStats.GetStatByType(StatType.Strength));
            
            Vitality = GetGeneticStat(motherStats.GetStatByType(StatType.Vitality),
                fatherStats.GetStatByType(StatType.Vitality));
            
            Intelligence = GetGeneticStat(motherStats.GetStatByType(StatType.Intelligence),
                fatherStats.GetStatByType(StatType.Intelligence));
            
            Expertise = GetGeneticStat(motherStats.GetStatByType(StatType.Expertise),
                fatherStats.GetStatByType(StatType.Expertise));
        }
    }

    public enum StatType
    {
        [Description("Strength")] Strength,
        [Description("Vitality")] Vitality,
        [Description("Intelligence")] Intelligence,
        [Description("Expertise")]  Expertise,
    }

    [Serializable]
    public class StatModifier
    {
        public StatType StatType;
        public int Modifier;

        public string ModifierString
        {
            get
            {
                if (Modifier < 0)
                {
                    return $"-{Modifier}";
                }

                if (Modifier > 0)
                {
                    return $"+{Modifier}";
                }
                
                return $"{Modifier}";
            }
        }

        public string StatName => StatType.GetDescription();
    }
}
