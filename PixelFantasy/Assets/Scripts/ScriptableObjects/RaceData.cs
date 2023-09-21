using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "RaceData", menuName = "Races/RaceData", order = 1)]
    public class RaceData : ScriptableObject
    {
        public string RaceName;
        public List<KinlingSkinToneData> AvailableSkinTones = new List<KinlingSkinToneData>();
        [ColorUsage(true, true)] public List<Color> AvailableEyeColours = new List<Color>();
        
        
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
    }
}
