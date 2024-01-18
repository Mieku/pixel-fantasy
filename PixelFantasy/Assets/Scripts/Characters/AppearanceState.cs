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
        public HairData Hair;

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

        public AppearanceState(Gender gender, AppearanceState motherAppearance, AppearanceState fatherAppearance)
        {
            EyeColour = Helper.RollDice(50) ? motherAppearance.EyeColour : fatherAppearance.EyeColour;
            Race = motherAppearance.Race;
            SkinTone = Helper.RollDice(50) ? motherAppearance.SkinTone : fatherAppearance.SkinTone;
            Hair = GetRandomHairByGender(gender);
        }
        
        public void RandomizeAppearance()
        {
            // Make a random one
            SkinTone = Race.GetRandomSkinTone();

            // Create a random HDR color
            EyeColour = Race.GetRandomEyeColour();
        }

        private HairData GetRandomHairByGender(Gender gender)
        {
            var hair = Race.GetRandomHairByGender(gender);
            return hair;
        }
    }
}
