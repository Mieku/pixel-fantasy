using System.Collections.Generic;
using Systems.Needs.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Popups.Kinling_Info_Popup
{
    public class NeedDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _needNameText;
        [SerializeField] private Image _needIcon;
        [SerializeField] private Image _barFill;
        [SerializeField] private Color _fullColour;
        [SerializeField] private Color _emptyColour;
        [SerializeField] private BarThresholdDisplay _thresholdDisplay;

        private NeedData _assignedStat;
        public NeedData Stat => _assignedStat;

        public void Init(NeedData stat, float value)
        {
            _assignedStat = stat;
            _needNameText.text = stat.DisplayName;

            if (stat.NeedIcon == null)
            {
                _needIcon.gameObject.SetActive(false);
            }
            else
            {
                _needIcon.gameObject.SetActive(true);
                _needIcon.sprite = stat.NeedIcon;
            }

            DetermineThresholds(stat);
            RefreshValue(value);
        }

        private void DetermineThresholds(NeedData stat)
        {
            List<float> allThresholds = new List<float>();
            if (stat.CriticalThreshold != null)
            {
                var critThresh = stat.CriticalThreshold.ThresholdValue;
                allThresholds.Add(critThresh);
            }
            
            foreach (var threshold in stat.Thresholds)
            {
                allThresholds.Add(threshold.ThresholdValue);
            }
            
            _thresholdDisplay.ShowThresholds(allThresholds);
        }

        public void RefreshValue(float value)
        {
            var percent = Mathf.Clamp01(value); // Just in case!
            _barFill.fillAmount = percent;
            
            // Colour?
            Color lerpedColor = Color.Lerp(_emptyColour, _fullColour, percent);
            _barFill.color = lerpedColor;
        }
    }
}
