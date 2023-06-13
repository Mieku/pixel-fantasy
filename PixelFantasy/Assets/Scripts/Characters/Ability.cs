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
        public Ability Charisma;
        public Ability Dexterity;
        public Ability Intelligence;
        public Ability Toughness;

        public Ability GetAbilityByType(AbilityType type)
        {
            switch (type)
            {
                case AbilityType.Strength:
                    return Strength;
                case AbilityType.Charisma:
                    return Charisma;
                case AbilityType.Dexterity:
                    return Dexterity;
                case AbilityType.Intelligence:
                    return Intelligence;
                case AbilityType.Toughness:
                    return Toughness;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        [Button("Randomize Scores")]
        public void RandomizeScores()
        {
            Strength.Level = Random.Range(1, 21);
            Charisma.Level = Random.Range(1, 21);
            Dexterity.Level = Random.Range(1, 21);
            Intelligence.Level = Random.Range(1, 21);
            Toughness.Level = Random.Range(1, 21);
        }
    }

    public enum AbilityType
    {
        Strength,
        Charisma,
        Dexterity,
        Intelligence,
        Toughness,
    }
}
