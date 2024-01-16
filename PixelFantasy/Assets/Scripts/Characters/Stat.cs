using System;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

namespace Characters
{
    [Serializable]
    public class Stat
    {
        public int Level;
        public float Exp;

        public Stat(int level, float exp = 0)
        {
            Level = level;
            Exp = exp;
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
    }

    public enum StatType
    {
        Strength,
        Vitality,
        Intelligence,
        Expertise,
    }
}
