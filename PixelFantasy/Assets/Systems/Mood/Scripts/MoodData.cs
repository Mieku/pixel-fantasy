using System;
using System.Collections.Generic;
using Characters;
using Managers;
using ScriptableObjects;
using Systems.Notifications.Scripts;
using Systems.Traits.Scripts;
using TaskSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.Mood.Scripts
{
    [Serializable]
    public class MoodData
    {
        [SerializeField] private List<EmotionalBreakdownSettings> _availableBreakdowns = new List<EmotionalBreakdownSettings>();
        
        [SerializeField] 
        private List<EmotionState> _allEmotionalStates = new List<EmotionState>();
        
        /*
            Note: RimWorld changes the baseMood depending on difficulty, I should too!
            RimWorld's Values:
            Peaceful	42
            Community builder	42
            Adventure story	37
            Strive to survive	32
            Blood and dust	27
         */
        private int _baseMood = 37;
        private const int BASE_BREAK_THRESHOLD = 35; // From RimWorld, should re-balance for Kinlings

        [SerializeField]
        private int _moodTarget;
        
        [SerializeField]
        private float _overallMood;
        
        private KinlingData _kinlingData;
        private TaskAI _taskAI => _kinlingData.Kinling.TaskAI;
        private MoodThresholdSettings _moodThresholdSettings;
        
        [SerializeField]
        private int _minorBreakThreshold;
        
        [SerializeField]
        private int _majorBreakThreshold;
        
        [SerializeField]
        private int _extremeBreakThreshold;
        
        [SerializeField]
        private EMoodBreakType _moodState;
        
        private PendingBreakdownState _pendingBreakdownState;
        private TaskAction _curBreakdownAction;

        public float MinorBreakThresholdPercent => _minorBreakThreshold / 100f;
        public float MajorBreakThresholdPercent => _majorBreakThreshold / 100f;
        public float ExtremeBreakThresholdPercent => _extremeBreakThreshold / 100f;

        private float _positiveMinuteTick => GameSettings.Instance.MoodPositiveHourlyRate / 60f;
        private float _negativeMinuteTick => GameSettings.Instance.MoodNegativeHourlyRate / 60f;

        public List<float> AllThresholds
        {
            get
            {
                List<float> results = new List<float>();
                results.Add(MinorBreakThresholdPercent);
                results.Add(MajorBreakThresholdPercent);
                results.Add(ExtremeBreakThresholdPercent);
                return results;
            }
        }

        public float OverallMood => _overallMood;
        public int MoodTarget => _moodTarget;
        
        public void Init(KinlingData kinlingData)
        {
            _kinlingData = kinlingData;
            _moodThresholdSettings = _kinlingData.GetMoodThresholdTrait();
  
            _moodTarget = _baseMood;
            _overallMood = _baseMood;
            
            AssignThresholds();
            
            CalculateTargetMood();
        }

        public void JumpMoodToTarget()
        {
            CalculateTargetMood();
            _overallMood = _moodTarget;
        }
        
        private void AssignThresholds()
        {
            if (_moodThresholdSettings != null)
            {
                _minorBreakThreshold = BASE_BREAK_THRESHOLD + _moodThresholdSettings.EmotionalBreakdownThresholdChange;
            }
            else
            {
                _minorBreakThreshold = BASE_BREAK_THRESHOLD;
            }

            _majorBreakThreshold = Mathf.Clamp(Mathf.FloorToInt(_minorBreakThreshold * (4f / 7f)), 1, 50);
            _extremeBreakThreshold = Mathf.Clamp(Mathf.FloorToInt(_minorBreakThreshold * (1f / 7f)), 1, 50);
        }

        private void CalculateTargetMood()
        {
            int moodModifier = _baseMood;
            foreach (var emotionalState in _allEmotionalStates)
            {
                moodModifier += emotionalState.MoodModifier;
            }

            moodModifier = Mathf.Clamp(moodModifier, 0, 100);
            _moodTarget = moodModifier;
        }

        public bool HasEmotion(EmotionSettings emotionSettings)
        {
            return FindEmotionState(emotionSettings) != null;
        }

        public void RemoveEmotion(EmotionSettings emotionSettingsToRemove)
        {
            // Make sure it has the emotion
            if (HasEmotion(emotionSettingsToRemove))
            {
                foreach (var emotionState in _allEmotionalStates)
                {
                    if (emotionState.LinkedEmotionSettings == emotionSettingsToRemove)
                    {
                        _allEmotionalStates.Remove(emotionState);
                        return;
                    }
                }
            }
        }

        public void ApplyEmotion(EmotionSettings emotionSettingsToAdd)
        {
            // When applying, make sure there is no current state with the emotion. If there is, reset its timer
            var curEmotionState = FindEmotionState(emotionSettingsToAdd);
            if (curEmotionState != null)
            {
                curEmotionState.ResetDuration();
            }
            else
            {
                EmotionState newEmotion = new EmotionState(emotionSettingsToAdd);
                _allEmotionalStates.Add(newEmotion);
            }
            
            CalculateTargetMood();
            
            GameEvents.Trigger_OnKinlingChanged(_kinlingData);
        }

        private EmotionState FindEmotionState(EmotionSettings emotionSettings)
        {
            foreach (var emotionalState in _allEmotionalStates)
            {
                if (emotionalState.LinkedEmotionSettings == emotionSettings)
                {
                    return emotionalState;
                }
            }

            return null;
        }

        public List<EmotionState> AllEmotions => _allEmotionalStates;

        public List<EmotionState> PositiveEmotions
        {
            get
            {
                List<EmotionState> results = new List<EmotionState>();
                foreach (var emotionalState in _allEmotionalStates)
                {
                    if (emotionalState.LinkedEmotionSettings.MoodModifier >= 0)
                    {
                        results.Add(emotionalState);
                    }
                }

                return results;
            }
        }
        
        public List<EmotionState> NegativeEmotions
        {
            get
            {
                List<EmotionState> results = new List<EmotionState>();
                foreach (var emotionalState in _allEmotionalStates)
                {
                    if (emotionalState.LinkedEmotionSettings.MoodModifier < 0)
                    {
                        results.Add(emotionalState);
                    }
                }

                return results;
            }
        }

        public void MinuteTick()
        {
            if(_kinlingData == null) return;
            
            List<EmotionState> expiredEmotions = new List<EmotionState>();
            foreach (var emotionalState in _allEmotionalStates)
            {
                var durationComplete = emotionalState.TickEmotionState();
                if (durationComplete)
                {
                    expiredEmotions.Add(emotionalState);
                }
            }

            if (expiredEmotions.Count > 0)
            {
                foreach (var expiredEmotion in expiredEmotions)
                {
                    _allEmotionalStates.Remove(expiredEmotion);
                }
                expiredEmotions.Clear();
                
                CalculateTargetMood();
                GameEvents.Trigger_OnKinlingChanged(_kinlingData);
            }
            
            // Tick the overallMood towards the target
            if (_moodTarget > _overallMood)
            {
                float tickAmount = Mathf.Min(_moodTarget - _overallMood, _positiveMinuteTick);
                _overallMood += tickAmount;
            } 
            else if (_moodTarget < _overallMood)
            {
                float tickAmount = Mathf.Max(_moodTarget - _overallMood, _negativeMinuteTick);
                _overallMood += tickAmount;
            }
            
            CheckMoodThresholds();
            TickPendingBreakdown();
        }

        private void CheckMoodThresholds()
        {
            if (_overallMood <= _extremeBreakThreshold)
            {
                // They are in an extreme break risk
                SetMoodState(EMoodBreakType.ExtremeBreak);
            } 
            else if (_overallMood <= _majorBreakThreshold)
            {
                // They are in a major break risk
                SetMoodState(EMoodBreakType.MajorBreak);
            }
            else if (_overallMood <= _minorBreakThreshold)
            {
                // They are in a minor break risk
                SetMoodState(EMoodBreakType.MinorBreak);
            }
            else
            {
                // All good!
                SetMoodState(EMoodBreakType.None);
            }
        }

        private void SetMoodState(EMoodBreakType newBreakType)
        {
            if(_moodState == newBreakType) return; // No Change

            _moodState = newBreakType;

            if (_moodState == EMoodBreakType.None)
            {
                _pendingBreakdownState = null;
                // TODO: If there is currently a msg for the player about a breakdown risk, remove it
            }
            else
            {
                _pendingBreakdownState = CreateRandomBreakdownState(_moodState, _availableBreakdowns);
                // TODO: Display a msg for the player that a breakdown risk is happening
            }
        }

        private PendingBreakdownState CreateRandomBreakdownState(EMoodBreakType breakType,
            List<EmotionalBreakdownSettings> allBreakdownOptions)
        {
            var filteredBreakdownOptions =
                allBreakdownOptions.FindAll(breakdown => breakdown.BreakdownType == breakType && _taskAI.IsActionPossible(breakdown.BreakdownTaskId));
            
            int random = Random.Range(0, filteredBreakdownOptions.Count - 1);
            var breakdown = filteredBreakdownOptions[random];
            PendingBreakdownState breakdownState = new PendingBreakdownState(breakdown, OnBreakdownBegin, OnBreakdownComplete);
            return breakdownState;
        }

        private void TickPendingBreakdown()
        {
            if (_pendingBreakdownState == null) return;
            
            _pendingBreakdownState.MinuteTick();
        }

        private void OnBreakdownBegin()
        {
            NotificationManager.Instance.CreateKinlingLog(_kinlingData.Kinling, $"{_kinlingData.Fullname} is having a Breakdown!", LogData.ELogType.Danger);

            // Start a breakdown action
            _curBreakdownAction = _taskAI.ForceTask(_pendingBreakdownState.BreakdownSettings.BreakdownTaskId);
            if (_curBreakdownAction == null)
            {
                // Check if the breakdown is still possible, if not swap with something else possible
                Debug.LogWarning($"Breakdown Action: {_curBreakdownAction.TaskId} could not start, creating a new breakdown state as a replacement");
                _pendingBreakdownState = CreateRandomBreakdownState(_moodState, _availableBreakdowns);
                _pendingBreakdownState.RemainingMinsToStart = 0;
            }
            
            GameEvents.Trigger_OnKinlingChanged(_kinlingData);
        }

        public void DEBUG_TriggerBreakdown(EmotionalBreakdownSettings breakdownSettings)
        {
            PendingBreakdownState breakdownState = new PendingBreakdownState(breakdownSettings, OnBreakdownBegin, OnBreakdownComplete);
            breakdownState.RemainingMinsToStart = 0;
            _pendingBreakdownState = breakdownState;
        }

        public void DEBUG_EndBreakdown()
        {
            _pendingBreakdownState.EndBreakdown();
        }

        private void OnBreakdownComplete()
        {
            NotificationManager.Instance.CreateKinlingLog(_kinlingData.Kinling, $"{_kinlingData.Fullname}'s Breakdown is over!", LogData.ELogType.Notification);
            
            // End the breakdown Action
            _curBreakdownAction.ConcludeAction();
            _pendingBreakdownState = null;
            
            // Give the Kinling an emotion to boost it out of danger
            var catharsisEmotion = Librarian.Instance.GetEmotion("Catharsis");
            ApplyEmotion(catharsisEmotion);
        }

        public enum EMoodBreakType
        {
            None,
            MinorBreak,
            MajorBreak,
            ExtremeBreak
        }

        public class PendingBreakdownState
        {
            public EmotionalBreakdownSettings BreakdownSettings;
            public int RemainingMinsToStart;
            public int RemainingMinsForRecovery;

            private Action _onBreakdownBegin;
            private Action _onBreakdownComplete;
            private bool _isBreakingdown;

            public PendingBreakdownState(EmotionalBreakdownSettings breakdownSettings, Action onBreakdownBegin, Action onBreakdownComplete)
            {
                BreakdownSettings = breakdownSettings;
                RemainingMinsToStart = breakdownSettings.MaxMinutesUntilBreakdown;
                RemainingMinsForRecovery = breakdownSettings.RecoveryTimeMins;
                
                _onBreakdownBegin = onBreakdownBegin;
                _onBreakdownComplete = onBreakdownComplete;
            }

            public void MinuteTick()
            {
                if (_isBreakingdown)
                {
                    RemainingMinsForRecovery--;
                    if (RemainingMinsForRecovery <= 0)
                    {
                        _isBreakingdown = false;
                        _onBreakdownComplete.Invoke();
                    }
                }
                else
                {
                    RemainingMinsToStart--;
                    if (RemainingMinsToStart <= 0)
                    {
                        _isBreakingdown = true;
                        _onBreakdownBegin.Invoke();
                    }
                }
            }

            public void EndBreakdown()
            {
                _isBreakingdown = false;
                _onBreakdownComplete.Invoke();
            }
        }
    }
}
