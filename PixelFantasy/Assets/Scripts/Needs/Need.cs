using System;
using ScriptableObjects;
using UnityEngine;

namespace Needs
{
    [Serializable]
    public class Need
    {
        public NeedData NeedData;
        [Range(0f, 100f)] public float Value;

        public float needMult; 
        
        public float NeedMultiplier => NeedData.IntensityMultipier(Value);

        public float AddValue(float amount)
        {
            Value = Mathf.Clamp(Value + amount, 0f, 100f);
            return Value;
        }

        public float SubtractValue(float amount)
        {
            Value = Mathf.Clamp(Value - amount, 0f, 100f);
            return Value;
        }
        
        public void DecayMinute()
        {
            var minDecayRate = NeedData.DecayRateParMin;
            SubtractValue(minDecayRate);

            needMult = NeedMultiplier;
        }
    }
}
