using System;
using Characters;
using Systems.Mood.Scripts;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    [Serializable]
    public class PermanentMoodModifier : Modifier
    {
        public override EModifierType ModifierType => EModifierType.PermanentMood;
        public EmotionSettings PermanentEmotion;
        
        public override void ApplyModifier(KinlingData kinlingData)
        {
            if (!kinlingData.Mood.HasEmotion(PermanentEmotion))
            {
                kinlingData.Mood.ApplyEmotion(PermanentEmotion);
            }
        }

        public override string GetModifierString()
        {
            string change = PermanentEmotion.MoodModifier.ToString();
            if (PermanentEmotion.MoodModifier > 0)
            {
                change = $"+{PermanentEmotion.MoodModifier}";
            }
            
            return $"Mood {change}";
        }
    }
}
