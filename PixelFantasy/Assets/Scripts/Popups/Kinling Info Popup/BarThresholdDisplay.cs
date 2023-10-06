using System;
using System.Collections.Generic;
using UnityEngine;

namespace Popups.Kinling_Info_Popup
{
    public class BarThresholdDisplay : MonoBehaviour
    {
        [SerializeField] private GameObject _indicatorPrefab;

        private readonly List<GameObject> _displayedIndicators = new List<GameObject>();
        private float _barWidth;
        
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

                //indicator.transform.localPosition = new Vector3(xPos, 2.25f, 0f);
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
