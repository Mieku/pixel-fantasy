using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "NeedData", menuName = "Needs/NeedData", order = 1)]
    public class NeedData : ScriptableObject
    {
        public NeedType NeedType;
        public AnimationCurve IntensityCurve;
        public float IntensityMuliplierMaximum;
        public float DecayRatePerHour;

        public float DecayRateParMin => DecayRatePerHour / 60f;

        public float IntensityMultipier(float needValue)
        {
            var percentValue = needValue / 100f;
            return IntensityCurve.Evaluate(percentValue) * IntensityMuliplierMaximum;
        }
    }

    public enum NeedType
    {
        // The Primary Needs
        Food,
        Energy,
        Fun,
        Social,
        Comfort,
        Beauty,
    }
}
