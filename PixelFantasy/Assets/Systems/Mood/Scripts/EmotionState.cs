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
    }
}
