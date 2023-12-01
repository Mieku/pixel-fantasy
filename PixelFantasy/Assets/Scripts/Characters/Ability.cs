using System;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

namespace Characters
{
    [Serializable]
    public class Ability
    {
        public int Level;
        public float Exp;

        public Ability(int level, float exp = 0)
        {
            Level = level;
            Exp = exp;
        }
    }

    [Serializable]
    public class Abilities
    {
        public Ability Strength;
        public Ability Vitality;
        public Ability Intelligence;
        public Ability Expertise;

        public Ability GetAbilityByType(AbilityType type)
        {
            switch (type)
            {
                case AbilityType.Strength:
                    return Strength;
                case AbilityType.Vitality:
                    return Vitality;
                case AbilityType.Intelligence:
                    return Intelligence;
                case AbilityType.Expertise:
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

    public enum AbilityType
    {
        Strength,
        Vitality,
        Intelligence,
        Expertise,
    }
}
