using System;
using System.Collections.Generic;
using Characters;
using Systems.Traits.Scripts;
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
        private MoodThresholdTrait _moodThresholdTrait;
        private int _minorBreakThreshold;
        private int _majorBreakThreshold;
        private int _extremeBreakThreshold;
        private EMoodBreakType _moodState;
        private PendingBreakdownState _pendingBreakdownState;
        
        public float OverallMood => _overallMood;

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
                float tickAmount = Mathf.Min(_moodTarget - _overallMood, NEGATIVE_HOURLY_TICK_RATE / 60f);
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
                allBreakdownOptions.FindAll(breakdown => breakdown.BreakdownType == breakType);

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
            Debug.Log("Breakdown has begun!");
            // TODO: Display a msg for the player that a breakdown is happening
            
            // TODO: Start a breakdown action
        }

        private void OnBreakdownComplete()
        {
            Debug.Log("Breakdown is over!");
            // TODO: End the breakdown Action
            
            // TODO: If there is currently a msg for the player about a breakdown, remove it
            
            // TODO: Give the Kinling an emotion to boost it out of danger, RimWorld has Catharsis for 2.5 days giving +40
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
        }
    }
}
