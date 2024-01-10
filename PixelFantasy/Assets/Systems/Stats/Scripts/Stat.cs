using System;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    [Serializable]
    public class Stat
    {
        [SerializeField] private AIStat _statData;
        [SerializeField] private float _value;

        public float Intensity => _statData.CalculateIntensity(_value);
        public float Value => _value;

        /// <summary>
        /// Increases the Stat by percentage
        /// </summary>
        /// <param name="amount">Percentage to increase stat by (ex 5% = 0.05)</param>
        /// <returns>Returns the current stat value after the change</returns>
        public float IncreaseStat(float amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"The amount ({amount}) to increase the stat should be a positive number, using abs instead");
                amount = Mathf.Abs(amount);
            }
            
            _value = Mathf.Clamp01(_value + amount);
            return _value;
        }

        /// <summary>
        /// Decreases the Stat by percentage
        /// </summary>
        /// <param name="amount">Percentage to decrease stat by (ex 5% = 0.05)</param>
        /// <returns>Returns the current stat value after the change</returns>
        public float DecreaseStat(float amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"The amount ({amount}) to decrease the stat should be a positive number, using abs instead");
                amount = Mathf.Abs(amount);
            }
            
            _value = Mathf.Clamp01(_value - amount);
            return _value;
        }

        public void MinuteTickDecayStat()
        {
            var decayPerMin = _statData.DecayRate;
            DecreaseStat(decayPerMin);
        }

        public void Initialize()
        {
            _value = _statData.InitialValue;
        }
    }
}
