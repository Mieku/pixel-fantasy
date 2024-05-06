// using System;
// using Managers;
// using ScriptableObjects;
// using Systems.Appearance.Scripts;
// using UnityEngine;
// using Random = UnityEngine.Random;
//
// namespace Characters
// {
//     [Serializable]
//     public class AppearanceData
//     {
//         public SkinToneSettings SkinTone;
//         public Gender Gender;
//         public RaceSettings Race;
//         public HairColourOption Hair;
//         public EyeSettings Eyes;
//         public Sprite Eyelashes;
//         public Sprite Eyebrows;
//
//         public AppearanceData(RaceSettings race, Gender gender)
//         {
//             Race = race;
//             Gender = gender;
//             
//             SkinTone = Race.GetRandomSkinToneOld();
//             Eyes = Race.GetRandomEyeColourOld();
//             Hair = GetRandomHairByGender(gender);
//             Eyelashes = Race.GetRandomEyelashesByGender(gender);
//             Eyebrows = Race.GetRandomEyebrowsByGender(gender);
//         }
//         
//         public AppearanceData(AppearanceData other)
//         {
//             Race = other.Race;
//             Eyes = other.Eyes;
//             SkinTone = other.SkinTone;
//             Gender = other.Gender;
//         }
//
//         public AppearanceData(Gender gender, AppearanceData motherAppearance, AppearanceData fatherAppearance)
//         {
//             Eyes = Helper.RollDice(50) ? motherAppearance.Eyes : fatherAppearance.Eyes;
//             Race = motherAppearance.Race;
//             SkinTone = Helper.RollDice(50) ? motherAppearance.SkinTone : fatherAppearance.SkinTone;
//             Hair = GetRandomHairByGender(gender);
//         }
//         
//         public void RandomizeAppearance()
//         {
//             // Make a random one
//             SkinTone = Race.GetRandomSkinToneOld();
//
//             // Create a random HDR color
//             Eyes = Race.GetRandomEyeColourOld();
//         }
//
//         private HairColourOption GetRandomHairByGender(Gender gender)
//         {
//             var hair = Race.GetRandomHairByGenderOld(gender);
//             var hairColour = hair.GetRandomHairColour();
//             return hairColour;
//         }
//     }
// }
