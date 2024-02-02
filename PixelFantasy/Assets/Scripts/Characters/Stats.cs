using System;
using ParadoxNotion.Design;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Characters
{
    public class Stats : MonoBehaviour
    {
        public Stat Strength;
        public Stat Vitality;
        public Stat Intelligence;
        public Stat Expertise;

        public void Init(StatsData data)
        {
            Strength = new Stat(StatType.Strength, data.GetStatByType(StatType.Strength).Level);
            Vitality = new Stat(StatType.Vitality, data.GetStatByType(StatType.Vitality).Level);
            Intelligence = new Stat(StatType.Intelligence, data.GetStatByType(StatType.Intelligence).Level);
            Expertise = new Stat(StatType.Expertise, data.GetStatByType(StatType.Expertise).Level);
        }
        
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
    }
    
    [Serializable]
    public class Stat
    {
        public int Level;
        public StatType Type;

        public Stat(StatType type, int level)
        {
            Level = level;
            Type = type;
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

    [Serializable]
    public class StatsData
    {
        public Stat Strength;
        public Stat Vitality;
        public Stat Intelligence;
        public Stat Expertise;
        
        public StatsData(StatsData motherStats, StatsData fatherStats)
        {
            Strength = new Stat(StatType.Strength, GetGeneticStatValue(StatType.Strength, motherStats, fatherStats));
            Vitality = new Stat(StatType.Vitality, GetGeneticStatValue(StatType.Vitality, motherStats, fatherStats));
            Intelligence = new Stat(StatType.Intelligence, GetGeneticStatValue(StatType.Intelligence, motherStats, fatherStats));
            Expertise = new Stat(StatType.Expertise, GetGeneticStatValue(StatType.Expertise, motherStats, fatherStats));
        }
        
        private int GetGeneticStatValue(StatType type, StatsData motherStat, StatsData fatherStat)
        {
            if (Helper.RollDice(50))
            {
                // Inherit
                var level = Helper.RollDice(50) ? motherStat.GetStatByType(type).Level : fatherStat.GetStatByType(type).Level;
                return level;
            }
            else
            {
                // Not Inherited
                var statLvl = Random.Range(2, 8);
                return statLvl;
            }
        }
        
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
    }
}
