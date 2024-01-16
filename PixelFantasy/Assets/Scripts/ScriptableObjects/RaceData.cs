using System;
using System.Collections.Generic;
using Characters;
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
    }
}
