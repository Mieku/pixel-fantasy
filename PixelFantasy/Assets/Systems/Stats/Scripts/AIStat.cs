using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Systems.Mood.Scripts;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    [CreateAssetMenu(menuName = "AI/Stat", fileName = "AIStat")]
    public class AIStat : ScriptableObject
    {
        [field: SerializeField] public string DisplayName { get; protected set; }
        [field: SerializeField] public string Description { get; protected set; }
        [field: SerializeField] public Sprite StatIcon { get; protected set; }
        [field: SerializeField] public bool IsVisible { get; protected set; } = true;
        [field: SerializeField, Range(0f, 1f)] public float InitialValue { get; protected set; } = 0.5f;
        [field: SerializeField] public float DailyDecayRate { get; protected set; } = 0.25f;
        [field: SerializeField] public AnimationCurve IntensityCurve { get; protected set; }

        [Header("Thresholds")]
        [field: SerializeField] public StatThreshold CriticalThreshold;
        [field: SerializeField] public StatThreshold[] Thresholds;

        public List<StatThreshold> AllThresholds
        {
            get
            {
                List<StatThreshold> results = new List<StatThreshold>();
                if (CriticalThreshold != null && CriticalThreshold.BelowThresholdEmotion != null)
                {
                    results.Add(CriticalThreshold);
                }

                foreach (var threshold in Thresholds)
                {
                    results.Add(threshold);
                }

                return results;
            }
        }

        public float CalculateIntensity(float statValue)
        {
            float intensity = IntensityCurve.Evaluate(statValue);
            return intensity;
        }

        public float DecayRate
        {
            get
            {
                // Per in game minute decay rate
                var decayRate = (DailyDecayRate / 24f) / 60f;
                return decayRate;
            }
        }

        public Emotion CheckThresholds(float statValue)
        {
            var allThresholdsSorted = AllThresholds.OrderBy(threshold => threshold.ThresholdValue);
            foreach (var threshold in allThresholdsSorted)
            {
                if (statValue <= threshold.ThresholdValue)
                {
                    return threshold.BelowThresholdEmotion;
                }
            }

            return null;
        }
    }

    [Serializable]
    public class StatThreshold
    {
        [field: SerializeField, Range(0f, 1f)] public float ThresholdValue;
        [field: SerializeField] public Emotion BelowThresholdEmotion;
    }
}
