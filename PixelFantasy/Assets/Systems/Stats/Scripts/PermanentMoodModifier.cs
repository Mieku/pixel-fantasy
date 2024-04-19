using System;
using Characters;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    [Serializable]
    public class PermanentMoodModifier : Modifier
    {
        public override EModifierType ModifierType => EModifierType.PermanentMood;
        public string MoodChangeTitle;
        public int MoodChange;
        
        public override void ApplyModifier(KinlingData kinlingData)
        {
            Debug.LogError("Not built yet");
        }
    }
}
