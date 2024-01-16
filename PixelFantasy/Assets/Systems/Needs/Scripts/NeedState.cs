using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.Needs.Scripts
{
    [Serializable]
    public class NeedState
    {
        [SerializeField] private NeedData _needData;
        [SerializeField] private float _value;

        public float Intensity => _needData.CalculateIntensity(_value);
        public float Value => _value;

        /// <summary>
        /// Increases the Need by percentage
        /// </summary>
        /// <param name="amount">Percentage to increase need by (ex 5% = 0.05)</param>
        /// <returns>Returns the current need value after the change</returns>
        public float IncreaseNeed(float amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"The amount ({amount}) to increase the need should be a positive number, using abs instead");
                amount = Mathf.Abs(amount);
            }
            
            _value = Mathf.Clamp01(_value + amount);
            return _value;
        }

        /// <summary>
        /// Decreases the Need by percentage
        /// </summary>
        /// <param name="amount">Percentage to decrease need by (ex 5% = 0.05)</param>
        /// <returns>Returns the current need value after the change</returns>
        public float DecreaseNeed(float amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"The amount ({amount}) to decrease the need should be a positive number, using abs instead");
                amount = Mathf.Abs(amount);
            }
            
            _value = Mathf.Clamp01(_value - amount);
            return _value;
        }

        public void MinuteTickDecayNeed()
        {
            var decayPerMin = _needData.DecayRate;
            DecreaseNeed(decayPerMin);
        }

        public void Initialize()
        {
            _value = _needData.InitialValue;
        }
    }
}
