using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Systems.Stats.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.Appearance.Scripts
{
    [CreateAssetMenu(fileName = "RaceSettings", menuName = "Settings/Kinlings/Race Settings")]
    public class RaceSettings : ScriptableObject
    {
        public string RaceName;
        public List<Color> SkinTones = new List<Color>();
        public List<Color> HairColours = new List<Color>();
        public List<HairSettings> MaleHairStyles = new List<HairSettings>();
        public List<HairSettings> FemaleHairStyles = new List<HairSettings>();
        public List<HairSettings> BeardStyles = new List<HairSettings>();
        public List<Color> EyesColours = new List<Color>();
        
        [SerializeField] private List<string> _maleFirstNames = new List<string>();
        [SerializeField] private List<string> _femaleFirstNames = new List<string>();
        [SerializeField] private List<string> _lastNames = new List<string>();
        
        [SerializeField] private string _maleNamesFilePath;
        [SerializeField] private string _femaleNamesFilePath;
        [SerializeField] private string _surnamesFilePath;

        [SerializeField] private RacialAgeData _racialAgeData;

        [SerializeField] private List<Trait> _allTraits = new List<Trait>();
        [SerializeField] private List<History> _allHistories = new List<History>();
        
        [Button("Load Male Names from file")]
        private void LoadMaleNamesFromFile()
        {
            LoadNamesFromFile(ref _maleFirstNames, _maleNamesFilePath);
        }
        
        [Button("Load Female Names from file")]
        private void LoadFemaleNamesFromFile()
        {
            LoadNamesFromFile(ref _femaleFirstNames, _femaleNamesFilePath);
        }
        
        [Button("Load Surnames from file")]
        private void LoadSurnamesFromFile()
        {
            LoadNamesFromFile(ref _lastNames, _surnamesFilePath);
        }

        private void LoadNamesFromFile(ref List<string> namesList, string filePath)
        {
            if (File.Exists(filePath))
            {
                string[] names = File.ReadAllLines(filePath);
                namesList.AddRange(names);
            }
            else
            {
                Debug.LogError("File not found: " + filePath);
            }
        }
        
        public Color32 GetRandomSkinTone()
        {
            int index = Random.Range(0, SkinTones.Count);
            return SkinTones[index];
        }
        
        public Color32 GetRandomEyeColour()
        {
            int index = Random.Range(0, EyesColours.Count);
            return EyesColours[index];
        }
        
        public Color32 GetRandomHairColour()
        {
            int index = Random.Range(0, HairColours.Count);
            return HairColours[index];
        }
        
        public HairSettings GetRandomHairStyleByGender(EGender gender)
        {
            if (gender == EGender.Female)
            {
                int index = Random.Range(0, FemaleHairStyles.Count);
                return FemaleHairStyles[index];
            }
            else
            {
                int index = Random.Range(0, MaleHairStyles.Count);
                return MaleHairStyles[index];
            }
        }
        
        public HairSettings GetRandomBeardStyle()
        {
            if (BeardStyles.Count == 0)
            {
                return null;
            }

            if (Helper.RollDice(50)) // 50% chance of having a beard
            {
                int index = Random.Range(0, BeardStyles.Count);
                return BeardStyles[index];
            }
            else
            {
                return null;
            }
        }
        
        public string GetRandomFirstName(EGender gender)
        {
            return gender == EGender.Female 
                ? _femaleFirstNames[Random.Range(0, _femaleFirstNames.Count)] 
                : _maleFirstNames[Random.Range(0, _maleFirstNames.Count)];
        }

        public string GetRandomLastName()
        {
            return _lastNames[Random.Range(0, _lastNames.Count)];
        }

        public RacialAgeData RacialAgeData => _racialAgeData;

        public History GetRandomHistory()
        {
            int randomIndex = Random.Range(0, _allHistories.Count);
            return _allHistories[randomIndex];
        }

        // Method to get a specified number of random, distinct, and non-conflicting traits
        public List<Trait> GetRandomTraits(int amount)
        {
            List<Trait> selectedTraits = new List<Trait>();
            List<Trait> tempTraits = new List<Trait>(_allTraits); // Copy all traits to a temporary list
            HashSet<Trait> disallowedTraits = new HashSet<Trait>(); // To track traits that cannot be selected

            if (amount > _allTraits.Count)
            {
                Debug.LogError("Requested more traits than available, returning fewer traits.");
                amount = _allTraits.Count;
            }

            while (selectedTraits.Count < amount && tempTraits.Count > 0)
            {
                int randomIndex = Random.Range(0, tempTraits.Count); // Get a random index
                Trait potentialTrait = tempTraits[randomIndex];

                // Check if the potential trait is in the disallowed list
                if (!disallowedTraits.Contains(potentialTrait))
                {
                    selectedTraits.Add(potentialTrait); // Add the selected trait
                    // Add all no-pair traits of the selected trait to the disallowed set
                    foreach (Trait noPair in potentialTrait.IncompatibleTraits)
                    {
                        disallowedTraits.Add(noPair);
                    }

                    // Also disallow selecting this trait again (if it's not inherently part of its own NoPairTraits)
                    disallowedTraits.Add(potentialTrait);
                }

                tempTraits.RemoveAt(randomIndex); // Remove the potential trait from consideration
            }

            if (selectedTraits.Count < amount)
            {
                Debug.LogWarning("Could not select the requested number of traits due to conflict restrictions.");
            }

            return selectedTraits;
        }
    }
    
    [Serializable]
    public class RacialAgeData
    {
        public int ChildMaxAge;
        public int AdultMaxAge;
        public int LifeExpectancy;
    }
    
    public enum EMaturityStage
    {
        Child = 0,
        Adult = 1,
        Senior = 2,
    }
}
