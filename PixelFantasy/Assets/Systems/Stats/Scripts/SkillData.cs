using System;
using Databrain.Attributes;
using ScriptableObjects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.Stats.Scripts
{
    [Serializable]
    public class SkillData
    {
        [ExposeToInspector, DatabrainSerialize]
        public int Level;
        
        [ExposeToInspector, DatabrainSerialize]
        public float Exp;

        [ExposeToInspector, DatabrainSerialize]
        public ESkillPassion Passion;

        public void RandomlyAssignPassion()
        {
            // Generate a random number between 1 and 100
            int diceRoll = Random.Range(1, 101);

            // Retrieve the probabilities from the game settings
            int minorChance = GameSettings.Instance.ExpSettings.ChanceForMinorPassion;
            int majorChance = GameSettings.Instance.ExpSettings.ChanceForMajorPassion;  // Fixed variable name here

            // Determine the passion level based on the dice roll
            if (diceRoll <= 100 - (minorChance + majorChance))
            {
                Passion = ESkillPassion.None; // Most common scenario with no passion
            }
            else if (diceRoll <= 100 - majorChance)
            {
                Passion = ESkillPassion.Minor; // Less common, minor passion
            }
            else
            {
                Passion = ESkillPassion.Major; // Least common, major passion
            }
        }
    }

    public enum ESkillPassion
    {
        None = 0,
        Minor = 1,
        Major = 2
    }
}
