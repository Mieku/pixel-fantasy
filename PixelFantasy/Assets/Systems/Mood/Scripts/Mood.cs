using System;
using System.Collections.Generic;
using Characters;
using Managers;
using Systems.Notifications.Scripts;
using Systems.Traits.Scripts;
using TaskSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.Mood.Scripts
{
    [RequireComponent(typeof(Unit))]
    public class Mood : MonoBehaviour
    {
        [SerializeField] private List<EmotionalBreakdown> _availableBreakdowns = new List<EmotionalBreakdown>();
        
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
        private const int POSITIVE_HOURLY_TICK_RATE = 12; // From RimWorld, should re-balance for Kinlings
        private const int NEGATIVE_HOURLY_TICK_RATE = -8; // From RimWorld, should re-balance for Kinlings
        private const int BASE_BREAK_THRESHOLD = 35; // From RimWorld, should re-balance for Kinlings

        private int _moodTarget;
        private float _overallMood;
        private Unit _unit;
        private TaskAI _taskAI => _unit.TaskAI;
        private MoodThresholdTrait _moodThresholdTrait;
        private int _minorBreakThreshold;
        private int _majorBreakThreshold;
        private int _extremeBreakThreshold;
        private EMoodBreakType _moodState;
        private PendingBreakdownState _pendingBreakdownState;
        private TaskAction _curBreakdownAction;

        public float MinorBreakThresholdPercent => _minorBreakThreshold / 100f;
        public float MajorBreakThresholdPercent => _majorBreakThreshold / 100f;
        public float ExtremeBreakThresholdPercent => _extremeBreakThreshold / 100f;

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

        private void Awake()
        {
            _unit = GetComponent<Unit>();
            _moodThresholdTrait = _unit.GetMoodThresholdTrait();
            
            GameEvents.MinuteTick += GameEvents_MinuteTick;
        }

        private void OnDestroy()
        {
            GameEvents.MinuteTick -= GameEvents_MinuteTick;
        }

        public void Init()
        {
            _moodTarget = _baseMood;
            _overallMood = _baseMood;
        }

        private void Start()
        {
            AssignThresholds();
        }

        private void AssignThresholds()
        {
            if (_moodThresholdTrait != null)
            {
                _minorBreakThreshold = BASE_BREAK_THRESHOLD + _moodThresholdTrait.EmotionalBreakdownThresholdChange;
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

        public bool HasEmotion(Emotion emotion)
        {
            return FindEmotionState(emotion) != null;
        }

        public void RemoveEmotion(Emotion emotionToRemove)
        {
            // Make sure it has the emotion
            if (HasEmotion(emotionToRemove))
            {
                foreach (var emotionState in _allEmotionalStates)
                {
                    if (emotionState.LinkedEmotion == emotionToRemove)
                    {
                        _allEmotionalStates.Remove(emotionState);
                        return;
                    }
                }
            }
        }

        public void ApplyEmotion(Emotion emotionToAdd)
        {
            // When applying, make sure there is no current state with the emotion. If there is, reset its timer
            var curEmotionState = FindEmotionState(emotionToAdd);
            if (curEmotionState != null)
            {
                curEmotionState.ResetDuration();
            }
            else
            {
                EmotionState newEmotion = new EmotionState(emotionToAdd);
                _allEmotionalStates.Add(newEmotion);
            }
            
            CalculateTargetMood();
        }

        private EmotionState FindEmotionState(Emotion emotion)
        {
            foreach (var emotionalState in _allEmotionalStates)
            {
                if (emotionalState.LinkedEmotion == emotion)
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
                    if (emotionalState.LinkedEmotion.MoodModifier >= 0)
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
                    if (emotionalState.LinkedEmotion.MoodModifier < 0)
                    {
                        results.Add(emotionalState);
                    }
                }

                return results;
            }
        }

        private void GameEvents_MinuteTick()
        {
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
            }
            
            // Tick the overallMood towards the target
            if (_moodTarget > _overallMood)
            {
                float tickAmount = Mathf.Min(_moodTarget - _overallMood, POSITIVE_HOURLY_TICK_RATE / 60f);
                _overallMood += tickAmount;
            } 
            else if (_moodTarget < _overallMood)
            {
                float tickAmount = Mathf.Max(_moodTarget - _overallMood, NEGATIVE_HOURLY_TICK_RATE / 60f);
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
            List<EmotionalBreakdown> allBreakdownOptions)
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
            NotificationManager.Instance.CreateKinlingLog(_unit, $"{_unit.GetUnitState().FullName} is having a Breakdown!", LogData.ELogType.Danger);

            // Start a breakdown action
            _curBreakdownAction = _taskAI.ForceTask(_pendingBreakdownState.Breakdown.BreakdownTaskId);
            if (_curBreakdownAction == null)
            {
                // Check if the breakdown is still possible, if not swap with something else possible
                Debug.LogWarning($"Breakdown Action: {_curBreakdownAction.TaskId} could not start, creating a new breakdown state as a replacement");
                _pendingBreakdownState = CreateRandomBreakdownState(_moodState, _availableBreakdowns);
                _pendingBreakdownState.RemainingMinsToStart = 0;
            }
        }

        public void DEBUG_TriggerBreakdown(EmotionalBreakdown breakdown)
        {
            PendingBreakdownState breakdownState = new PendingBreakdownState(breakdown, OnBreakdownBegin, OnBreakdownComplete);
            breakdownState.RemainingMinsToStart = 0;
            _pendingBreakdownState = breakdownState;
        }

        public void DEBUG_EndBreakdown()
        {
            _pendingBreakdownState.EndBreakdown();
        }

        private void OnBreakdownComplete()
        {
            NotificationManager.Instance.CreateKinlingLog(_unit, $"{_unit.GetUnitState().FullName}'s Breakdown is over!", LogData.ELogType.Notification);
            
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
            public EmotionalBreakdown Breakdown;
            public int RemainingMinsToStart;
            public int RemainingMinsForRecovery;

            private Action _onBreakdownBegin;
            private Action _onBreakdownComplete;
            private bool _isBreakingdown;

            public PendingBreakdownState(EmotionalBreakdown breakdown, Action onBreakdownBegin, Action onBreakdownComplete)
            {
                Breakdown = breakdown;
                RemainingMinsToStart = breakdown.MaxMinutesUntilBreakdown;
                RemainingMinsForRecovery = breakdown.RecoveryTimeMins;
                
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
