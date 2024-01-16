using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Systems.Mood.Scripts;
using UnityEngine;

namespace Systems.Needs.Scripts
{
    [CreateAssetMenu(menuName = "AI/Need", fileName = "NeedData")]
    public class NeedData : ScriptableObject
    {
        [field: SerializeField] public string DisplayName { get; protected set; }
        [field: SerializeField] public string Description { get; protected set; }
        [field: SerializeField] public Sprite NeedIcon { get; protected set; }
        [field: SerializeField] public bool IsVisible { get; protected set; } = true;
        [field: SerializeField, Range(0f, 1f)] public float InitialValue { get; protected set; } = 0.5f;
        [field: SerializeField] public float DailyDecayRate { get; protected set; } = 0.25f;
        [field: SerializeField] public AnimationCurve IntensityCurve { get; protected set; }

        [Header("Thresholds")]
        [field: SerializeField] public NeedThreshold CriticalThreshold;
        [field: SerializeField] public NeedThreshold[] Thresholds;

        public List<NeedThreshold> AllThresholds
        {
            get
            {
                List<NeedThreshold> results = new List<NeedThreshold>();
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

        public float CalculateIntensity(float needValue)
        {
            float intensity = IntensityCurve.Evaluate(needValue);
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

        public Emotion CheckThresholds(float needValue)
        {
            var allThresholdsSorted = AllThresholds.OrderBy(threshold => threshold.ThresholdValue);
            foreach (var threshold in allThresholdsSorted)
            {
                if (needValue <= threshold.ThresholdValue)
                {
                    return threshold.BelowThresholdEmotion;
                }
            }

            return null;
        }
    }

    [Serializable]
    public class NeedThreshold
    {
        [field: SerializeField, Range(0f, 1f)] public float ThresholdValue;
        [field: SerializeField] public Emotion BelowThresholdEmotion;
    }
}
