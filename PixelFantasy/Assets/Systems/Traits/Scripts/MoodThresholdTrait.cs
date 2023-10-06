using UnityEngine;

namespace Systems.Traits.Scripts
{
    [CreateAssetMenu(menuName = "AI/Trait/Mood Threshold Trait", fileName = "MoodThresholdTrait")]
    public class MoodThresholdTrait : Trait
    {
        public int EmotionalBreakdownThresholdChange;
    }
}
