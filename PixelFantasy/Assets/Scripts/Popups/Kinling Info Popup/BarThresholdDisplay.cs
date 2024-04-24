using System;
using System.Collections.Generic;
using Systems.Needs.Scripts;
using UnityEngine;

namespace Popups.Kinling_Info_Popup
{
    public class BarThresholdDisplay : MonoBehaviour
    {
        [SerializeField] private GameObject _indicatorPrefab;

        private readonly List<GameObject> _displayedIndicators = new List<GameObject>();
        private float _barWidth;

        public void ShowThresholds(List<NeedThreshold> needThresholds)
        {
            List<float> simpleThresholds = new List<float>();
            foreach (var needThreshold in needThresholds)
            {
                if (!needThreshold.HideThreshold)
                {
                    simpleThresholds.Add(needThreshold.ThresholdValue);
                }
            }
            
            ShowThresholds(simpleThresholds);
        }
        
        public void ShowThresholds(List<float> allThresholds)
        {
            foreach (var displayedIndicator in _displayedIndicators)
            {
                Destroy(displayedIndicator);
            }
            _displayedIndicators.Clear();
            
            foreach (var threshold in allThresholds)
            {
                var xPos = GetXPosForPercentage(threshold);
                var indicator = Instantiate(_indicatorPrefab, gameObject.transform);

                var rectTransform = indicator.GetComponent<RectTransform>();
                Vector2 anchoredPosition = rectTransform.anchoredPosition;

                // Modify the x component of the anchored position.
                anchoredPosition.x = xPos;

                // Assign the modified anchored position back to the RectTransform.
                rectTransform.anchoredPosition = anchoredPosition;
                
                indicator.SetActive(true);
                _displayedIndicators.Add(indicator);
            }
        }

        private float GetXPosForPercentage(float percentage)
        {
            var rectTransform = GetComponent<RectTransform>();
            _barWidth = rectTransform.rect.width;
            return _barWidth * percentage;
        }
    }
}
