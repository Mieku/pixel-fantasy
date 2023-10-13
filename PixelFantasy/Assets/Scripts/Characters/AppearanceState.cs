using System;
using Managers;
using ScriptableObjects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Characters
{
    [Serializable]
    public class AppearanceState
    {
        [ColorUsage(true, true)] public Color EyeColour;
        public KinlingSkinToneData SkinTone;
        public Gender Gender;
        public RaceData Race;

        public AppearanceState(RaceData race, Gender gender)
        {
            Race = race;
            Gender = gender;
        }
        
        public AppearanceState(AppearanceState other)
        {
            Race = other.Race;
            EyeColour = other.EyeColour;
            SkinTone = other.SkinTone;
            Gender = other.Gender;
        }
        
        public void RandomizeAppearance()
        {
            // Make a random one
            SkinTone = Race.GetRandomSkinTone();

            // int genderIndex = Random.Range(0, 2);
            // if (genderIndex == 0)
            // {
            //     Gender = Gender.Male;
            // }
            // else
            // {
            //     Gender = Gender.Female;
            // }

            // Create a random HDR color
            EyeColour = Race.GetRandomEyeColour();
        }
    }
}
