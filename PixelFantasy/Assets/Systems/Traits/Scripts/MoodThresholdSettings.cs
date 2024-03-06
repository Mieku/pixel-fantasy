using UnityEngine;

namespace Systems.Traits.Scripts
{
    [CreateAssetMenu(menuName = "Settings/AI/Mood Threshold Settings", fileName = "MoodThresholdSettings")]
    public class MoodThresholdSettings : TraitSettings
    {
        public int EmotionalBreakdownThresholdChange;
    }
}
