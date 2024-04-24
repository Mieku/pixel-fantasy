using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Systems.Mood.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.Needs.Scripts
{
    [CreateAssetMenu(menuName = "Settings/AI/Need Settings", fileName = "NeedSettings")]
    public class NeedSettings : ScriptableObject
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
        
        [field: SerializeField] public float PositiveHourlyTickRate { get; private set; } = .4f;
        [field: SerializeField] public float NegativeHourlyTickRate { get; private set; } = -0.3f;

        public List<NeedThreshold> AllThresholds
        {
            get
            {
                List<NeedThreshold> results = new List<NeedThreshold>();
                if (CriticalThreshold != null && CriticalThreshold.BelowThresholdEmotionSettings != null)
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

        public EmotionSettings CheckThresholds(float needValue)
        {
            var allThresholdsSorted = AllThresholds.OrderBy(threshold => threshold.ThresholdValue);
            foreach (var threshold in allThresholdsSorted)
            {
                if (needValue <= threshold.ThresholdValue)
                {
                    return threshold.BelowThresholdEmotionSettings;
                }
            }

            return null;
        }
    }

    [Serializable]
    public class NeedThreshold
    {
        [field: SerializeField, Range(0f, 1f)] public float ThresholdValue;
        [field: SerializeField] public EmotionSettings BelowThresholdEmotionSettings;
        [field: SerializeField] public bool HideThreshold { get; private set; }
    }
}
