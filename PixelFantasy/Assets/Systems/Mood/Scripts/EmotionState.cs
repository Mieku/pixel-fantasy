using UnityEngine;

namespace Systems.Mood.Scripts
{
    public class EmotionState
    {
        public Emotion LinkedEmotion;
        public float RemainingTimeMins;

        public int MoodModifier => LinkedEmotion.MoodModifier;

        public EmotionState(Emotion emotion)
        {
            LinkedEmotion = emotion;
            if (!emotion.IsIndefinite)
            {
                RemainingTimeMins = emotion.Duration * 60f;
            }
        }

        public void ResetDuration()
        {
            if (!LinkedEmotion.IsIndefinite)
            {
                RemainingTimeMins = LinkedEmotion.Duration * 60f;
            }
        }
        
        /// <returns>Whether the emotion's duration is complete</returns>
        public bool TickEmotionState()
        {
            if (!LinkedEmotion.IsIndefinite)
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
                if (LinkedEmotion.IsIndefinite)
                {
                    return "";
                }
                
                var timeLeftHours = (int)(RemainingTimeMins / 60);
                int remainderMins = (int)(RemainingTimeMins / 60f - timeLeftHours) * 60;

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
