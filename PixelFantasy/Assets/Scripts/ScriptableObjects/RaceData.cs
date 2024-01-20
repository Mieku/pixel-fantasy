using System;
using System.Collections.Generic;
using System.IO;
using Characters;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "RaceData", menuName = "Races/RaceData", order = 1)]
    public class RaceData : ScriptableObject
    {
        public string RaceName;
        public List<KinlingSkinToneData> AvailableSkinTones = new List<KinlingSkinToneData>();
        [ColorUsage(true, true)] public List<Color> AvailableEyeColours = new List<Color>();
        
        [SerializeField] private BodyData _childBodyData;
        [SerializeField] private BodyData _adultBodyData;
        [SerializeField] private BodyData _seniorBodyData;

        [SerializeField] private List<HairData> _maleHairOptions = new List<HairData>();
        [SerializeField] private List<HairData> _femaleHairOptions = new List<HairData>();
        
        [SerializeField] private List<string> _maleFirstNames = new List<string>();
        [SerializeField] private List<string> _femaleFirstNames = new List<string>();
        [SerializeField] private List<string> _lastNames = new List<string>();
        
        [SerializeField] private string _maleNamesFilePath;
        [SerializeField] private string _femaleNamesFilePath;
        [SerializeField] private string _surnamesFilePath;

        [SerializeField] private RacialAgeData _racialAgeData;
        
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
        
        public KinlingSkinToneData GetRandomSkinTone()
        {
            int index = Random.Range(0, AvailableSkinTones.Count);
            return AvailableSkinTones[index];
        }
        
        public Color GetRandomEyeColour()
        {
            int index = Random.Range(0, AvailableEyeColours.Count);
            return AvailableEyeColours[index];
        }

        public HairData GetRandomHairByGender(Gender gender)
        {
            if (gender == Gender.Female)
            {
                int index = Random.Range(0, _femaleHairOptions.Count);
                return _femaleHairOptions[index];
            }
            else
            {
                int index = Random.Range(0, _maleHairOptions.Count);
                return _maleHairOptions[index];
            }
        }

        public BodyData GetBodyDataByMaturity(EMaturityStage maturityStage)
        {
            switch (maturityStage)
            {
                case EMaturityStage.Child:
                    return _childBodyData;
                case EMaturityStage.Adult:
                    return _adultBodyData;
                case EMaturityStage.Senior:
                    return _seniorBodyData;
                default:
                    throw new ArgumentOutOfRangeException(nameof(maturityStage), maturityStage, null);
            }
        }

        public string GetRandomFirstName(Gender gender)
        {
            return gender == Gender.Female 
                ? _femaleFirstNames[Random.Range(0, _femaleFirstNames.Count)] 
                : _maleFirstNames[Random.Range(0, _maleFirstNames.Count)];
        }

        public string GetRandomLastName()
        {
            return _lastNames[Random.Range(0, _lastNames.Count)];
        }

        public RacialAgeData RacialAgeData => _racialAgeData;
    }
}
