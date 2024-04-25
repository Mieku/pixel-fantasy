using System;
using System.Collections.Generic;
using Characters;
using Popups.Kinling_Info_Popup;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Kinling_Details
{
    public class MoodSection : MonoBehaviour
    {
        [SerializeField, BoxGroup("Mood")] private BarThresholdDisplay _thresholdDisplay;
        [SerializeField, BoxGroup("Mood")] private BarTargetIndicator _targetIndicator;
        [SerializeField, BoxGroup("Mood")] private Image _overallMoodBarFill;

        [SerializeField] private EmotionDisplay _emotionDisplayPrefab;
        [SerializeField] private Transform _emotionLayout;
        
        private KinlingData _kinlingData;
        private Action _refreshLayoutCallback;
        private List<EmotionDisplay> _displayedEmotions = new List<EmotionDisplay>();

        public void ShowSection(KinlingData kinlingData, Action refreshLayoutCallback)
        {
            _kinlingData = kinlingData;
            _refreshLayoutCallback = refreshLayoutCallback;
            gameObject.SetActive(true);
            _emotionDisplayPrefab.gameObject.SetActive(false);
            
            _thresholdDisplay.ShowThresholds(_kinlingData.Mood.AllThresholds);
            RefreshOverallMoodDisplay();
            
            RefreshDisplayedEmotions();
            
            GameEvents.MinuteTick += GameEvent_MinuteTick;
        }

        public void Hide()
        {
            GameEvents.MinuteTick -= GameEvent_MinuteTick;
            
            gameObject.SetActive(false);
            _kinlingData = null;
        }

        public void KinlingUpdateRefresh()
        {
            RefreshDisplayedEmotions();
        }
        
        private void GameEvent_MinuteTick()
        {
            RefreshOverallMoodDisplay();
        }

        private void RefreshDisplayedEmotions()
        {
            foreach (var displayedEmotion in _displayedEmotions)
            {
                Destroy(displayedEmotion.gameObject);
            }
            _displayedEmotions.Clear();

            var posEmotions = _kinlingData.Mood.PositiveEmotions;
            var negEmotions = _kinlingData.Mood.NegativeEmotions;

            int index = 0;
            foreach (var posEmotion in posEmotions)
            {
                var emo = Instantiate(_emotionDisplayPrefab, _emotionLayout);
                emo.gameObject.SetActive(true);
                emo.Init(posEmotion, index);
                _displayedEmotions.Add(emo);
                index++;
            }
            
            foreach (var negEmotion in negEmotions)
            {
                var emo = Instantiate(_emotionDisplayPrefab, _emotionLayout);
                emo.gameObject.SetActive(true);
                emo.Init(negEmotion, index);
                _displayedEmotions.Add(emo);
                index++;
            }
            
            _refreshLayoutCallback.Invoke();
        }
        
        private void RefreshOverallMoodDisplay()
        {
            var overallMoodPercent = _kinlingData.Mood.OverallMood / 100f;
            var targetMoodPercent = _kinlingData.Mood.MoodTarget / 100f;

            _overallMoodBarFill.fillAmount = overallMoodPercent;
            _targetIndicator.SetTargetIndicator(targetMoodPercent);
        }
    }
}
