using System;
using ScriptableObjects;
using Systems.Stats.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.Traits.Scripts
{
    
    [System.Serializable]
    public class TraitElement
    {
        //public NeedType NeedType;
        public AIStat LinkedStat;
        
        [Header("Scoring Scales")]
        [Range(0.5f, 1.5f)] public float Scoring_PositiveScale = 1f;
        [Range(0.5f, 1.5f)] public float Scoring_NegativeScale = 1f;
        
        [Header("Impact Scales")]
        [Range(0.5f, 1.5f)] public float Impact_PositiveScale = 1f;
        [Range(0.5f, 1.5f)] public float Impact_NegativeScale = 1f;

        [Header("Decay Rate")] 
        [Range(0.5f, 1.5f)] public float DecayRateScale = 1f;

        public float Apply(AIStat targetStat, Trait.ETargetType targetType, float currentValue)
        {
            if (targetStat == LinkedStat)
            {
                switch (targetType)
                {
                    case Trait.ETargetType.Score:
                        if (currentValue > 0)
                            currentValue *= Scoring_PositiveScale;
                        else if (currentValue < 0)
                            currentValue *= Scoring_NegativeScale;
                        break;
                    case Trait.ETargetType.Impact:
                        if (currentValue > 0)
                            currentValue *= Impact_PositiveScale;
                        else if (currentValue < 0)
                            currentValue *= Impact_NegativeScale;
                        break;
                    case Trait.ETargetType.DecayRate:
                        currentValue *= DecayRateScale;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null);
                }
            }

            return currentValue;
        }
    }
    
    [CreateAssetMenu(menuName = "AI/Trait", fileName = "Trait")]
    public class Trait : ScriptableObject
    {
        public enum ETargetType
        {
            Score,
            Impact,
            DecayRate,
        }

        public string DisplayName;
        public string Description;
        public Sprite Icon;
        public TraitElement[] Impacts;
        
        public float Apply(AIStat targetStat, ETargetType targetType, float currentValue)
        {
            foreach (var impact in Impacts)
            {
                currentValue = impact.Apply(targetStat, targetType, currentValue);
            }

            return currentValue;
        }
    }
}
