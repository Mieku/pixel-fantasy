using System;
using Managers;
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
        //[field: SerializeField, Range(0f, 1f)] public float DecayRate { get; protected set; } = 0.005f;
        [field: SerializeField] public AnimationCurve IntensityCurve { get; protected set; }

        [Header("Thresholds")]
        [field: SerializeField] public StatThreshold CriticalThreshold;
        [field: SerializeField] public StatThreshold[] Thresholds;

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
    }

    [Serializable]
    public class StatThreshold
    {
        [field: SerializeField, Range(0f, 1f)] public float ThresholdValue;
        [field: SerializeField] public StatThresholdEffect BelowThresholdEffect;
        [field: SerializeField] public StatThresholdEffect AboveThresholdEffect;
    }

    [Serializable]
    public class StatThresholdEffect
    {
        [field: SerializeField] public string MoodEffectName;
        [field: SerializeField] public string MoodEffectDescription;
        [field: SerializeField] public int MoodModifier;
    }
}
