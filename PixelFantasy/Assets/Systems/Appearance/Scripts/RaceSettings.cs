using System;
using System.Collections.Generic;
using System.IO;
using Characters;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Systems.Appearance.Scripts
{
    [CreateAssetMenu(fileName = "RaceSettings", menuName = "Settings/Kinlings/Race Settings")]
    public class RaceSettings : ScriptableObject
    {
        public string RaceName;
        public List<SkinToneSettings> AvailableSkinTones = new List<SkinToneSettings>();
        public List<EyeSettings> EyeOptions;
        public List<Sprite> MaleEyelashOptions;
        public List<Sprite> FemaleEyelashOptions;
        public List<Sprite> MaleEyebrowOptions;
        public List<Sprite> FemaleEyebrowOptions;
        
        [FormerlySerializedAs("_childBodyData")] [SerializeField] private BodySettings _childBodySettings;
        [FormerlySerializedAs("_adultBodyData")] [SerializeField] private BodySettings _adultBodySettings;
        [FormerlySerializedAs("_seniorBodyData")] [SerializeField] private BodySettings _seniorBodySettings;

        [SerializeField] private List<HairSettings> _maleHairOptions = new List<HairSettings>();
        [SerializeField] private List<HairSettings> _femaleHairOptions = new List<HairSettings>();
        
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
        
        public SkinToneSettings GetRandomSkinTone()
        {
            int index = Random.Range(0, AvailableSkinTones.Count);
            return AvailableSkinTones[index];
        }
        
        public EyeSettings GetRandomEyeColour()
        {
            int index = Random.Range(0, EyeOptions.Count);
            return EyeOptions[index];
        }

        public HairSettings GetRandomHairByGender(Gender gender)
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

        public Sprite GetRandomEyelashesByGender(Gender gender)
        {
            switch (gender)
            {
                case Gender.Male:
                    return MaleEyelashOptions[Random.Range(0, MaleEyelashOptions.Count)];
                case Gender.Female:
                    return FemaleEyelashOptions[Random.Range(0, FemaleEyelashOptions.Count)];
                default:
                    throw new ArgumentOutOfRangeException(nameof(gender), gender, null);
            }
        }
        
        public Sprite GetRandomEyebrowsByGender(Gender gender)
        {
            switch (gender)
            {
                case Gender.Male:
                    return MaleEyebrowOptions[Random.Range(0, MaleEyebrowOptions.Count)];
                case Gender.Female:
                    return FemaleEyebrowOptions[Random.Range(0, FemaleEyebrowOptions.Count)];
                default:
                    throw new ArgumentOutOfRangeException(nameof(gender), gender, null);
            }
        }

        public BodySettings GetBodyDataByMaturity(EMaturityStage maturityStage)
        {
            switch (maturityStage)
            {
                case EMaturityStage.Child:
                    return _childBodySettings;
                case EMaturityStage.Adult:
                    return _adultBodySettings;
                case EMaturityStage.Senior:
                    return _seniorBodySettings;
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
