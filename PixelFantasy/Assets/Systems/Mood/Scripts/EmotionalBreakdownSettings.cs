using System;
using Systems.SmartObjects.Scripts;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Systems.Mood.Scripts
{
    [CreateAssetMenu(fileName = "EmotionalBreakdownSettings", menuName = "Settings/AI/Emotional Breakdown Settings")]
    public class EmotionalBreakdownSettings : ScriptableObject
    {
        public MoodData.EMoodBreakType BreakdownType;
        public int MinRecoveryHours = 12;
        public int MaxRecoveryHours = 24;

        public const int MIN_MINOR_BREAK_TIME = 11520; // 8 Days, in Minutes
        public const int MAX_MINOR_BREAK_TIME = 17280; // 12 Days, in Minutes
        
        public const int MIN_MAJOR_BREAK_TIME = 2880; // 2 Days, in Minutes
        public const int MAX_MAJOR_BREAK_TIME = 5760; // 4 Days, in Minutes
        
        public const int MIN_EXTREME_BREAK_TIME = 720; // 0.5 Day, in Minutes
        public const int MAX_EXTREME_BREAK_TIME = 1440; // 1.0 Day, in Minutes

        public string BreakdownTaskId;

        public int MaxMinutesUntilBreakdown
        {
            get
            {
                switch (BreakdownType)
                {
                    case MoodData.EMoodBreakType.MinorBreak:
                        return Random.Range(MIN_MINOR_BREAK_TIME, MAX_MINOR_BREAK_TIME);
                    case MoodData.EMoodBreakType.MajorBreak:
                        return Random.Range(MIN_MAJOR_BREAK_TIME, MAX_MAJOR_BREAK_TIME);
                    case MoodData.EMoodBreakType.ExtremeBreak:
                        return Random.Range(MIN_EXTREME_BREAK_TIME, MAX_EXTREME_BREAK_TIME);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public int RecoveryTimeMins
        {
            get
            {
                int minMins = MinRecoveryHours * 60;
                int maxMins = MaxRecoveryHours * 60;
                return Random.Range(minMins, maxMins);
            }
        }
    }
}
