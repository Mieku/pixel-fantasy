using System;
using System.Collections.Generic;
using Characters;
using Newtonsoft.Json;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.Needs.Scripts
{
    public class Need
    {
        [JsonRequired] private string _needSettingsID;
        [JsonRequired] private float _value;
        [JsonRequired] private float _targetValue;
        [JsonRequired] private bool _hasTargetValue;

        [JsonIgnore] public float Intensity => _needSettings.CalculateIntensity(_value);
        [JsonIgnore] public float Value => _value;
        [JsonIgnore] public float TargetValue => _targetValue;
        [JsonIgnore] public bool HasTargetValue => _hasTargetValue;

        [JsonIgnore] private Kinling _kinling;

        [JsonIgnore] private NeedSettings _needSettings => GameSettings.Instance.LoadNeedSettings(_needSettingsID);

        public Need(NeedSettings settings)
        {
            _needSettingsID = settings.name;
        }
        
        public void Init(Kinling kinling)
        {
            _value = _needSettings.InitialValue;
            _targetValue = 0;
            _hasTargetValue = false;
            _kinling = kinling;
        }
        
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

        public float SetNeed(float amount)
        {
            _value = Mathf.Clamp01(amount);
            return _value;
        }

        public void SetTargetValue(float target)
        {
            _targetValue = target;
            _hasTargetValue = true;
        }

        public void RemoveTargetValue()
        {
            _hasTargetValue = false;
            _targetValue = 0;
        }

        public void MinuteTickDecayNeed()
        {
            var decayPerMin = _needSettings.DecayRate;
            DecreaseNeed(decayPerMin);
        }

        public void MinuteTick()
        {
            MinuteTickDecayNeed();

            if (_hasTargetValue)
            {
                // Tick the overallMood towards the target
                if (_targetValue > _value)
                {
                    float tickAmount = Mathf.Min(_targetValue - _value,  _needSettings.PositiveHourlyTickRate / 60f);
                    _value += tickAmount;
                } 
                else if (_targetValue < _value)
                {
                    float tickAmount = Mathf.Max(_targetValue - _value, _needSettings.NegativeHourlyTickRate / 60f);
                    _value += tickAmount;
                }
            }
            
            CheckThresholds();
        }

        public List<NeedThreshold> GetThresholds()
        {
            return _needSettings.AllThresholds;
        }

        private void CheckThresholds()
        {
            var emotion = _needSettings.CheckThresholds(_value);
            if (!_kinling.MoodData.HasEmotion(emotion))
            {
                // Remove the others
                foreach (var threshold in GetThresholds())
                {
                    var otherEmotion = threshold.BelowThresholdEmotionSettings;
                    if (otherEmotion != null)
                    {
                        _kinling.MoodData.RemoveEmotion(otherEmotion);
                    }
                }
                    
                if (emotion != null)
                {
                    _kinling.MoodData.ApplyEmotion(emotion);
                }
            }
        }
    }
}
