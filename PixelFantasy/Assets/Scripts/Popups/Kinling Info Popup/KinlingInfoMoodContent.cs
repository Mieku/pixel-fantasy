using System;
using System.Collections.Generic;
using Characters;
using Systems.Mood.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Popups.Kinling_Info_Popup
{
    public class KinlingInfoMoodContent : MonoBehaviour
    {
        [SerializeField] private BarThresholdDisplay _thresholdDisplay;
        [SerializeField] private BarTargetIndicator _targetIndicator;
        [SerializeField] private Transform _positiveEmotionParent;
        [SerializeField] private Transform _negativeEmotionParent;
        [SerializeField] private Image _overallMoodBarFill;
        [SerializeField] private EmotionDisplay _positiveEmotionDisplayPrefab;
        [SerializeField] private EmotionDisplay _negativeEmotionDisplayPrefab;
        
        private Kinling _kinling;
        private Mood _kinlingMood => _kinling.KinlingMood;
        private List<EmotionDisplay> _displayedEmotions = new List<EmotionDisplay>();

        public void Show(Kinling kinling)
        {
            _kinling = kinling;
            _positiveEmotionDisplayPrefab.gameObject.SetActive(false);
            _negativeEmotionDisplayPrefab.gameObject.SetActive(false);
            
            gameObject.SetActive(true);
            
            _thresholdDisplay.ShowThresholds(_kinlingMood.AllThresholds);

            GameEvents.MinuteTick += GameEvent_MinuteTick;
        }
        
        public void Close()
        {
            gameObject.SetActive(false);
            
            ClearAllDisplayedEmotions();
            
            GameEvents.MinuteTick -= GameEvent_MinuteTick;
        }

        private void ClearAllDisplayedEmotions()
        {
            foreach (var emotionDisplay in _displayedEmotions)
            {
                Destroy(emotionDisplay.gameObject);
            }
            _displayedEmotions.Clear();
        }
        
        private void GameEvent_MinuteTick()
        {
            RefreshOverallMoodDisplay();

            var emotionStates = new HashSet<EmotionState>(_kinlingMood.AllEmotions);
            var displayedEmotionStates = new HashSet<EmotionState>(DisplayedEmotionState());

            // Object pools for positive and negative emotions
            var positivePool = new List<EmotionDisplay>();
            var negativePool = new List<EmotionDisplay>();

            // Add any new emotions
            foreach (var emotionState in emotionStates)
            {
                if (!displayedEmotionStates.Contains(emotionState))
                {
                    EmotionDisplay newEmotionDisplay;
                    if (emotionState.LinkedEmotion.MoodModifier >= 0)
                    {
                        newEmotionDisplay = GetFromPool(positivePool, _positiveEmotionDisplayPrefab, _positiveEmotionParent);
                    }
                    else
                    {
                        newEmotionDisplay = GetFromPool(negativePool, _negativeEmotionDisplayPrefab, _negativeEmotionParent);
                    }
                    newEmotionDisplay.Init(emotionState);
                    newEmotionDisplay.gameObject.SetActive(true);
                    _displayedEmotions.Add(newEmotionDisplay);
                }
            }

            // Remove any expired emotions and add to pool
            foreach (var displayedEmotionState in displayedEmotionStates)
            {
                if (!emotionStates.Contains(displayedEmotionState))
                {
                    var expiredDisplay = _displayedEmotions.Find(display => display.EmotionState == displayedEmotionState);
                    _displayedEmotions.Remove(expiredDisplay);
                    expiredDisplay.gameObject.SetActive(false);

                    if (expiredDisplay.EmotionState.LinkedEmotion.MoodModifier >= 0)
                    {
                        positivePool.Add(expiredDisplay);
                    }
                    else
                    {
                        negativePool.Add(expiredDisplay);
                    }
                }
            }

            // Refresh the emotions (consider a more selective refresh strategy)
            foreach (var displayedEmotion in _displayedEmotions)
            {
                displayedEmotion.Refresh();
            }
        }

        private EmotionDisplay GetFromPool(List<EmotionDisplay> pool, EmotionDisplay prefab, Transform parent)
        {
            if (pool.Count > 0)
            {
                var obj = pool[0];
                pool.RemoveAt(0);
                return obj;
            }
            return Instantiate(prefab, parent);
        }

        // private void GameEvent_MinuteTick()
        // {
        //     RefreshOverallMoodDisplay();
        //     
        //     var emotionStates = _kinlingMood.AllEmotions;
        //     var displayedEmotionStates = DisplayedEmotionState();
        //
        //     // Add any new emotions
        //     List<EmotionState> newEmotionStates = new List<EmotionState>();
        //     foreach (var emotionState in emotionStates)
        //     {
        //         if (!displayedEmotionStates.Contains(emotionState))
        //         {
        //             newEmotionStates.Add(emotionState);
        //         }
        //     }
        //     
        //     // Remove any emotions that are expired
        //     List<EmotionState> expiredEmotionStates = new List<EmotionState>();
        //     foreach (var displayedEmotionState in displayedEmotionStates)
        //     {
        //         if (!emotionStates.Contains(displayedEmotionState))
        //         {
        //             expiredEmotionStates.Add(displayedEmotionState);
        //         }
        //     }
        //
        //     // Create the new emotions
        //     foreach (var newEmotionState in newEmotionStates)
        //     {
        //         // Positive Emotion
        //         if (newEmotionState.LinkedEmotion.MoodModifier >= 0)
        //         {
        //             var newEmotionDisplay = Instantiate(_positiveEmotionDisplayPrefab, _positiveEmotionParent);
        //             newEmotionDisplay.Init(newEmotionState);
        //             newEmotionDisplay.gameObject.SetActive(true);
        //             _displayedEmotions.Add(newEmotionDisplay);
        //         }
        //         else // Negative Emotion
        //         {
        //             var newEmotionDisplay = Instantiate(_negativeEmotionDisplayPrefab, _negativeEmotionParent);
        //             newEmotionDisplay.Init(newEmotionState);
        //             newEmotionDisplay.gameObject.SetActive(true);
        //             _displayedEmotions.Add(newEmotionDisplay);
        //         }
        //     }
        //     
        //     // Destroy the expired emotions
        //     foreach (var expiredEmotionState in expiredEmotionStates)
        //     {
        //         var expiredDisplay = _displayedEmotions.Find(display => display.EmotionState == expiredEmotionState);
        //         _displayedEmotions.Remove(expiredDisplay);
        //         Destroy(expiredDisplay.gameObject);
        //     }
        //     
        //     // Refresh the emotions
        //     foreach (var displayedEmotion in _displayedEmotions)
        //     {
        //         displayedEmotion.Refresh();
        //     }
        // }

        private void RefreshOverallMoodDisplay()
        {
            var overallMoodPercent = _kinlingMood.OverallMood / 100f;
            var targetMoodPercent = _kinlingMood.MoodTarget / 100f;

            _overallMoodBarFill.fillAmount = overallMoodPercent;
            _targetIndicator.SetTargetIndicator(targetMoodPercent);
        }

        private List<EmotionState> DisplayedEmotionState()
        {
            List<EmotionState> results = new List<EmotionState>();
            foreach (var emotionDisplay in _displayedEmotions)
            {
                results.Add(emotionDisplay.EmotionState);
            }

            return results;
        }
        
        
    }
}
