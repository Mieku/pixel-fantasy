using System;
using UnityEngine;

namespace Systems.Mood.Scripts
{
    [Serializable]
    public class EmotionState
    {
        public EmotionSettings LinkedEmotionSettings;
        public float RemainingTimeMins;

        public int MoodModifier => LinkedEmotionSettings.MoodModifier;

        public EmotionState(EmotionSettings emotionSettings)
        {
            LinkedEmotionSettings = emotionSettings;
            if (!emotionSettings.IsIndefinite)
            {
                RemainingTimeMins = emotionSettings.Duration * 60f;
            }
        }

        public void ResetDuration()
        {
            if (!LinkedEmotionSettings.IsIndefinite)
            {
                RemainingTimeMins = LinkedEmotionSettings.Duration * 60f;
            }
        }
        
        /// <returns>Whether the emotion's duration is complete</returns>
        public bool TickEmotionState()
        {
            if (!LinkedEmotionSettings.IsIndefinite)
            {
                RemainingTimeMins--;
                return RemainingTimeMins <= 0;
            }
            
            return false;
        }

        public string MoodModifierDisplay
        {
            get
            {
                if (MoodModifier > 0)
                {
                    return $"+{MoodModifier}";
                }
                
                return $"{MoodModifier}";
            }
        }

        public string TimeLeftHoursDisplay
        {
            get
            {
                if (LinkedEmotionSettings.IsIndefinite)
                {
                    return "";
                }
                
                var timeLeftHours = (int)(RemainingTimeMins / 60);
                int remainderMins = (int)RemainingTimeMins;

                if (timeLeftHours > 0)
                {
                    return $"{timeLeftHours}h";
                }

                if (remainderMins > 0)
                {
                    return $"{remainderMins}m";
                }
                
                return "";
            }
        }
    }
}
